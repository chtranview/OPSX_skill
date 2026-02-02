using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace MyWeb.Models;

[Index(nameof(CollectedTime))]
[Index(nameof(SessionId))]
[Index(nameof(Sarea))]
public class TpiUbikeAreaRecord
{
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public int Id { get; set; }

    [Required]
    public Guid SessionId { get; set; }

    [Required]
    public DateTime CollectedTime { get; set; }

    [Required]
    [MaxLength(32)]
    public string Sno { get; set; } = string.Empty;

    [Required]
    [MaxLength(200)]
    public string Sna { get; set; } = string.Empty;

    [MaxLength(200)]
    public string Snaen { get; set; } = string.Empty;

    [Required]
    [MaxLength(100)]
    public string Sarea { get; set; } = string.Empty;

    [MaxLength(100)]
    public string Sareaen { get; set; } = string.Empty;

    [MaxLength(300)]
    public string Ar { get; set; } = string.Empty;

    [MaxLength(300)]
    public string Aren { get; set; } = string.Empty;

    public int Quantity { get; set; }

    public int AvailableRentBikes { get; set; }

    public int AvailableReturnBikes { get; set; }

    [MaxLength(4)]
    public string Act { get; set; } = string.Empty;

    public double Latitude { get; set; }

    public double Longitude { get; set; }

    [MaxLength(50)]
    public string Mday { get; set; } = string.Empty;

    [MaxLength(50)]
    public string? SrcUpdateTime { get; set; }

    [MaxLength(50)]
    public string? UpdateTime { get; set; }

    [MaxLength(50)]
    public string? InfoTime { get; set; }

    [MaxLength(50)]
    public string? InfoDate { get; set; }
}
