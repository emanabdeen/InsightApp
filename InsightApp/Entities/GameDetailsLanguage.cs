using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace InsightApp.Entities;

[Table("GameDetailsLanguage")]
public partial class GameDetailsLanguage
{
    [Key]
    [Column("id")]
    public int Id { get; set; }

    public int? GameId { get; set; }

    public int? LanguageId { get; set; }

    [ForeignKey("GameId")]
    [InverseProperty("GameDetailsLanguages")]
    public virtual Game? Game { get; set; }

    [ForeignKey("LanguageId")]
    [InverseProperty("GameDetailsLanguages")]
    public virtual LanguageTable? Language { get; set; }
}
