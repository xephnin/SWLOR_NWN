
using System.Collections.Generic;
using Dapper.Contrib.Extensions;
using SWLOR.Game.Server.Data.Contracts;

namespace SWLOR.Game.Server.Data.Entity
{
    [Table("[PCBaseTypes]")]
    public class PCBaseType: IEntity
    {
        [ExplicitKey]
        public int PCBaseTypeID { get; set; }
        public string Name { get; set; }
    }
}
