using Dapper.FluentMap.Dommel.Mapping;
using LN_history.Data.Model;

namespace LN_history.Data.Configuration;

public class ChannelEntityMap : DommelEntityMap<Channel>
{
    public ChannelEntityMap()
    {
        ToTable("channels");
        Map(c => c.Source_Node_Id).ToColumn("source_node_id");
        Map(c => c.Target_Node_Id).ToColumn("target_node_id");
        Map(c => c.Amount_Sat).ToColumn("amount_sat");
        Map(c => c.Features).ToColumn("features");
    }
}