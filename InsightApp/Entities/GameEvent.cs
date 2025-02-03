using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;
using InsightApp.Attributes;

namespace InsightApp.Entities;

[Table("GameEvent")]
public partial class GameEvent
{
    [Key]
    public int EventId { get; set; }

    [StringLength(50)]
    [Unicode(false)]
    public string EventName { get; set; } = null!;

    [Unicode(false)]
    public string Details { get; set; } = null!;

    [Required]
    [FutureDate(ErrorMessage = "The Start Date must be in the future.")]
    public DateOnly StartDate { get; set; }

    public DateOnly? EndDate { get; set; }

    public TimeOnly? StartTime { get; set; }

    public TimeOnly? EndTime { get; set; }

    public double? Duration { get; set; }

    public int EvTypeId { get; set; }

    [StringLength(100)]
    [Unicode(false)]
    public string? EventLink { get; set; }

    public bool IsDeleted { get; set; } = false;

    public int? AddressId { get; set; }

    [ForeignKey("AddressId")]
    [InverseProperty("GameEvents")]
    public virtual EventAddressTable? Address { get; set; }

    [ForeignKey("EvTypeId")]
    [InverseProperty("GameEvents")]
    public virtual EventType? EvType { get; set; }

    [InverseProperty("Event")]
    public virtual ICollection<MemberEventRegist> MemberEventRegists { get; set; } = new List<MemberEventRegist>();

}
