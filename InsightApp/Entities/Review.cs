using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace InsightApp.Entities;

[Table("Review")]
public partial class Review
{
    [Key]
    public int ReviewId { get; set; }

    public int? GameId { get; set; }

    public int? MemberId { get; set; }

    public int StatusId { get; set; }

    [Unicode(false)]
    public string ReviewBody { get; set; } = null!;

    [Unicode(false)]
    public string? RejectReason { get; set; }

    [ForeignKey("GameId")]
    [InverseProperty("Reviews")]
    public virtual Game? Game { get; set; }

    [ForeignKey("MemberId")]
    [InverseProperty("Reviews")]
    public virtual Member? Member { get; set; }

    [ForeignKey("StatusId")]
    [InverseProperty("Reviews")]
    public virtual ReviewStatus Status { get; set; } = null!;
}
