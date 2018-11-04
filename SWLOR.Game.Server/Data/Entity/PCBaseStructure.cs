
using System;
using System.Collections.Generic;

using SWLOR.Game.Server.Data.Contracts;

namespace SWLOR.Game.Server.Data.Entity
{
    [Table("[PCBaseStructures]")]
    public class PCBaseStructure: IEntity
    {
        public PCBaseStructure()
        {
            CustomName = "";
        }

        [Key]
        public int PCBaseStructureID { get; set; }
        public int PCBaseID { get; set; }
        public int BaseStructureID { get; set; }
        public double LocationX { get; set; }
        public double LocationY { get; set; }
        public double LocationZ { get; set; }
        public double LocationOrientation { get; set; }
        public double Durability { get; set; }
        public int? InteriorStyleID { get; set; }
        public int? ExteriorStyleID { get; set; }
        public int? ParentPCBaseStructureID { get; set; }
        public string CustomName { get; set; }
        public int StructureBonus { get; set; }
        public DateTime? DateNextActivity { get; set; }
    }
}