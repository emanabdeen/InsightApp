using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;
namespace InsightApp.Entities;

[Table("Country")]
public class Country
{
    [Key]
    [Column("id")]
    public int Id { get; set; }

    [StringLength(30)]
    public string CountryName { get; set; }

}

