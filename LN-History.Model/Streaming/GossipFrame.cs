namespace LN_History.Model;

/// <summary>
/// A single live gossip message pushed through the stream: its inventory <c>internal_id</c>,
/// BOLT #7 message type (256 channel_announcement / 257 node_announcement / 258 channel_update),
/// and the raw self-describing gossip envelope bytes (varint(len) ++ uint16(type) ++ payload).
/// </summary>
public sealed record GossipFrame(long InternalId, int Type, byte[] RawGossip);
