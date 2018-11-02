﻿using System.Collections.Generic;
using NWN;
using SWLOR.Game.Server.Data;
using SWLOR.Game.Server.GameObject;

using SWLOR.Game.Server.Service.Contracts;
using SWLOR.Game.Server.ValueObject;
using SWLOR.Game.Server.ValueObject.Dialog;

namespace SWLOR.Game.Server.Conversation
{
    public class XPTome: ConversationBase
    {
        private class Model
        {
            public NWItem Item { get; set; }
            public int SkillID { get; set; }
        }

        private readonly ISkillService _skill;

        public XPTome(
            INWScript script, 
            IDialogService dialog,
            ISkillService skill) 
            : base(script, dialog)
        {
            _skill = skill;
        }

        public override PlayerDialog SetUp(NWPlayer player)
        {
            PlayerDialog dialog = new PlayerDialog("CategoryPage");

            DialogPage categoryPage = new DialogPage(
                "This tome holds techniques lost to the ages. Select a skill to earn experience in that art."
            );

            DialogPage skillListPage = new DialogPage(
                "This tome holds techniques lost to the ages. Select a skill to earn experience in that art."
            );

            DialogPage confirmPage = new DialogPage(
                "<SET LATER>",
                "Select this skill."
            );

            dialog.AddPage("CategoryPage", categoryPage);
            dialog.AddPage("SkillListPage", skillListPage);
            dialog.AddPage("ConfirmPage", confirmPage);

            return dialog;
        }

        public override void Initialize()
        {
            List<CachedSkillCategory> categories = _skill.GetActiveCategories();

            foreach (CachedSkillCategory category in categories)
            {
                AddResponseToPage("CategoryPage", category.Name, true, category.SkillCategoryID);
            }

            Model vm = GetDialogCustomData<Model>();
            vm.Item = (GetPC().GetLocalObject("XP_TOME_OBJECT"));
            SetDialogCustomData(vm);
        }

        public override void DoAction(NWPlayer player, string pageName, int responseID)
        {
            switch (pageName)
            {
                case "CategoryPage":
                    HandleCategoryPageResponse(responseID);
                    break;
                case "SkillListPage":
                    HandleSkillListResponse(responseID);
                    break;
                case "ConfirmPage":
                    HandleConfirmPageResponse();
                    break;
            }
        }

        public override void Back(NWPlayer player, string beforeMovePage, string afterMovePage)
        {
        }

        private void HandleCategoryPageResponse(int responseID)
        {
            DialogResponse response = GetResponseByID("CategoryPage", responseID);
            int categoryID = (int)response.CustomData;
            
            List<CachedPCSkill> pcSkills = _skill.GetPCSkillsForCategory(GetPC().GlobalID, categoryID);

            ClearPageResponses("SkillListPage");
            foreach (CachedPCSkill pcSkill in pcSkills)
            {
                CachedSkill skill = _skill.GetSkill(pcSkill.SkillID);
                AddResponseToPage("SkillListPage", skill.Name, true, pcSkill.SkillID);
            }

            ChangePage("SkillListPage");
        }

        private void HandleSkillListResponse(int responseID)
        {
            DialogResponse response = GetResponseByID("SkillListPage", responseID);
            int skillID = (int)response.CustomData;
            CachedSkill skill = _skill.GetSkill(skillID);
            string header = "Are you sure you want to improve your " + skill.Name + " skill?";
            SetPageHeader("ConfirmPage", header);

            Model vm = GetDialogCustomData<Model>();
            vm.SkillID = skillID;
            SetDialogCustomData(vm);
            ChangePage("ConfirmPage");
        }

        private void HandleConfirmPageResponse()
        {
            Model vm = GetDialogCustomData<Model>();

            if (vm.Item != null && vm.Item.IsValid)
            {
                int xp = vm.Item.GetLocalInt("XP_TOME_AMOUNT");
                _skill.GiveSkillXP(GetPC(), vm.SkillID, xp);
                vm.Item.Destroy();
            }

            EndConversation();
        }

        public override void EndDialog()
        {
        }
    }
}
