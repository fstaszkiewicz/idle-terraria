using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace IdleTerraria.Api.Entities
{
    [Table("loadouts")]
    public class Loadout
    {
        [Key]
        [Column("id")]
        public Guid Id { get; set; }

        [Column("player_id")]
        public Guid? PlayerId { get; set; }

        [Column("role_type")]
        [StringLength(10)]
        public string RoleType { get; set; } = string.Empty; // PVE, PVP, BOSS

        [Column("weapon_inv_id")]
        public Guid? WeaponInvId { get; set; }

        [Column("armor_inv_id")]
        public Guid? ArmorInvId { get; set; }

        [Column("pet_inv_id")]
        public Guid? PetInvId { get; set; }

        [ForeignKey(nameof(PlayerId))]
        public Player? Player { get; set; }

        [ForeignKey(nameof(WeaponInvId))]
        public Inventory? Weapon { get; set; }

        [ForeignKey(nameof(ArmorInvId))]
        public Inventory? Armor { get; set; }

        [ForeignKey(nameof(PetInvId))]
        public Inventory? Pet { get; set; }
    }
}