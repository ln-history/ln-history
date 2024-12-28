using System.ComponentModel.DataAnnotations.Schema;
using LN_history.Data.DataStores;
using Microsoft.EntityFrameworkCore;

namespace LN_history.Data.Model;

[Keyless]
[Table("channel_announcements")]
public class ChannelAnnouncementMessage : ChannelMessage
{
    public string Features { get; set; }
    public string NodeId1 { get; set; }
    public string NodeId2 { get; set; }
}