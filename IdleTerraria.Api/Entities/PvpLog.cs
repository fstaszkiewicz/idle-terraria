using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json;

namespace IdleTerraria.Api.Entities
{
    [Table("pvp_logs")]
    public class PvpLog
    {
        [Key]
        [Column("id")]
        public Guid Id { get; set; }

        [Column("attacker_id")]
        public Guid? AttackerId { get; set; }

        [Column("defender_id")]
        public Guid? DefenderId { get; set; }

        [Column("battle_data", TypeName = "jsonb")]
        public JsonDocument? BattleData { get; set; }

        [ForeignKey(nameof(AttackerId))]
        public Player? Attacker { get; set; }

        [ForeignKey(nameof(DefenderId))]
        public Player? Defender { get; set; }
    }
}