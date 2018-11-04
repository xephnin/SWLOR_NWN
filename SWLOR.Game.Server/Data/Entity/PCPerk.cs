
using SWLOR.Game.Server.Data.Contracts;

namespace SWLOR.Game.Server.Data.Entity
{
    [Table("[PCPerks]")]
    public class PCPerk: IEntity
    {
        [Key]
        public int PCPerkID { get; set; }
        public string PlayerID { get; set; }
        public System.DateTime AcquiredDate { get; set; }
        public int PerkID { get; set; }
        public int PerkLevel { get; set; }
    }
}
