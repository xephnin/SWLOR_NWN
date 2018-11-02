﻿using System.Collections.Generic;
using SWLOR.Game.Server.Data;
using SWLOR.Game.Server.Enumeration;
using SWLOR.Game.Server.GameObject;

namespace SWLOR.Game.Server.Service.Contracts
{
    public interface ISkillService
    {
        int SkillCap { get; }

        float CalculateRegisteredSkillLevelAdjustedXP(float xp, int registeredLevel, int skillRank);
        List<SkillCategory> GetActiveCategories();
        int GetPCSkillRank(NWPlayer player, int skillID);
        int GetPCSkillRank(NWPlayer player, SkillType skill);
        PCSkill GetPCSkill(NWPlayer player, int skillID);
        List<PCSkill> GetAllPCSkills(NWPlayer player);
        Skill GetSkill(int skillID);
        Skill GetSkill(SkillType skillType);
        List<PCSkill> GetPCSkillsForCategory(string playerID, int skillCategoryID);
        int GetPCTotalSkillCount(string playerID);
        SkillXPRequirement GetSkillXPRequirementByRank(int skillID, int rank);
        void GiveSkillXP(NWPlayer oPC, int skillID, int xp);
        void GiveSkillXP(NWPlayer oPC, SkillType skill, int xp);
        void OnAreaExit();
        void OnCreatureDeath(NWCreature creature);
        void OnHitCastSpell(NWPlayer oPC);
        void OnModuleClientLeave();
        void OnModuleEnter();
        void OnModuleItemEquipped();
        void OnModuleItemUnequipped();
        void RegisterPCToAllCombatTargetsForSkill(NWPlayer player, SkillType skillType, NWCreature target);
        void ToggleSkillLock(string playerID, int skillID);
    }
}
