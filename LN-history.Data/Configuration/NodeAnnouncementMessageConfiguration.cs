using Dapper.FluentMap.Dommel.Mapping;
using LN_history.Data.Model;

namespace LN_history.Data.Configuration;

public class NodeAnnouncementMessageConfiguration : DommelEntityMap<NodeAnnouncementMessage>
{
    public NodeAnnouncementMessageConfiguration()
    {
        Map(x => x.NodeId).ToColumn("node_id");
        Map(x => x.Features).ToColumn("features");
        Map(x => x.Timestamp).ToColumn("timestamp");
        Map(x => x.RgbColor).ToColumn("rgb_color");
        Map(x => x.Addresses).ToColumn("addresses");
    }
}