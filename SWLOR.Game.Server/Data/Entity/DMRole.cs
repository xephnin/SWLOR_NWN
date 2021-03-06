
using System;
using System.Collections.Generic;

using SWLOR.Game.Server.Data.Contracts;

namespace SWLOR.Game.Server.Data.Entity
{
    [Table("[DMRole]")]
    public class DMRole: IEntity
    {
        [ExplicitKey]
        public int ID { get; set; }
        public string Description { get; set; }
    }
}
