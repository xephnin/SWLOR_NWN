﻿using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using NWN;
using SWLOR.Game.Server.Data.Contracts;
using SWLOR.Game.Server.Data;
using SWLOR.Game.Server.Data.Entity;
using SWLOR.Game.Server.GameObject;

using SWLOR.Game.Server.Service.Contracts;
using SWLOR.Game.Server.ValueObject;

namespace SWLOR.Game.Server.Service
{
    public class ExaminationService: IExaminationService
    {
        private readonly IDataService _data;
        private readonly INWScript _;
        private readonly IColorTokenService _color;
        private readonly ISkillService _skill;

        public ExaminationService(
            IDataService data, 
            INWScript script, 
            IColorTokenService color,
            ISkillService skill)
        {
            _data = data;
            _ = script;
            _color = color;
            _skill = skill;
        }

        public bool OnModuleExamine(NWPlayer examiner, NWObject target)
        {
            string backupDescription = target.GetLocalString("BACKUP_DESCRIPTION");

            if (!string.IsNullOrWhiteSpace(backupDescription))
            {
                target.UnidentifiedDescription = backupDescription;
            }

            if (!examiner.IsDM || !target.IsPlayer || target.IsDM) return false;

            backupDescription = target.IdentifiedDescription;
            target.SetLocalString("BACKUP_DESCRIPTION", backupDescription);
            
            PlayerCharacter playerEntity = _data.PlayerCharacters.Single(x => x.PlayerID == target.GlobalID);
            NWArea area = NWModule.Get().Areas.Single(x => x.Resref == playerEntity.RespawnAreaResref);
            string respawnAreaName = area.Name;

            StringBuilder description =
                new StringBuilder(
                    _color.Green("ID: ") + target.GlobalID + "\n" +
                    _color.Green("Character Name: ") + target.Name + "\n" +
                    _color.Green("Respawn Area: ") + respawnAreaName + "\n" +
                    _color.Green("Skill Points: ") + playerEntity.TotalSPAcquired + " (Unallocated: " + playerEntity.UnallocatedSP + ")" + "\n" +
                    _color.Green("FP: ") + playerEntity.CurrentFP + " / " + playerEntity.MaxFP + "\n" +
                    _color.Green("Skill Levels: ") + "\n\n");

            List<CachedPCSkill> pcSkills = _skill.GetAllPCSkills(target.Object);

            foreach (CachedPCSkill pcSkill in pcSkills)
            {
                CachedSkill skill = _skill.GetSkill(pcSkill.SkillID);
                description.Append(skill.Name).Append(" rank ").Append(pcSkill.Rank).AppendLine();
            }

            description.Append("\n\n").Append(_color.Green("Perks: ")).Append("\n\n");

            List<PCPerkHeader> pcPerks = _data.StoredProcedure<PCPerkHeader>("GetPCPerksForMenuHeader",
                new SqlParameter("PlayerID", target.GlobalID));

            foreach (PCPerkHeader perk in pcPerks)
            {
                description.Append(perk.Name).Append(" Lvl. ").Append(perk.Level).AppendLine();
            }
            
            description.Append("\n\n").Append(_color.Green("Description: \n\n")).Append(backupDescription).AppendLine();
            target.UnidentifiedDescription = description.ToString();
            
            return true;
        }

    }
}
