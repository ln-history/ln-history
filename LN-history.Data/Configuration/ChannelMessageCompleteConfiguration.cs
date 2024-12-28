using Dapper.FluentMap.Dommel.Mapping;
using LN_history.Data.Model;

namespace LN_history.Data.Configuration;

public class ChannelMessageCompleteConfiguration : DommelEntityMap<ChannelMessageComplete>
{
    public ChannelMessageCompleteConfiguration()
    {
        Map(x => x.Scid).ToColumn("scid");
        Map(x => x.Timestamp).ToColumn("timestamp");
        Map(x => x.MessageFlags).ToColumn("message_flags");
        Map(x => x.ChannelFlags).ToColumn("channel_flags");
        Map(x => x.CltvExpiryDelta).ToColumn("cltv_expiry_delta");
        Map(x => x.HtlcMinimumMSat).ToColumn("htlc_minimum_msat");
        Map(x => x.FeeBaseMSat).ToColumn("fee_base_msat");
        Map(x => x.FeeProportionalMillionths).ToColumn("fee_proportional_millionths");
        Map(x => x.HtlcMaximumMSat).ToColumn("htlc_maximum_msat");
        Map(x => x.ChainHash).ToColumn("chain_hash");
        Map(x => x.NodeId1).ToColumn("node_id_1");
        Map(x => x.NodeId2).ToColumn("node_id_2");
        Map(x => x.Features).ToColumn("features");
    }
}