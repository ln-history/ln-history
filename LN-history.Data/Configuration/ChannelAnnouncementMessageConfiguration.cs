using Dapper.FluentMap.Dommel.Mapping;
using LN_history.Data.Model;

namespace LN_history.Data.Configuration;

public class ChannelAnnouncementMessageConfiguration : DommelEntityMap<ChannelAnnouncementMessage>
{
    public ChannelAnnouncementMessageConfiguration()
    {
        Map(x => x.Features).ToColumn("features");
        Map(x => x.Scid).ToColumn("scid");
        Map(x => x.ChainHash).ToColumn("chain_hash");
        Map(x => x.NodeId1).ToColumn("node_id_1");
        Map(x => x.NodeId2).ToColumn("node_id_2");
    }
}