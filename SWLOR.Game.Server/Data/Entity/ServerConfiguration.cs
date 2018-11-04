
using Dapper.Contrib.Extensions;
using SWLOR.Game.Server.Data.Contracts;

namespace SWLOR.Game.Server.Data.Entity
{
    [Table("[ServerConfiguration]")]
    public class ServerConfiguration: IEntity
    {
        public ServerConfiguration()
        {
            ServerName = "";
            MessageOfTheDay = "";
        }

        [ExplicitKey]
        public int ServerConfigurationID { get; set; }
        public string ServerName { get; set; }
        public string MessageOfTheDay { get; set; }
        public int AreaBakeStep { get; set; }
    }
}
