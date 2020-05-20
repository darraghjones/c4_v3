using System;
using System.Collections.Generic;
using System.Fabric;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Contracts;
using Microsoft.ServiceFabric.Data.Collections;
using Microsoft.ServiceFabric.Services.Communication.Runtime;
using Microsoft.ServiceFabric.Services.Runtime;
using Microsoft.ServiceFabric.Services.Remoting;
using Microsoft.ServiceFabric.Services.Remoting.Runtime;

namespace GameManager
{
    /// <summary>
    /// An instance of this class is created for each service replica by the Service Fabric runtime.
    /// </summary>
    internal sealed class GameManager : StatefulService, IGameManager
    {
        public GameManager(StatefulServiceContext context)
            : base(context)
        { }

        private async Task<IReliableDictionary<Guid, GameDto>> GetGames()
        {
            return await this.StateManager.GetOrAddAsync<IReliableDictionary<Guid, GameDto>>("games");
        }

        /// <summary>
        /// Optional override to create listeners (e.g., HTTP, Service Remoting, WCF, etc.) for this service replica to handle client or user requests.
        /// </summary>
        /// <remarks>
        /// For more information on service communication, see https://aka.ms/servicefabricservicecommunication
        /// </remarks>
        /// <returns>A collection of listeners.</returns>
        protected override IEnumerable<ServiceReplicaListener> CreateServiceReplicaListeners()
        {
            return this.CreateServiceRemotingReplicaListeners();
        }

        public async Task StartSinglePlayerGame(GameDto game)
        {
            game.GameType = GameTypeDto.SinglePlayer;
            var games = await GetGames();

            using (var tx = this.StateManager.CreateTransaction())
            {
                await games.TryAddAsync(tx, game.GameId, game);

                // If an exception is thrown before calling CommitAsync, the transaction aborts, all changes are 
                // discarded, and nothing is saved to the secondary replicas.
                await tx.CommitAsync();
            }
        }

        public async Task StartTwoPlayerGame(GameDto game)
        {
            game.GameType = GameTypeDto.TwoPlayer;
            var games = await GetGames();

            using (var tx = this.StateManager.CreateTransaction())
            {
                await games.TryAddAsync(tx, game.GameId, game);

                // If an exception is thrown before calling CommitAsync, the transaction aborts, all changes are 
                // discarded, and nothing is saved to the secondary replicas.
                await tx.CommitAsync();
            }
        }

        public Task CreateGame()
        {
            throw new NotImplementedException();
        }

        public Task JoinGame()
        {
            throw new NotImplementedException();
        }

        public async Task<List<GameDto>> ListGames()
        {
            var games = await GetGames();
            using var tx = this.StateManager.CreateTransaction();
            var enumerable = (await games.CreateEnumerableAsync(tx)).GetAsyncEnumerator();
            var result = new List<GameDto>();
            while (await enumerable.MoveNextAsync(CancellationToken.None))
            {
                result.Add(enumerable.Current.Value);
            }
            return result;
        }

        public async Task<GameDto> GetGame(Guid gameId)
        {
            var games = await GetGames();
            using var tx = this.StateManager.CreateTransaction();
            return (await games.TryGetValueAsync(tx, gameId)).Value;
        }
    }
}
