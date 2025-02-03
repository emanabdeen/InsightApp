using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace InsightApp.Entities;

[Table("Friend")]
public partial class Friend
{
    [Key]
    [Column("id")]
    public int? Id { get; set; }

    public int? MemberId { get; set; }

    public int? FriendId { get; set; }

    [ForeignKey("FriendId")]
    [InverseProperty("FriendFriendNavigations")]
    public virtual Member? FriendNavigation { get; set; }

    [ForeignKey("MemberId")]
    [InverseProperty("FriendMembers")]
    public virtual Member? Member { get; set; }
}
