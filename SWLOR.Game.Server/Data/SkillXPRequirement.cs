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
    
    public partial class SkillXPRequirement: IEntity
    {
        public int SkillXPRequirementID { get; set; }
        public int SkillID { get; set; }
        public int Rank { get; set; }
        public int XP { get; set; }
    
        public virtual Skill Skill { get; set; }
    }
}
