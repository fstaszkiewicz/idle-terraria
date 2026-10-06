using System.ComponentModel.DataAnnotations.Schema;

namespace IdleTerraria.Api.Entities;

[Table("biome_mobs")]
public sealed class BiomeMob
{
    [Column("biome_id")]
    public int BiomeId { get; set; }

    [Column("mob_template_id")]
    public int MobTemplateId { get; set; }

    [Column("spawn_weight")]
    public int SpawnWeight { get; set; }

    public Biome Biome { get; set; } = null!;

    public MobTemplate MobTemplate { get; set; } = null!;
}