using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace InsightApp.Entities;

[Table("EventAddressTable")]
public class EventAddressTable
{
    [Key]
    public int AddressId { get; set; }

    [StringLength(40)]
    [Unicode(false)]
    public string? StreetName { get; set; }

    [StringLength(10)]
    [Unicode(false)]
    public string? StreetNumber { get; set; }

    [StringLength(10)]
    [Unicode(false)]
    public string? Unit { get; set; }

    [StringLength(12)]
    [Unicode(false)]
    public string? PostalCode { get; set; }

    [StringLength(30)]
    [Unicode(false)]
    public string? City { get; set; }

    [StringLength(25)]
    [Unicode(false)]
    public string? Province { get; set; }

    [StringLength(25)]
    [Unicode(false)]
    public string? Country { get; set; }

    [InverseProperty("Address")]
    public virtual ICollection<GameEvent> GameEvents { get; set; } = new List<GameEvent>();
}

