using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace InsightApp.Entities;

[Table("OrderTable")]
public partial class OrderTable
{
    [Key]
    public int OrderId { get; set; }

    public DateOnly? OrderDate { get; set; }

    public TimeOnly? OrderTime { get; set; }

    public double? TotalPayment { get; set; }

    public bool OrderFulfilled { get; set; }

    public int MemberId { get; set; }

    [ForeignKey("MemberId")]
    [InverseProperty("OrderTables")]
    public virtual Member Member { get; set; } = null!;

    [InverseProperty("Order")]
    public virtual ICollection<OrderItem> OrderItems { get; set; } = new List<OrderItem>();
}
