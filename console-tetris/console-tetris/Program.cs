using System;
using System.Threading.Tasks;
using System.Collections.Generic;

namespace console_tetris
{
    class Program
    {
        static Random rng = new Random();
        static private bool[,,] blocks = new bool[,,]
        {
            {
                {false, false, false, false },
                {false, false, false, false },
                {true, true, true, true },
                {false, false, false, false }
            },
            {
                {false, false, false, false },
                {false, true, false, false },
                {false, true, true, true },
                {false, false, false, false }
            },
            {
                {false, false, false, false },
                {false, false, true, false },
                {true, true, true, false },
                {false, false, false, false }
            },
            {
                {false, false, false, false },
                {false, true, true, false },
                {false, true, true, false },
                {false, false, false, false }
            },
            {
                {false, false, false, false },
                {false, true, true, false },
                {true, true, false, false },
                {false, false, false, false }
            },
            {
                {false, false, false, false },
                {false, true, true, false },
                {false, false, true, true },
                {false, false, false, false }
            },
            {
                {false, false, false, false },
                {false, true, false, false },
                {true, true, true, false },
                {false, false, false, false }
            }
        };


        static int updateRate = 1000;

        static bool[,] board;
        static int[] spawnPos = new int[] { 4, 0 };

        static int[] currBlockPos = new int[2];
        static int[] lastBlockPos = new int[2];
        static bool[,] currTile = new bool[,] {
                {false, false, false, false },
                {false, false, false, false },
                {true, true, true, true },
                {false, false, false, false }
            };
        static bool[,] lastTile = new bool[,] {
                {false, false, false, false },
                {false, false, false, false },
                {true, true, true, true },
                {false, false, false, false }
            };

        static int[] collAdjustMent = new int[2];

        static bool inGame = true;
        static bool inputLoopRunning = false;
        static bool gameLoopRunning = false;

        static void Main(string[] args)
        {
            string[] menuOptions = new string[] { "Play", "Exit" };
            int pointer = 0;

            while (true)
            {
                bool inMenu = true;
                while (inMenu)
                {
                    Console.Clear();
                    Console.WriteLine("__________\n| TETRIS |\n‾‾‾‾‾‾‾‾‾‾\n");
                    for (int i = 0; i < menuOptions.Length; i++)
                    {
                        if (pointer == i)
                            Console.Write(" > ");
                        Console.WriteLine(menuOptions[i]);
                    }

                    switch (Console.ReadKey(true).Key)
                    {
                        case ConsoleKey.Enter:
                        case ConsoleKey.Spacebar:
                            if (pointer == 0)
                                inMenu = false;
                            else if (pointer == 1)
                                return;
                            break;
                        case ConsoleKey.UpArrow:
                            pointer--;
                            pointer = Math.Clamp(pointer, 0, menuOptions.Length - 1);
                            break;
                        case ConsoleKey.DownArrow:
                            pointer++;
                            pointer = Math.Clamp(pointer, 0, menuOptions.Length - 1);
                            break;
                    }
                }

                inGame = true;

                ResetValues();

                Task iLoop = Task.Run(InputLoop);
                Task gLoop = Task.Run(GameLoop);

                while (inGame || inputLoopRunning || gameLoopRunning) ;
            }
        }

        static async void GameLoop() {
            gameLoopRunning = true;
            while (inGame)
            {
                currBlockPos[1]++;

                UpdateBlock();

                await Task.Delay(updateRate);
            }
            gameLoopRunning = false;
        }

        static void ResetValues()
        {
            board = new bool[20, 10];

            ResetPos();
        }
        static void ResetPos()
        {
            spawnPos.CopyTo(currBlockPos, 0);
            spawnPos.CopyTo(lastBlockPos, 0);
        }


        static async void InputLoop()
        {
            inputLoopRunning = true;
            while (inGame)
            {
                if (!Console.KeyAvailable)
                    continue;

                switch (Console.ReadKey(true).Key)
                {
                    case ConsoleKey.Spacebar:
                        currBlockPos[1]++;
                        break;
                    case ConsoleKey.LeftArrow:
                        currBlockPos[0]--;
                        break;
                    case ConsoleKey.RightArrow:
                        currBlockPos[0]++;
                        break;
                    case ConsoleKey.UpArrow:
                        currTile = RotateBlock(in currTile, true);
                        break;
                    case ConsoleKey.DownArrow:
                        currTile = RotateBlock(in currTile, false);
                        break;
                }

                UpdateBlock();
            }
            inputLoopRunning = false;
        }

        static void UpdateBoard(bool[,] board)
        {
            Console.Clear();

            for (int y = 0; y < board.GetLength(0); y++)
            {
                for(int x = 0; x < board.GetLength(1); x++)
                {
                    if (board[y, x])
                        Console.Write('0');
                    else
                        Console.Write(' ');
                }
                Console.WriteLine("|");
            }

            for (int x = 0; x < board.GetLength(1); x++)
                Console.Write("‾");

            Console.WriteLine($"\n{currBlockPos[0]} , {currBlockPos[1]}");
        }

        static void UpdateBlock()
        {
            PlaceBlock(lastBlockPos, lastTile, false);
            bool blockstopped = false;

            if (CheckForColl(currBlockPos, out blockstopped) && collAdjustMent[0] == 0 && collAdjustMent[1] == 0)
            {
                lastBlockPos.CopyTo(currBlockPos, 0);
                if (CheckForColl(currBlockPos, out blockstopped) && currBlockPos[0] == spawnPos[0] && currBlockPos[1] == spawnPos[1])
                    inGame = false;
            }


            while (collAdjustMent[0] != 0 || collAdjustMent[1] != 0)
            {
                currBlockPos[0] += collAdjustMent[0];
                currBlockPos[1] += collAdjustMent[1];
                CheckForColl(currBlockPos, out blockstopped);
            }

            currBlockPos.CopyTo(lastBlockPos, 0);
            CopyValues(in currTile, out lastTile);

            PlaceBlock(currBlockPos, currTile);

            if (blockstopped)
                SpawnBlock();

            UpdateBoard(board);
        }

        static bool[,] RotateBlock(in bool[,] block, bool clockwise)
        {
            bool[,] result = block;
            
            result = Rotate(result);

            int[] flipAxis;
            if (clockwise)
                flipAxis = new int[] { 1, 0 };
            else
                flipAxis = new int[] { 0, 1 };

            result = Flip(result, flipAxis);
            return result;
        }
        static bool[,] Rotate(in bool[,] block)
        {
            bool[,] rotated = new bool[block.GetLength(1), block.GetLength(0)];
            for (int y = 0; y < block.GetLength(0); y++)
            {
                for (int x = 0; x < block.GetLength(1); x++)
                {
                    rotated[x, y] = block[y, x];
                }
            }
            return rotated;
        }
        static bool[,] Flip(in bool[,] block, int[] axis)
        {
            bool[,] flipped = new bool[block.GetLength(0), block.GetLength(1)];
            for(int y = 0; y < block.GetLength(0); y++)
            {
                for(int x = 0; x < block.GetLength(1); x++)
                {
                    int blockY = y;
                    int blockX = x;

                    if (axis[0] != 0)
                        blockX = block.GetLength(1) - x - 1;
                    if (axis[1] != 0)
                        blockY = block.GetLength(0) - y - 1;

                    flipped[y, x] = block[blockY, blockX];
                }
            }
            return flipped;
        }


        static void PlaceBlock(int[] pos,bool[,] tile , bool place = true)
        {
            for (int y = 0; y < tile.GetLength(0); y++)
            {
                for (int x = 0; x < tile.GetLength(1); x++)
                {
                    int boardX = pos[0] + x;
                    int boardY = pos[1] + y;

                    //if (boardY < 0 || boardX < 0 || boardY >= board.GetLength(0) || boardX >= board.GetLength(1))
                    //    continue;
                    if (tile[y, x])
                        board[boardY, boardX] = place;
                }
            }
        }

        static void SpawnBlock()
        {
            ResetPos();
            CopyValues(in blocks, rng.Next(0, blocks.GetLength(0)) , out currTile);
            CopyValues(in currTile, out lastTile);
            RemoveFullLines();
        }

        static bool CheckForColl(int[] pos, out bool blockstopped)
        {
            collAdjustMent[0] = 0;
            collAdjustMent[1] = 0;
            bool collOccured = false;

            int[] tileBottom = new int[currTile.GetLength(1)];
            for (int i = 0; i < tileBottom.Length; i++)
                tileBottom[i] = -1;

            bool tileBottomHit = false;

            for(int y = 0; y < currTile.GetLength(0); y++)
            {
                for(int x = 0; x < currTile.GetLength(1); x++)
                {
                    if (!currTile[y, x])
                        continue;
                    tileBottom[x] = y;

                    int boardY = pos[1] + y;
                    int boardX = pos[0] + x;

                    if (boardX < 0)
                        collAdjustMent[0] = 1;
                    else if (boardX >= board.GetLength(1))
                        collAdjustMent[0] = -1;

                    if (boardY >= board.GetLength(0))
                        collAdjustMent[1] = -1;

                    if (( collAdjustMent[0] != 0 || collAdjustMent[1] != 0 || boardY < 0 ) || (board[boardY, boardX]))
                        collOccured = true;
                }
            }

            for(int x = 0; x < tileBottom.Length && !tileBottomHit; x++)
            {
                if (tileBottom[x] < 0)
                    continue;

                int boardX = pos[0] + x;
                int boardY = pos[1] + tileBottom[x];
                if (!currTile[tileBottom[x], x] || boardX < 0 || boardX >= board.GetLength(1)) 
                    continue;
                if (boardY + 1 >= board.GetLength(0) || board[boardY + 1, boardX])
                    tileBottomHit = true;
            }
            blockstopped = tileBottomHit;

            return collOccured;
        }

        static void RemoveFullLines()
        {
            int[] fullLines = CheckForFullLines(in board);
            for(int l = 0; l < fullLines.Length; l++)
            {
                MoveDownLinesAbove(fullLines[l], ref board);
            }
        }

        static int[] CheckForFullLines(in bool[,] checkboard) {
            List<int> fullLines = new List<int>();

            for(int y = 0; y < checkboard.GetLength(0); y++)
            {
                bool fullLine = true;
                for(int x = 0; x < checkboard.GetLength(1); x++)
                {
                    if (!checkboard[y, x])
                    {
                        fullLine = false;
                        break;
                    }
                }
                if (fullLine)
                    fullLines.Add(y);
            }

             return fullLines.ToArray();
        }

        static void MoveDownLinesAbove(int line, ref bool[,] board)
        {
            for(int y = line - 1; y >= 0; y--)
            {
                for(int x = 0; x < board.GetLength(1); x++)
                {
                    if (y + 1 >= board.GetLength(0))
                        continue;
                    board[y + 1, x] = board[y, x];
                }
            }
        }

        static void CopyValues<t>(in t[,] arr1, out t[,] arr2)
        {
            arr2 = new t[arr1.GetLength(0), arr1.GetLength(1)];
            for (int y = 0; y < arr1.GetLength(0); y++)
                for (int x = 0; x < arr1.GetLength(1); x++)
                    arr2[y, x] = arr1[y, x];
        }
        static void CopyValues<t>(in t[,,] arr1, in int index, out t[,] arr2)
        {
            arr2 = new t[arr1.GetLength(1), arr1.GetLength(2)];
            for (int y = 0; y < arr1.GetLength(1); y++)
                for (int x = 0; x < arr1.GetLength(2); x++)
                    arr2[y, x] = arr1[index,y, x];
        }
    }
}
