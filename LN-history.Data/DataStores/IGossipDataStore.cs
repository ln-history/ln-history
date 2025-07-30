namespace LN_history.Data.DataStores;

public interface IGossipDataStore
{
    MemoryStream GetGossipSnapshotByTimestampJoins(DateTime timestamp);

    MemoryStream GetGossipSnapshotByTimestampOptimizedJoins(DateTime timestamp);

    MemoryStream GetGossipSnapshotByTimestampCuts(DateTime timestamp);

    MemoryStream GetGossipSnapshotDifferenceByTimestamps(DateTime startTimestamp, DateTime endTimestamp);

    MemoryStream GetNodeGossipByNodeId(string nodeId, DateTime timestamp);

    MemoryStream GetChannelGossipByScid(string scid, DateTime timestamp);
}