namespace LN_History.Model;

/// <summary>A live subscription to <see cref="IGossipStream"/>. Dispose to unsubscribe.</summary>
public interface IGossipSubscription : IDisposable
{
    /// <summary>
    /// Frames as they arrive, until the token is cancelled or the subscription is disposed.
    /// </summary>
    IAsyncEnumerable<GossipFrame> ReadAllAsync(CancellationToken cancellationToken);

    /// <summary>
    /// Number of frames dropped for this subscriber because it could not keep up with the producer
    /// (drop-oldest). Non-zero means this consumer is too slow and lost data.
    /// </summary>
    long Dropped { get; }
}
