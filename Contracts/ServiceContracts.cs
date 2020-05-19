using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Microsoft.ServiceFabric.Actors;
using Microsoft.ServiceFabric.Services.Remoting;

namespace Contracts
{
    public interface IGameActor : IActor, IActorEventPublisher<IGameActorEvents>
    {
        Task MakeMove(int col);

        Task<GameStateDto> GetState();
    }

    public interface IGameActorEvents : IActorEvents
    {
        void GameStateChanged(GameStateDto gameState);
    }

    public interface IGameManager : IService
    {
        Task StartSinglePlayerGame(GameDto game);
        Task StartTwoPlayerGame(GameDto game);
        Task CreateGame();
        Task JoinGame();
        Task<List<GameDto>> ListGames();

        Task<GameDto> GetGame(Guid gameId);
    }
}
