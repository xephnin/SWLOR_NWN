
using System;
using Dapper.Contrib.Extensions;
using SWLOR.Game.Server.Data.Contracts;

namespace SWLOR.Game.Server.Data.Entity
{
    [Table("SpawnObjects")]
    public class SpawnObject: IEntity
    {
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2214:DoNotCallOverridableMethodsInConstructors")]
        public SpawnObject()
        {
            SpawnRule = "";
            BehaviourScript = "";
        }

        [Key]
        public int SpawnObjectID { get; set; }
        public int SpawnID { get; set; }
        public string Resref { get; set; }
        public int Weight { get; set; }
        public string SpawnRule { get; set; }
        public int? NPCGroupID { get; set; }
        public string BehaviourScript { get; set; }
        public int DeathVFXID { get; set; }
    }
}
