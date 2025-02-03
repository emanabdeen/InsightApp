using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace InsightApp.Entities;

[Table("LanguageTable")]
public partial class LanguageTable
{
    [Key]
    public int LanguageId { get; set; }

    [StringLength(30)]
    [Unicode(false)]
    public string LanguageName { get; set; } = null!;

    [InverseProperty("Language")]
    public virtual ICollection<GameDetailsLanguage> GameDetailsLanguages { get; set; } = new List<GameDetailsLanguage>();

    [InverseProperty("Language")]
    public virtual ICollection<MemberLanguagePref> MemberLanguagePrefs { get; set; } = new List<MemberLanguagePref>();
}
