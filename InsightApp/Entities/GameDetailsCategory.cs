using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace InsightApp.Entities;

[Table("GameDetailsCategory")]
public partial class GameDetailsCategory
{
    [Key]
    [Column("id")]
    public int Id { get; set; }

    public int? GameId { get; set; }

    public int? CategoryId { get; set; }

    [ForeignKey("CategoryId")]
    [InverseProperty("GameDetailsCategories")]
    public virtual GameCategory? Category { get; set; }

    [ForeignKey("GameId")]
    [InverseProperty("GameDetailsCategories")]
    public virtual Game? Game { get; set; }
}
