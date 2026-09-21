using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace IdleTerraria.Api.Entities
{
    [Table("skill_trees")]
    public class SkillTree
    {
        [Key]
        [Column("id")]
        public int Id { get; set; }

        [Column("tree_type")]
        [StringLength(30)]
        public string TreeType { get; set; } = string.Empty;

        [Column("node_name")]
        [StringLength(50)]
        public string NodeName { get; set; } = string.Empty;

        [Column("stat_modifier")]
        [StringLength(30)]
        public string StatModifier { get; set; } = string.Empty;

        [Column("modifier_value")]
        public decimal ModifierValue { get; set; }
    }
}