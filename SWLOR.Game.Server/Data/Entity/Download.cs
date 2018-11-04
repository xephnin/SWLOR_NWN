

using SWLOR.Game.Server.Data.Contracts;

namespace SWLOR.Game.Server.Data.Entity
{
    [Table("[Downloads]")]
    public class Download: IEntity
    {
        public Download()
        {
            Name = "";
            Description = "";
            Url = "";
        }

        [ExplicitKey]
        public int DownloadID { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public string Url { get; set; }
        public bool IsActive { get; set; }
    }
}
