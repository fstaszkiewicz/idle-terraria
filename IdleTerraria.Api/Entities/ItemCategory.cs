using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace IdleTerraria.Api.Entities
{
    [Table("item_categories")]
    public class ItemCategory
    {
        [Key]
        [Column("id")]
        public int Id { get; set; }

        [Column("name")]
        [StringLength(50)]
        public string Name { get; set; } = string.Empty;

        [Column("equipable")]
        public bool Equipable { get; set; }
    }
}