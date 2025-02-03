using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace InsightApp.Entities;

[Table("ReviewStatus")]
public partial class ReviewStatus
{
    [Key]
    public int StatusId { get; set; }

    [StringLength(15)]
    [Unicode(false)]
    public string Statusname { get; set; } = null!;

    [InverseProperty("Status")]
    public virtual ICollection<Review> Reviews { get; set; } = new List<Review>();
}
