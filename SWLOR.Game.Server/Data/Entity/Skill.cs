
using System.Collections.Generic;
using Dapper.Contrib.Extensions;
using SWLOR.Game.Server.Data.Contracts;

namespace SWLOR.Game.Server.Data.Entity
{
    [Table("Skills")]
    public partial class Skill: IEntity, ICacheable
    {
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2214:DoNotCallOverridableMethodsInConstructors")]
        public Skill()
        {
            this.Name = "";
            this.Description = "";
            this.CraftBlueprints = new HashSet<CraftBlueprint>();
            this.PCSkills = new HashSet<PCSkill>();
            this.PerkLevelSkillRequirements = new HashSet<PerkLevelSkillRequirement>();
            this.SkillXPRequirements = new HashSet<SkillXPRequirement>();
        }

        [ExplicitKey]
        public int SkillID { get; set; }
        public int SkillCategoryID { get; set; }
        public string Name { get; set; }
        public int MaxRank { get; set; }
        public bool IsActive { get; set; }
        public string Description { get; set; }
        public int Primary { get; set; }
        public int Secondary { get; set; }
        public int Tertiary { get; set; }
        public bool ContributesToSkillCap { get; set; }
    
        public virtual Attribute PrimaryAttribute { get; set; }
        public virtual Attribute SecondaryAttribute { get; set; }
        public virtual Attribute TertiaryAttribute { get; set; }
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<CraftBlueprint> CraftBlueprints { get; set; }
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<PCSkill> PCSkills { get; set; }
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<PerkLevelSkillRequirement> PerkLevelSkillRequirements { get; set; }
        public virtual SkillCategory SkillCategory { get; set; }
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<SkillXPRequirement> SkillXPRequirements { get; set; }
    }
}
