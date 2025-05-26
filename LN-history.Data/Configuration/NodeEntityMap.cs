using Dapper.FluentMap.Dommel.Mapping;
using LN_history.Data.Model;

namespace LN_history.Data.Configuration;


public class NodeEntityMap: DommelEntityMap<Node> 
{
    public NodeEntityMap()
    {
        ToTable("nodes");
        Map(n => n.Node_Id).ToColumn("node_id");
        Map(n => n.Rgb_Color).ToColumn("rgb_color");
        Map(n => n.From_Timestamp).ToColumn("from_timestamp");
        Map(n => n.To_Timestamp).ToColumn("to_timestamp");
    }
}