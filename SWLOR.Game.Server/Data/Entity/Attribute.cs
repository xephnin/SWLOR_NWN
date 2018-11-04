using System.Collections.Generic;

using SWLOR.Game.Server.Data.Contracts;

namespace SWLOR.Game.Server.Data.Entity
{
    [Table("[Attributes]")]
    public class Attribute: IEntity
    {
        public Attribute()
        {
            Name = "";
        }

        [ExplicitKey]
        public int AttributeID { get; set; }
        public int NWNValue { get; set; }
        public string Name { get; set; }
    }
}