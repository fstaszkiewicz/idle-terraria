using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace IdleTerraria.Api.Entities;

[Table("mob_loot_drops")]
public sealed class MobLootDrop
{
    [Key]
    [Column("id")]
    public int Id { get; set; }

    [Column("mob_template_id")]
    public int MobTemplateId { get; set; }

    [Column("item_template_id")]
    public int ItemTemplateId { get; set; }

    [Column("drop_chance", TypeName = "numeric(5,4)")]
    public decimal DropChance { get; set; }

    [Column("minimum_quantity")]
    public int MinimumQuantity { get; set; }

    [Column("maximum_quantity")]
    public int MaximumQuantity { get; set; }

    public MobTemplate MobTemplate { get; set; } = null!;

    public ItemTemplate ItemTemplate { get; set; } = null!;
}