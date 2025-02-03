using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace InsightApp.Entities;

[Table("MemberPlatformPref")]
public partial class MemberPlatformPref
{
    [Key]
    [Column("id")]
    public int Id { get; set; }

    public int? MemberId { get; set; }

    public int? PlatformId { get; set; }

    [ForeignKey("MemberId")]
    [InverseProperty("MemberPlatformPrefs")]
    public virtual Member? Member { get; set; }

    [ForeignKey("PlatformId")]
    [InverseProperty("MemberPlatformPrefs")]
    public virtual GamePlatform? Platform { get; set; }
}
