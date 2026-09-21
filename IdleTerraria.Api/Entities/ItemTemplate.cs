using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace IdleTerraria.Api.Entities
{
    [Table("item_templates")]
    public class ItemTemplate
    {
        [Key]
        [Column("id")]
        public int Id { get; set; }

        [Column("category_id")]
        public int? CategoryId { get; set; }

        [Column("name")]
        [StringLength(100)]
        public string Name { get; set; } = string.Empty;

        [Column("tier")]
        public int Tier { get; set; }

        [Column("base_value")]
        public int BaseValue { get; set; }

        [Column("attack_speed")]
        public decimal AttackSpeed { get; set; }

        [ForeignKey(nameof(CategoryId))]
        public ItemCategory? Category { get; set; }
    }
}