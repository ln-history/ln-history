using System.Collections;
using LN_history.Data.Model;

namespace LN_history.Core.Comparer;

public class ChannelUpdateComparer : IEqualityComparer<ChannelUpdateMessage>
{
    public bool Equals(ChannelUpdateMessage? x, ChannelUpdateMessage? y)
    {
        if (ReferenceEquals(x, y)) return true;
        if (x is null) return false;
        if (y is null) return false;
        if (x.GetType() != y.GetType()) return false;
        return x.Scid == y.Scid && x.Timestamp.Equals(y.Timestamp) && x.MessageFlags == y.MessageFlags && x.ChannelFlags == y.ChannelFlags && x.CltvExpiryDelta == y.CltvExpiryDelta && x.HtlcMinimumMSat == y.HtlcMinimumMSat && x.HtlcMaximumMSat == y.HtlcMaximumMSat && x.FeeBaseMSat == y.FeeBaseMSat && x.FeeProportionalMillionths == y.FeeProportionalMillionths && x.ChainHash == y.ChainHash;
    }

    public int GetHashCode(ChannelUpdateMessage obj)
    {
        var hashCode = new HashCode();
        hashCode.Add(obj.Scid);
        hashCode.Add(obj.Timestamp);
        hashCode.Add(obj.MessageFlags);
        hashCode.Add(obj.ChannelFlags);
        hashCode.Add(obj.CltvExpiryDelta);
        hashCode.Add(obj.HtlcMinimumMSat);
        hashCode.Add(obj.HtlcMaximumMSat);
        hashCode.Add(obj.FeeBaseMSat);
        hashCode.Add(obj.FeeProportionalMillionths);
        hashCode.Add(obj.ChainHash);
        return hashCode.ToHashCode();
    }
}