using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace IdleTerraria.Api.Entities
{
    [Table("player_professions")]
    [PrimaryKey(nameof(PlayerId), nameof(ProfessionType))]
    public class PlayerProfession
    {
        [Column("player_id")]
        public Guid PlayerId { get; set; }

        [Column("profession_type")]
        [StringLength(20)]
        public string ProfessionType { get; set; } = string.Empty;

        [Column("experience")]
        public long Experience { get; set; }

        [Column("level")]
        public int Level { get; set; }

        [ForeignKey(nameof(PlayerId))]
        public Player? Player { get; set; }
    }
}