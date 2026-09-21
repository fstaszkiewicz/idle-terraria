using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace IdleTerraria.Api.Entities
{
    [Table("player_stats")]
    public class PlayerStats
    {
        [Key]
        [Column("player_id")]
        public Guid PlayerId { get; set; }

        [Column("stat_strength")]
        public int StatStrength { get; set; }

        [Column("stat_dexterity")]
        public int StatDexterity { get; set; }

        [Column("stat_luck")]
        public int StatLuck { get; set; }

        [Column("stat_vitality")]
        public int StatVitality { get; set; }

        // Relacja zwrotna - wkazuje, do kogo należą te statystyki
        [ForeignKey("PlayerId")]
        public Player? Player { get; set; }
    }
}
