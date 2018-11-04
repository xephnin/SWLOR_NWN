
using Dapper.Contrib.Extensions;
using SWLOR.Game.Server.Data.Contracts;

namespace SWLOR.Game.Server.Data.Entity
{
    [Table("[LootTableItems]")]
    public class LootTableItem: IEntity
    {
        public LootTableItem()
        {
            SpawnRule = "";
        }

        [Key]
        public int LootTableItemID { get; set; }
        public int LootTableID { get; set; }
        public string Resref { get; set; }
        public int MaxQuantity { get; set; }
        public byte Weight { get; set; }
        public bool IsActive { get; set; }
        public string SpawnRule { get; set; }
    }
}
