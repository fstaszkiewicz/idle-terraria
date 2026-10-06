using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace IdleTerraria.Api.Entities;

[Table("item_combat_profiles")]
public sealed class ItemCombatProfile
{
    [Key]
    [Column("item_template_id")]
    public int ItemTemplateId { get; set; }

    [Column("base_damage")]
    public int BaseDamage { get; set; }

    [Column("attack_interval_seconds")]
    public decimal AttackIntervalSeconds { get; set; }

    [Column("critical_chance")]
    public decimal CriticalChance { get; set; }

    [Column("armor_penetration")]
    public int ArmorPenetration { get; set; }

    public ItemTemplate ItemTemplate { get; set; } = null!;
}