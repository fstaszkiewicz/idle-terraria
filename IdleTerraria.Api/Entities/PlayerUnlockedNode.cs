using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace IdleTerraria.Api.Entities
{
    [Table("player_unlocked_nodes")]
    [PrimaryKey(nameof(PlayerId), nameof(NodeId), nameof(LoadoutId))]
    public class PlayerUnlockedNode
    {
        [Column("player_id")]
        public Guid PlayerId { get; set; }

        [Column("node_id")]
        public int NodeId { get; set; }

        [Column("loadout_id")]
        public Guid LoadoutId { get; set; }

        [ForeignKey(nameof(PlayerId))]
        public Player? Player { get; set; }

        [ForeignKey(nameof(NodeId))]
        public SkillTree? Node { get; set; }

        [ForeignKey(nameof(LoadoutId))]
        public Loadout? Loadout { get; set; }
    }
}