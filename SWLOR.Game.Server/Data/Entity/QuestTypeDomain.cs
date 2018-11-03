
using System.Collections.Generic;
using Dapper.Contrib.Extensions;
using SWLOR.Game.Server.Data.Contracts;

namespace SWLOR.Game.Server.Data.Entity
{
    [Table("QuestTypeDomain")]
    public class QuestTypeDomain: IEntity
    {
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2214:DoNotCallOverridableMethodsInConstructors")]
        public QuestTypeDomain()
        {
        }

        [Key]
        public int QuestTypeID { get; set; }
        public string Name { get; set; }
    }
}
