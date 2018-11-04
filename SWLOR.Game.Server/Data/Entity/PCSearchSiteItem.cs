

using SWLOR.Game.Server.Data.Contracts;

namespace SWLOR.Game.Server.Data.Entity
{
    [Table("[PCSearchSiteItems]")]
    public class PCSearchSiteItem: IEntity
    {
        [Key]
        public long PCSearchSiteItemID { get; set; }
        public string PlayerID { get; set; }
        public int SearchSiteID { get; set; }
        public string SearchItem { get; set; }
    }
}
