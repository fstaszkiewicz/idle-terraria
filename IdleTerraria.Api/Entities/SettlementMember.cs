using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace IdleTerraria.Api.Entities
{
    [Table("settlement_members")]
    public class SettlementMember
    {
        [Key]
        [Column("player_id")]
        public Guid PlayerId { get; set; } // Dowódca jest kluczem głównym

        [Column("settlement_id")]
        public Guid? SettlementId { get; set; }

        [Column("is_registered_for_war")]
        public bool IsRegisteredForWar { get; set; }

        [Column("contribution_gold")]
        public long ContributionGold { get; set; }

        [ForeignKey(nameof(PlayerId))]
        public Player? Player { get; set; }

        [ForeignKey(nameof(SettlementId))]
        public Settlement? Settlement { get; set; }
    }
}