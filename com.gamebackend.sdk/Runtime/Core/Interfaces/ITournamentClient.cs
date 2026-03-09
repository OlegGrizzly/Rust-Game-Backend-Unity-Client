using System.Collections.Generic;
using System.Threading;
using Cysharp.Threading.Tasks;
using GameBackend.Core.Models;

namespace GameBackend.Core.Interfaces
{
    public interface ITournamentClient
    {
        UniTask<IEnumerable<Tournament>> ListTournamentsAsync(int limit = 50, CancellationToken ct = default);
        UniTask<TournamentDetails> GetTournamentAsync(string tournamentId, CancellationToken ct = default);
        UniTask JoinTournamentAsync(string tournamentId, CancellationToken ct = default);
        UniTask<TournamentRecord> WriteTournamentRecordAsync(string tournamentId,
            long score, long subscore = 0, Dictionary<string, object> metadata = null, CancellationToken ct = default);
    }
}
