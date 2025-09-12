using Fiap.CRM.Infra.CrossCutting.Http.FCG.Models;
using Refit;

namespace Fiap.CRM.Infra.CrossCutting.Http.FCG
{
    public interface IFCGService
    {
        [Get("/api/v1/games")]
        Task<ApiEnvelope<List<GameResponse>>> GetAllGamesAsync();
    }
}
