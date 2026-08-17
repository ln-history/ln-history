namespace LN_History.Model;

/// <summary>
/// In-process fan-out of live gossip. A single producer (the database LISTEN listener) publishes
/// frames; any number of subscribers (WebSocket connections) read them. Each subscriber has its
/// own bounded buffer with drop-oldest semantics, so a slow reader is never able to block the
/// producer or stall the other subscribers.
/// </summary>
public interface IGossipStream
{
    /// <summary>Publish a frame to every current subscriber whose type filter accepts it.</summary>
    void Publish(GossipFrame frame);

    /// <summary>
    /// Register a subscriber. The returned subscription starts buffering immediately (before the
    /// first read), so no frame published after this call is missed. Dispose to unsubscribe.
    /// </summary>
    /// <param name="types">BOLT #7 message types to receive, or null/empty for all types.</param>
    IGossipSubscription Subscribe(IReadOnlyCollection<int>? types);
}
