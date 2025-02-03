using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace InsightApp.Entities;

[Table("WishList")]
public partial class WishList
{
    [Key]
    [Column("id")]
    public int Id { get; set; }

    public int? MemberId { get; set; }

    public int? GameId { get; set; }

    [ForeignKey("GameId")]
    [InverseProperty("WishLists")]
    public virtual Game? Game { get; set; }

    [ForeignKey("MemberId")]
    [InverseProperty("WishLists")]
    public virtual Member? Member { get; set; }
}
