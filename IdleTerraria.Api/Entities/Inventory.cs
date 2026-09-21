using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace IdleTerraria.Api.Entities
{
    [Table("inventory")]
    public class Inventory
    {
        [Key]
        [Column("id")]
        public Guid Id { get; set; }

        [Column("player_id")]
        public Guid? PlayerId { get; set; }

        [Column("template_id")]
        public int? TemplateId { get; set; }

        [Column("prefix")]
        [StringLength(30)]
        public string? Prefix { get; set; }

        [Column("upgrade_level")]
        public int UpgradeLevel { get; set; }

        [Column("quantity")]
        public int Quantity { get; set; }

        [ForeignKey(nameof(PlayerId))]
        public Player? Player { get; set; }

        [ForeignKey(nameof(TemplateId))]
        public ItemTemplate? Template { get; set; }
    }
}