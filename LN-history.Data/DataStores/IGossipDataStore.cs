namespace LN_history.Data.DataStores;

public interface IGossipDataStore
{
    MemoryStream GetGossipSnapshotByTimestamp(DateTime timestamp);

    MemoryStream GetGossipSnapshotDifferenceByTimestamps(DateTime startTimestamp, DateTime endTimestamp);

    MemoryStream GetNodeGossipByNodeId(string nodeId, DateTime timestamp);

    MemoryStream GetChannelGossipByScid(string scid, DateTime timestamp);
}