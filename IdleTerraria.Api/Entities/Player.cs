using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace IdleTerraria.Api.Entities
{
    [Table("players")]
    public class Player
    {
        [Key]
        [Column("id")]
        public Guid Id { get; set; }

        [Column("username")]
        [StringLength(20)]
        public string Username { get; set; } = string.Empty;

        [Column("level")]
        public int Level { get; set; }

        [Column("experience")]
        public long Experience { get; set; }

        [Column("gold")]
        public long Gold { get; set; }

        [Column("stardust")]
        public int Stardust { get; set; }

        [Column("energy")]
        public int Energy { get; set; }

        [Column("arena_elo")]
        public int ArenaElo { get; set; } = 1200;

        [Column("current_biome_id")]
        public int? CurrentBiomeId { get; set; }

        [Column("stats_bought_n")]
        public int StatsBoughtN { get; set; }

        
        // Relacja 1 do 1
        public PlayerStats? Stats { get; set; }
    }
}
