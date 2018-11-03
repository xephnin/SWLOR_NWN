using System;
using System.Collections.Generic;
using Dapper.Contrib.Extensions;
using SWLOR.Game.Server.Data.Contracts;

namespace SWLOR.Game.Server.Data.Entity
{
    [Table("Quests")]
    public class Quest: IEntity, ICacheable
    {
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2214:DoNotCallOverridableMethodsInConstructors")]
        public Quest()
        {
            this.OnAcceptRule = "";
            this.OnAdvanceRule = "";
            this.OnCompleteRule = "";
            this.OnKillTargetRule = "";
            this.OnAcceptArgs = "";
            this.OnAdvanceArgs = "";
            this.OnCompleteArgs = "";
            this.OnKillTargetArgs = "";
            this.PCQuestStatus = new HashSet<PCQuestStatus>();
            this.PerkLevelQuestRequirements = new HashSet<PerkLevelQuestRequirement>();
            this.QuestKillTargetLists = new HashSet<QuestKillTargetList>();
            this.QuestPrerequisites = new HashSet<QuestPrerequisite>();
            this.RequiredQuests = new HashSet<QuestPrerequisite>();
            this.QuestRequiredItemLists = new HashSet<QuestRequiredItemList>();
            this.QuestRequiredKeyItemLists = new HashSet<QuestRequiredKeyItemList>();
            this.QuestRewardItems = new HashSet<QuestRewardItem>();
            this.QuestStates = new HashSet<QuestState>();
        }

        [ExplicitKey]
        public int QuestID { get; set; }
        public string Name { get; set; }
        public string JournalTag { get; set; }
        public int FameRegionID { get; set; }
        public int RequiredFameAmount { get; set; }
        public bool AllowRewardSelection { get; set; }
        public int RewardGold { get; set; }
        public Nullable<int> RewardKeyItemID { get; set; }
        public int RewardFame { get; set; }
        public bool IsRepeatable { get; set; }
        public string MapNoteTag { get; set; }
        public Nullable<int> StartKeyItemID { get; set; }
        public bool RemoveStartKeyItemAfterCompletion { get; set; }
        public string OnAcceptRule { get; set; }
        public string OnAdvanceRule { get; set; }
        public string OnCompleteRule { get; set; }
        public string OnKillTargetRule { get; set; }
        public string OnAcceptArgs { get; set; }
        public string OnAdvanceArgs { get; set; }
        public string OnCompleteArgs { get; set; }
        public string OnKillTargetArgs { get; set; }
    
        public virtual FameRegion FameRegion { get; set; }
        public virtual KeyItem RewardKeyItem { get; set; }
        public virtual KeyItem TemporaryKeyItem { get; set; }
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<PCQuestStatus> PCQuestStatus { get; set; }
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<PerkLevelQuestRequirement> PerkLevelQuestRequirements { get; set; }
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<QuestKillTargetList> QuestKillTargetLists { get; set; }
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<QuestPrerequisite> QuestPrerequisites { get; set; }
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<QuestPrerequisite> RequiredQuests { get; set; }
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<QuestRequiredItemList> QuestRequiredItemLists { get; set; }
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<QuestRequiredKeyItemList> QuestRequiredKeyItemLists { get; set; }
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<QuestRewardItem> QuestRewardItems { get; set; }
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<QuestState> QuestStates { get; set; }
    }
}
