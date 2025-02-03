using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace InsightApp.Entities;

[Table("AddressTable")]
public partial class AddressTable
{
    [Key]
    public int? AddressId { get; set; }

    public int? MemberId { get; set; }

    [StringLength(40)]
    [Unicode(false)]
    [Required(ErrorMessage = "Please enter a Street Name.")]
    public string StreetName { get; set; }

    [StringLength(10)]
    [Unicode(false)]
    [Required(ErrorMessage = "Please enter a Street Number.")]
    public string StreetNumber { get; set; }

	[StringLength(10)]
	[Unicode(false)]
	public string? Unit { get; set; } = "";

    [StringLength(12)]
    [Unicode(false)]
    [Required(ErrorMessage = "Please enter a PostalCode.")]
    public string PostalCode { get; set; }

    [StringLength(30)]
    [Unicode(false)]
    [Required(ErrorMessage = "Please enter a City.")]
    public string City { get; set; }

    [StringLength(25)]
    [Unicode(false)]
    [Required(ErrorMessage = "Please enter a Province.")]
    public string Province { get; set; }

    [StringLength(25)]
    [Unicode(false)]
    [Required(ErrorMessage = "Please enter a Country.")]
    public string Country { get; set; }

    public bool? IsShipping { get; set; }

    [Unicode(false)]
    public string? DelivaryInstructions { get; set; }

    [ForeignKey("MemberId")]
    [InverseProperty("AddressTables")]
    public virtual Member? Member { get; set; }
}
