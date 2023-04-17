using System;
using System.Threading.Tasks;

namespace console_tetris
{
    class Program
    {
        //private enum InputDir { none, right, left, down }
        //static private InputDir _dirInput;

        //static private bool[][,] blocks = new bool[][,]{
        //    new bool[,] {
        //        {true, true, true, true}
        //    },
        //    new bool[,] {
        //        {true, false, false},
        //        {true, true, true}
        //    },
        //    new bool[,] {
        //        {false, false, true},
        //        {true, true, true}
        //    },
        //    new bool[,]{
        //        {true, true},
        //        {true, true}
        //    },
        //    new bool[,] {
        //        {false, true, true},
        //        {true, true, false}
        //    },
        //    new bool[,]  {
        //        {true, true, false},
        //        {false, true, true}
        //    },
        //    new bool[,]  {
        //        {false, true, false},
        //        {true, true, true}
        //    }
        //};
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

        static void Main(string[] args)
        {
            ResetValues();

            Task.Run( InputLoop );
            Task.Run( GameLoop );

            while (true) ;
        }

        static async void GameLoop() {

            while (true)
            {
                currBlockPos[1]++;

                UpdateBlock();

                await Task.Delay(updateRate);
            }
        }

        static void ResetValues()
        {
            board = new bool[20, 10];

            spawnPos.CopyTo(currBlockPos, 0);
            spawnPos.CopyTo(lastBlockPos, 0);
        }

        static async void InputLoop()
        {
            while (true)
            {
                if (!Console.KeyAvailable)
                    continue;



                switch (Console.ReadKey(true).Key)
                {
                    case ConsoleKey.LeftArrow:
                        currBlockPos[0]--;
                        break;
                    case ConsoleKey.RightArrow:
                        currBlockPos[0]++;
                        break;
                    case ConsoleKey.DownArrow:
                        currBlockPos[1]++;
                        break;
                    case ConsoleKey.UpArrow:
                        currTile = RotateBlock(in currTile);
                        break;
                }

                UpdateBlock();
            }
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


            if (CheckForColl(currBlockPos) && collAdjustMent[0] == 0 && collAdjustMent[1] == 0)
                lastBlockPos.CopyTo(currBlockPos, 0);

            while (collAdjustMent[0] != 0 || collAdjustMent[1] != 0)
            {
                currBlockPos[0] += collAdjustMent[0];
                currBlockPos[1] += collAdjustMent[1];
                CheckForColl(currBlockPos);
            }

            currBlockPos.CopyTo(lastBlockPos, 0);
            CopyValues(in currTile, out lastTile);

            PlaceBlock(currBlockPos, currTile);

            UpdateBoard(board);
        }

        static bool[,] RotateBlock(in bool[,] block)
        {
            bool[,] rotated = new bool[block.GetLength(1), block.GetLength(0)];
            for(int y = 0; y < block.GetLength(0); y++)
            {
                for(int x = 0; x < block.GetLength(1); x++)
                {
                    rotated[x, y] = block[y, x];
                }
            }
            return rotated;
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

        static bool CheckForColl(int[] pos)
        {
            collAdjustMent[0] = 0;
            collAdjustMent[1] = 0;
            bool collOccured = false;
            int tileBottom = 0;

            bool tileBottomHit;

            for(int y = 0; y < currTile.GetLength(0); y++)
            {
                for(int x = 0; x < currTile.GetLength(1); x++)
                {
                    if (!currTile[y, x])
                        continue;
                    tileBottom = y;

                    int boardY = pos[1] + y;
                    int boardX = pos[0] + x;

                    if (boardX < 0)
                        collAdjustMent[0]++;
                    else if (boardX >= board.GetLength(1))
                        collAdjustMent[0]--;

                    if (boardY >= board.GetLength(0))
                        collAdjustMent[1]--;

                    if (( collAdjustMent[0] != 0 || collAdjustMent[1] != 0 || boardY < 0 ) || (board[boardY, boardX]))
                        collOccured = true;
                }
            }
            return collOccured;
        }

        static void CopyValues<t>(in t[,] arr1, out t[,] arr2)
        {
            arr2 = new t[arr1.GetLength(0), arr1.GetLength(1)];
            for (int y = 0; y < arr1.GetLength(0); y++)
                for (int x = 0; x < arr1.GetLength(1); x++)
                    arr2[y, x] = arr1[y, x];
        }
    }
}
