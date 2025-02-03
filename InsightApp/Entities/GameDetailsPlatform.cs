using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace InsightApp.Entities;

[Table("GameDetailsPlatform")]
public partial class GameDetailsPlatform
{
    [Key]
    [Column("id")]
    public int Id { get; set; }

    public int? GameId { get; set; }

    public int? PlatformId { get; set; }

    [ForeignKey("GameId")]
    [InverseProperty("GameDetailsPlatforms")]
    public virtual Game? Game { get; set; }

    [ForeignKey("PlatformId")]
    [InverseProperty("GameDetailsPlatforms")]
    public virtual GamePlatform? Platform { get; set; }
}
