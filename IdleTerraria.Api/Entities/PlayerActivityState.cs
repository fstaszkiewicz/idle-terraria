using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace IdleTerraria.Api.Entities
{
    [Table("player_activity_states")]
    public class PlayerActivityState
    {
        [Key]
        [Column("player_id")]
        public Guid PlayerId { get; set; }

        [Column("activity_type")]
        [StringLength(20)]
        public string ActivityType { get; set; } = string.Empty;

        [Column("biome_id")]
        public int? BiomeId { get; set; }

        [Column("started_at")]
        public DateTime StartedAt { get; set; }

        [Column("last_batch_calculated_at")]
        public DateTime LastBatchCalculatedAt { get; set; }

        [ForeignKey(nameof(PlayerId))]
        public Player? Player { get; set; }
    }
}