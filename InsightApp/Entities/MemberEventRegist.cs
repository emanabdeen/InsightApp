using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace InsightApp.Entities;

[Table("MemberEventRegist")]
public partial class MemberEventRegist
{
    [Key]
    [Column("id")]
    public int Id { get; set; }

    public int? MemberId { get; set; }

    public int? EventId { get; set; }

    [ForeignKey("EventId")]
    [InverseProperty("MemberEventRegists")]
    public virtual GameEvent? Event { get; set; }

    [ForeignKey("MemberId")]
    [InverseProperty("MemberEventRegists")]
    public virtual Member? Member { get; set; }
}
