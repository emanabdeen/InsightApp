using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace InsightApp.Entities;

[Table("EventType")]
public partial class EventType
{
    [Key]
    public int EvTypeId { get; set; }

    [StringLength(15)]
    [Unicode(false)]
    public string EvTypeName { get; set; } = null!;

    [InverseProperty("EvType")]
    public virtual ICollection<GameEvent> GameEvents { get; set; } = new List<GameEvent>();
}
