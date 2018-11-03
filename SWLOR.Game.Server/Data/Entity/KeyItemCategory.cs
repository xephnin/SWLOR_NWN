
using System.Collections.Generic;
using Dapper.Contrib.Extensions;
using SWLOR.Game.Server.Data.Contracts;

namespace SWLOR.Game.Server.Data.Entity
{
    [Table("KeyItemCategories")]
    public partial class KeyItemCategory: IEntity, ICacheable
    {
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2214:DoNotCallOverridableMethodsInConstructors")]
        public KeyItemCategory()
        {
            this.KeyItems = new HashSet<KeyItem>();
        }

        [ExplicitKey]
        public int KeyItemCategoryID { get; set; }
        public string Name { get; set; }
        public bool IsActive { get; set; }
    
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<KeyItem> KeyItems { get; set; }
    }
}
