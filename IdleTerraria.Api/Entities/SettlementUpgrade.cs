using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace IdleTerraria.Api.Entities
{
    [Table("settlement_upgrades")]
    [PrimaryKey(nameof(SettlementId), nameof(UpgradeType))]
    public class SettlementUpgrade
    {
        [Column("settlement_id")]
        public Guid SettlementId { get; set; }

        [Column("upgrade_type")]
        [StringLength(50)]
        public string UpgradeType { get; set; } = string.Empty;

        [Column("level")]
        public int Level { get; set; }

        [ForeignKey(nameof(SettlementId))]
        public Settlement? Settlement { get; set; }
    }
}