using LN_history.Data.DataStores;
using LN_History.Model;

namespace LN_history.Core.Services;

public class GossipStreamService : IGossipStreamService
{
    private readonly IGossipStream _stream;
    private readonly IGossipRawStore _rawStore;

    public GossipStreamService(IGossipStream stream, IGossipRawStore rawStore)
    {
        _stream = stream;
        _rawStore = rawStore;
    }

    public IGossipSubscription Subscribe(IReadOnlyCollection<int>? types) => _stream.Subscribe(types);

    public Task<IReadOnlyList<GossipFrame>> GetBackfillAsync(
        long sinceInternalId, IReadOnlyCollection<int>? types, int max, CancellationToken cancellationToken) =>
        _rawStore.GetSinceAsync(sinceInternalId, types, max, cancellationToken);
}
