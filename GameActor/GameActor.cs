using System;
using System.Threading;
using System.Threading.Tasks;
using Contracts;
using Domain;
using Microsoft.ServiceFabric.Actors;
using Microsoft.ServiceFabric.Actors.Runtime;

namespace GameActor
{
    
    [StatePersistence(StatePersistence.Persisted)]
    internal class GameActor : Actor, IGameActor
    {
        private readonly IGameManager gameManager;
        private IActorTimer timer;

        private IActorReminder reminder;

        public GameActor(ActorService actorService, ActorId actorId, IGameManager gameManager)
            : base(actorService, actorId)
        {
            this.gameManager = gameManager;
        }


        protected override async Task OnActivateAsync()
        {
            ActorEventSource.Current.ActorMessage(this, "Actor activated.");

            await StateManager.TryAddStateAsync("gameState", new GameStateDto());

            var gameInfo = await gameManager.GetGame(this.Id.GetGuidId());

            await StateManager.TryAddStateAsync("gameInfo", gameInfo);

        }

        protected override Task OnDeactivateAsync()
        {
            if (timer != null)
            {
                UnregisterTimer(timer);
            }

            return base.OnDeactivateAsync();
        }

        public async Task MakeMove(int col)
        {
            var state = await GetState();

            var engine = new C4Engine(state.Board, state.Score, state.NumPieces);
            engine.MakeMove(state.CurrentPlayer, col);

            await UpdateState(engine, state);

            var gameInfo = await StateManager.GetStateAsync<GameDto>("gameInfo");

            if (gameInfo.GameType == GameTypeDto.SinglePlayer)
            {
                timer = RegisterTimer(ComputerMove, null, TimeSpan.Zero, Timeout.InfiniteTimeSpan);
            }
        }

        public Task<GameStateDto> GetState()
        {
            return StateManager.GetStateAsync<GameStateDto>("gameState");
        }

        private async Task UpdateState(C4Engine engine, GameStateDto currentState)
        {
            var state = new GameStateDto
            {
                Board = engine.Board,
                Score = engine.Score,
                NumPieces = engine.NumPieces,
                Winner = engine.IsWinner(),
                Host = Environment.MachineName,
                LastUpdated = DateTimeOffset.UtcNow,
                CurrentPlayer = currentState.CurrentPlayer == 1 ? 2 : 1,
            };

            if (state.Winner != 0)
            {
                for (var k = 0; k < 69; k++)
                    if (state.Score[state.Winner - 1][k] == 16)
                    {
                        for (var i = 0; i < 6; i++)
                        for (var j = 0; j < 7; j++)
                            if (C4Engine.Map[i][j][k] == 1)
                            {
                                state.WinningGrid[i][j] = state.Winner;
                            }
                    }
            }

            await StateManager.SetStateAsync("gameState", state);

            GetEvent<IGameActorEvents>().GameStateChanged(state);
        }

        private async Task ComputerMove(object _)
        {
            var state = await GetState();

            var engine = new C4Engine(state.Board, state.Score, state.NumPieces);
            engine.ComputerMove(state.CurrentPlayer, 5);

            await UpdateState(engine, state);

        }
    }
}
