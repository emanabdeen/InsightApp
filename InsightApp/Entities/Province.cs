using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;
namespace InsightApp.Entities;

[Table("Province")]
public class Province
{
    [Key]
    [Column("id")]
    public int Id { get; set; }

    [StringLength(30)]
    public string ProvinceName { get; set; }

}

