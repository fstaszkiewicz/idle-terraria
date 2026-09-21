using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace IdleTerraria.Api.Entities
{
    [Table("wandering_shop_stock")]
    public class WanderingShopStock
    {
        [Key]
        [Column("id")]
        public Guid Id { get; set; }

        [Column("template_id")]
        public int? TemplateId { get; set; }

        [Column("price_in_gold")]
        public long PriceInGold { get; set; }

        [Column("price_modifier_pct")]
        public decimal PriceModifierPct { get; set; }

        [Column("expires_at")]
        public DateTime ExpiresAt { get; set; }

        [ForeignKey(nameof(TemplateId))]
        public ItemTemplate? Template { get; set; }
    }
}