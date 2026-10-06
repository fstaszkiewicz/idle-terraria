using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace IdleTerraria.Api.Entities;

[Table("item_templates")]
public sealed class ItemTemplate
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

    [Column("category_id")]
    public int CategoryId { get; set; }

    [Column("tier")]
    public int Tier { get; set; }

    [Column("base_value")]
    public long BaseValue { get; set; }

    [Column("max_stack_size")]
    public int MaxStackSize { get; set; } = 1;

    [Column("is_tradable")]
    public bool IsTradable { get; set; } = true;

    public ItemCategory Category { get; set; } = null!;

    public ItemCombatProfile? CombatProfile { get; set; }

    public ICollection<MobLootDrop> LootDrops { get; set; } = [];
}