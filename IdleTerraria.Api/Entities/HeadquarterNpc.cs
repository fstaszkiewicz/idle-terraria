using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace IdleTerraria.Api.Entities
{
    [Table("headquarter_npcs")]
    public class HeadquarterNpc
    {
        [Key]
        [Column("id")]
        public int Id { get; set; }

        [Column("player_id")]
        public Guid? PlayerId { get; set; }

        [Column("npc_name")]
        [StringLength(50)]
        public string NpcName { get; set; } = string.Empty;

        [Column("morale_percentage")]
        public int MoralePercentage { get; set; }

        [Column("is_unlocked")]
        public bool IsUnlocked { get; set; }

        [ForeignKey(nameof(PlayerId))]
        public Player? Player { get; set; }
    }
}