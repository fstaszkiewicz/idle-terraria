using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace IdleTerraria.Api.Entities
{
    [Table("settlements")]
    public class Settlement
    {
        [Key]
        [Column("id")]
        public Guid Id { get; set; }

        [Column("name")]
        [StringLength(50)]
        public string Name { get; set; } = string.Empty;

        [Column("leader_id")]
        public Guid? LeaderId { get; set; }

        [Column("glory_emblems")]
        public int GloryEmblems { get; set; }

        [Column("gold_vault")]
        public long GoldVault { get; set; }

        [Column("elo_rating")]
        public int EloRating { get; set; }

        [ForeignKey(nameof(LeaderId))]
        public Player? Leader { get; set; }
    }
}