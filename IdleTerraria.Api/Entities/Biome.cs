using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace IdleTerraria.Api.Entities;

[Table("biomes")]
public sealed class Biome
{
    [Key]
    [Column("id")]
    public int Id { get; set; }

    [Required]
    [StringLength(50)]
    [Column("code")]
    public string Code { get; set; } = string.Empty;

    [Required]
    [StringLength(100)]
    [Column("name")]
    public string Name { get; set; } = string.Empty;

    [Column("minimum_level")]
    public int MinimumLevel { get; set; }

    [Column("maximum_level")]
    public int? MaximumLevel { get; set; }

    [Column("is_active")]
    public bool IsActive { get; set; } = true;

    public ICollection<BiomeMob> MobPool { get; set; } = [];
}