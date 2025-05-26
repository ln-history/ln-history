using LN_history.Data.DataStores;
using LN_history.Data.Model;

namespace LN_history.Core.Services;

public class NetworkSnapshotService: INetworkSnapshotService
{
    private readonly INetworkSnapshotDataStore _networkSnapshotDataStore;

    public NetworkSnapshotService(INetworkSnapshotDataStore networkSnapshotDataStore)
    {
        _networkSnapshotDataStore = networkSnapshotDataStore;
    }

    
    public async Task<NetworkSnapshot> GetSnapshotAsync(DateTime timestamp, CancellationToken cancellationToken)
    {
        var (nodes, channels) = await _networkSnapshotDataStore.GetSnapshotAsync(timestamp, cancellationToken);

        return new NetworkSnapshot
        {
            Timestamp = timestamp,
            Nodes = nodes.Select(n => new NodeSnapshot
            {
                NodeId = BitConverter.ToString(n.Node_Id).Replace("-", ""),
                Alias = n.Alias,
                RgbColor = $"#{BitConverter.ToString(n.Rgb_Color).Replace("-", "").ToLower()}",
                FromTimestamp = n.From_Timestamp,
                ToTimestamp = n.To_Timestamp
            }).ToList(),

            Channels = channels.Select(c => new ChannelSnapshot
            {
                Scid = c.Scid,
                SourceNodeId = c.Source_Node_Id,
                TargetNodeId = c.Target_Node_Id,
                AmountSat = c.Amount_Sat,
                Features = BitConverter.ToString(c.Features).Replace("-", ""),
                FromTimestamp = c.Channel_From,
                ToTimestamp = c.Channel_To,
                Policy = new ChannelPolicy
                {
                    FromTimestamp = c.Update_From,
                    ToTimestamp = c.Update_To,
                    HtlcMinimumMsat = c.Htlc_Minimum_Msat,
                    HtlcMaximumMsat = c.Htlc_Maximum_Msat,
                    FeeBaseMsat = c.Fee_Base_Msat,
                    FeePpmMsat = c.Fee_Ppm_Msat,
                    CltvExpiryDelta = c.Cltv_Expiry_Delta
                }
            }).ToList()
        };
    }
}