using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace IdleTerraria.Api.Entities;

[Table("mob_templates")]
public sealed class MobTemplate
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

    [Column("level")]
    public int Level { get; set; }

    [Column("base_experience")]
    public int BaseExperience { get; set; }

    [Column("minimum_gold")]
    public long MinimumGold { get; set; }

    [Column("maximum_gold")]
    public long MaximumGold { get; set; }

    public ICollection<BiomeMob> Biomes { get; set; } = [];

    public ICollection<MobLootDrop> LootDrops { get; set; } = [];
}