using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using System.Text;

namespace Contracts
{
    public class GameDto
    {
        [DataMember]
        public Guid GameId { get; set; }
        [DataMember]
        public PlayerDto Yellow { get; set; }
        [DataMember]
        public PlayerDto Red { get; set; }

        public GameTypeDto GameType { get; set; }
    }

    [DataContract]
    public class PlayerDto
    {
        [DataMember]
        public string Name { get; set; }
    }

    [DataContract]
    public enum GameTypeDto
    {
        [EnumMember]
        SinglePlayer,
        [EnumMember]
        TwoPlayer,
    }

    [DataContract]
    public class GameStateDto
    {
        public GameStateDto()
        {
            int i, j;
            // Initialize the board
            Board = new int[6][];
            WinningGrid = new int[6][];
            for (i = 0; i < 6; i++)
            {
                Board[i] = new int[7];
                WinningGrid[i] = new int[7];
                for (j = 0; j < 7; j++)
                {
                    Board[i][j] = 0;
                }
            }

            // Initialize the scores
            Score = new int[2][];
            for (i = 0; i < 2; i++)
            {
                Score[i] = new int[69];
                for (j = 0; j < 69; j++)
                {
                    Score[i][j] = 1;
                }
            }
        }

        [DataMember]
        public int[][] Board { get; set; }

        [DataMember]
        public int[][] WinningGrid { get; set; }

        [DataMember] public int[][] Score { get; set; }

        [DataMember] public int CurrentPlayer { get; set; } = 1;

        [DataMember]
        public int Winner { get; set; }

        [DataMember]
        public int NumPieces { get; set; }

        [DataMember]
        public string Host { get; set; } = Environment.MachineName;

        [DataMember]
        public DateTimeOffset LastUpdated { get; set; } = DateTimeOffset.UtcNow;
    }

    [DataContract]
    public enum GamePhaseDto
    {
        [EnumMember]
        InProgress,
        [EnumMember]
        WaitingForOpponent,
        [EnumMember]
        Finished,
    }
}
