using System;
using static System.Console;

namespace Domain
{
    public class C4Engine
    {
        public int[][] Board => board.DeepClone();
        public int[][] Score => score.DeepClone();
        public int NumPieces => numPieces;

        private int[][] board;
        private int[][] score;
        public static readonly int[][][] Map;
        private int numPieces;
        private readonly Random random = new Random();

        public C4Engine()
        {
            ResetEngine();
        }

        private int Rand()
        {
            return random.Next();
        }

        private C4Engine Copy()
        {
            return new C4Engine(board.DeepClone(), score.DeepClone(), numPieces);
        }

        public C4Engine(int[][] board, int[][] score, int numPieces)
        {
            this.numPieces = numPieces;
            this.board = board;
            this.score = score;
        }

        static C4Engine()
        {
            int i, j, k, count = 0;

            // Initialize the map
            Map = new int[6][][];
            for (i = 0; i < 6; i++)
            {
                Map[i] = new int[7][];
                for (j = 0; j < 7; j++)
                {
                    Map[i][j] = new int[69];
                    for (k = 0; k < 69; k++)
                        Map[i][j][k] = 0;
                }
            }

            // Set the horizontal win positions
            for (i = 0; i < 6; i++)
                for (j = 0; j < 4; j++)
                {
                    for (k = 0; k < 4; k++)
                        Map[i][j + k][count] = 1;
                    count++;
                }

            // Set the vertical win positions
            for (i = 0; i < 7; i++)
                for (j = 0; j < 3; j++)
                {
                    for (k = 0; k < 4; k++)
                        Map[j + k][i][count] = 1;
                    count++;
                }

            // Set the forward diagonal win positions
            for (i = 0; i < 3; i++)
                for (j = 0; j < 4; j++)
                {
                    for (k = 0; k < 4; k++)
                        Map[i + k][j + k][count] = 1;
                    count++;
                }

            // Set the backward diagonal win positions
            for (i = 0; i < 3; i++)
                for (j = 6; j >= 3; j--)
                {
                    for (k = 0; k < 4; k++)
                        Map[i + k][j - k][count] = 1;
                    count++;
                }
        }


        public void ResetEngine()
        {
            int i, j;
            // Initialize the board
            board = new int[6][];
            for (i = 0; i < 6; i++)
            {
                board[i] = new int[7];
                for (j = 0; j < 7; j++)
                    board[i][j] = 0;
            }

            // Initialize the scores
            score = new int[2][];
            for (i = 0; i < 2; i++)
            {
                score[i] = new int[69];
                for (j = 0; j < 69; j++)
                    score[i][j] = 1;
            }

            numPieces = 0;
        }

        int DropPiece(int player, int col)
        {
            int row;
            for (row = 0; (row < 6) && (board[row][col] != 0); row++) { }

            // The column is full
            if (row == 6)
                return -1;

            // The move is OK
            board[row][col] = player;
            numPieces++;
            UpdateScore(player, row, col);
            return row;
        }

        public int IsWinner()
        {
            for (int i = 0; i < 69; i++)
            {
                if (score[0][i] == 16)
                    return 1;
                if (score[1][i] == 16)
                    return 2;
            }
            //Game is not yet won
            return 0;
        }

        int CalcScore(int player)
        {
            int s = 0;
            for (int i = 0; i < 69; i++)
                s += score[player - 1][i];
            return s;
        }

        void UpdateScore(int player, int row, int col)
        {
            for (int i = 0; i < 69; i++)
                if (Map[row][col][i] == 1)
                {
                    score[player - 1][i] <<= 1;
                    score[OtherPlayer(player) - 1][i] = 0;
                }
        }

        static int OtherPlayer(int player)
        {
            return ((player == 1) ? 2 : 1);
        }

        public bool IsTie()
        {
            return (numPieces == 42);
        }

        public int MakeMove(int player, int col)
        {
            if (col < 0 || col > 6)
                return -1;
            int row = DropPiece(player, col);
            if (row < 0)
                return -1;
            return row;
        }

        public int ComputerMove(int player, int level)
        {
            int bestCol = -1;
            int best = -30000;

            // Simulate a drop in each of the columns
            for (int currentCol = 0; currentCol < 7; currentCol++)
            {
                C4Engine tempState = Copy();

                // If column is full, try a different column
                if (tempState.DropPiece(player, currentCol) < 0)
                    continue;

                // If this drop wins the game, then go here
                if (tempState.IsWinner() == player)
                {
                    bestCol = currentCol;
                    break;
                }

                // Otherwise, evaluate this move
                var goodness = tempState.Evaluate(player, level, 1);

                // If this move looks better than previous moves, remember it
                if (goodness > best)
                {
                    best = goodness;
                    bestCol = currentCol;
                }

                //If the move is equally good, make a random decision
                if (goodness == best)
                    if (Rand() % 2 == 0)
                        bestCol = currentCol;
            }

            // Drop the piece in the best column
            if (bestCol >= 0)
            {
                var row = DropPiece(player, bestCol);
                if (row >= 0)
                    return bestCol;
            }
            return -1;
        }

        int Evaluate(int player, int level, int depth)
        {
            int best = -30000;

            if (depth <= level)
            {
                for (int currentCol = 0; currentCol < 7; currentCol++)
                {
                    C4Engine tempState = Copy();

                    if (tempState.DropPiece(OtherPlayer(player), currentCol) < 0)
                        continue;

                    int goodness;
                    if (tempState.IsWinner() == OtherPlayer(player))
                        goodness = 25000 - depth;
                    else
                        goodness = tempState.Evaluate(OtherPlayer(player), level, depth + 1);

                    if (goodness > best)
                        best = goodness;

                }

                // What's good for the other player is bad for this one
                return -best;
            }
            return (CalcScore(player) - CalcScore(OtherPlayer(player)));
        }

        public void Display()
        {
            int row, col;
            WriteLine();
            for (col = 1; col <= 7; col++)
                Write(col + " ");
            WriteLine();

            for (row = 5; row >= 0; row--)
            {
                for (col = 0; col < 7; col++)
                    switch (board[row][col])
                    {
                        case 0:
                            Write(". ");
                            break;
                        case 1:
                            Write("X ");
                            break;
                        case 2:
                            Write("O ");
                            break;
                    }
                WriteLine();
            }
        }
    }
}