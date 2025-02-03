using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace InsightApp.Entities;

[Table("OrderItem")]
public partial class OrderItem
{
    [Key]
    [Column("id")]
    public int Id { get; set; }

    public int? OrderId { get; set; }

    public int? GameId { get; set; }
    public int? MemberId { get; set; }

    public int IsPhysical { get; set; }

    public int? Quantity { get; set; } = 1;

    [ForeignKey("GameId")]
    [InverseProperty("OrderItems")]
    public virtual Game? Game { get; set; }

    [ForeignKey("OrderId")]
    [InverseProperty("OrderItems")]
    public virtual OrderTable? Order { get; set; }

    [ForeignKey("MemberId")]
    [InverseProperty("OrderItems")]
    public virtual Member? Member { get; set; }
}
