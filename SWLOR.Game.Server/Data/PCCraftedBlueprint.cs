//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated from a template.
//
//     Manual changes to this file may cause unexpected behavior in your application.
//     Manual changes to this file will be overwritten if the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

namespace SWLOR.Game.Server.Data
{
    using System;
    using System.Collections.Generic;
    
    using SWLOR.Game.Server.Data.Contracts;
    
    public partial class PCCraftedBlueprint: IEntity
    {
        public int PCCraftedBlueprintID { get; set; }
        public string PlayerID { get; set; }
        public long CraftBlueprintID { get; set; }
        public System.DateTime DateFirstCrafted { get; set; }
    
        public virtual CraftBlueprint CraftBlueprint { get; set; }
        public virtual PlayerCharacter PlayerCharacter { get; set; }
    }
}
