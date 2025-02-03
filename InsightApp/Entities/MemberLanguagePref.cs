using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace InsightApp.Entities;

[Table("MemberLanguagePref")]
public partial class MemberLanguagePref
{
    [Key]
    [Column("id")]
    public int Id { get; set; }

    public int? MemberId { get; set; }

    public int? LanguageId { get; set; }

    [ForeignKey("LanguageId")]
    [InverseProperty("MemberLanguagePrefs")]
    public virtual LanguageTable? Language { get; set; }

    [ForeignKey("MemberId")]
    [InverseProperty("MemberLanguagePrefs")]
    public virtual Member? Member { get; set; }
}
