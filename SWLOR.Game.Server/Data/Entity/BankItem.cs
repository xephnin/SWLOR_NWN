
using System;
using Dapper.Contrib.Extensions;
using SWLOR.Game.Server.Data.Contracts;

namespace SWLOR.Game.Server.Data.Entity
{
    [Table("BankItems")]
    public class BankItem: IEntity
    {
        [Key]
        public int BankItemID { get; set; }
        public int BankID { get; set; }
        public string PlayerID { get; set; }
        public string ItemID { get; set; }
        public string ItemName { get; set; }
        public string ItemTag { get; set; }
        public string ItemResref { get; set; }
        public string ItemObject { get; set; }
        public DateTime DateStored { get; set; }
    }
}
