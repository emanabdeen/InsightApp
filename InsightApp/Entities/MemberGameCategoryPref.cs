using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace InsightApp.Entities;

[Table("MemberGameCategoryPref")]
public partial class MemberGameCategoryPref
{
    [Key]
    [Column("id")]
    public int Id { get; set; }

    public int? MemberId { get; set; }

    public int? CategoryId { get; set; }

    [ForeignKey("CategoryId")]
    [InverseProperty("MemberGameCategoryPrefs")]
    public virtual GameCategory? Category { get; set; }

    [ForeignKey("MemberId")]
    [InverseProperty("MemberGameCategoryPrefs")]
    public virtual Member? Member { get; set; }
}
