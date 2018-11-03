﻿using NWN;
using SWLOR.Game.Server.Data.Contracts;
using SWLOR.Game.Server.Data;
using SWLOR.Game.Server.Enumeration;
using SWLOR.Game.Server.GameObject;
using SWLOR.Game.Server.Perk;
using SWLOR.Game.Server.Service.Contracts;
using System;
using System.Linq;
using SWLOR.Game.Server.Data.Entity;
using PerkExecutionType = SWLOR.Game.Server.Enumeration.PerkExecutionType;

namespace SWLOR.Game.Server.Event.Delayed
{
    public class FinishAbilityUse : IRegisteredEvent
    {
        private readonly IDataContext _db;
        private readonly INWScript _;
        private readonly IAbilityService _ability;
        private readonly IColorTokenService _color;
        private readonly ICustomEffectService _customEffect;

        public FinishAbilityUse(
            IDataContext db,
            INWScript script,
            IAbilityService ability,
            IColorTokenService color,
            ICustomEffectService customEffect)
        {
            _db = db;
            _ = script;
            _ability = ability;
            _color = color;
            _customEffect = customEffect;
        }

        public bool Run(params object[] args)
        {
            NWPlayer pc = (NWPlayer)args[0];
            string spellUUID = Convert.ToString(args[1]);
            int perkID = (int)args[2];
            NWObject target = (NWObject)args[3];
            int pcPerkLevel = (int) args[4];

            Data.Entity.Perk entity = _db.Perks.Single(x => x.PerkID == perkID);
            CooldownCategory cooldown = _db.CooldownCategories.SingleOrDefault(x => x.CooldownCategoryID == entity.CooldownCategoryID);
            PerkExecutionType executionType = (PerkExecutionType) entity.ExecutionTypeID;

            return App.ResolveByInterface<IPerk, bool>("Perk." + entity.ScriptName, perk =>
            {
                if (pc.GetLocalInt(spellUUID) == (int)SpellStatusType.Interrupted || // Moved during casting
                        pc.CurrentHP < 0 || pc.IsDead) // Or is dead/dying
                {
                    pc.DeleteLocalInt(spellUUID);
                    return false;
                }

                pc.DeleteLocalInt(spellUUID);

                if (executionType == PerkExecutionType.ForceAbility ||
                    executionType == PerkExecutionType.CombatAbility ||
                    executionType == PerkExecutionType.Stance)
                {
                    perk.OnImpact(pc, target, pcPerkLevel);
                    
                    if (entity.CastAnimationID != null && entity.CastAnimationID > 0)
                    {
                        pc.AssignCommand(() =>
                        {
                            _.ActionPlayAnimation((int)entity.CastAnimationID, 1f, 1f);
                        });
                    }

                    if (target.IsNPC)
                    {
                        _ability.ApplyEnmity(pc, (target.Object), entity);
                    }
                }
                else if(executionType == PerkExecutionType.QueuedWeaponSkill)
                {
                    _ability.HandleQueueWeaponSkill(pc, entity, perk);
                }


                // Adjust FP only if spell cost > 0
                PlayerCharacter pcEntity = _db.PlayerCharacters.Single(x => x.PlayerID == pc.GlobalID);
                if (perk.FPCost(pc, entity.BaseFPCost) > 0)
                {
                    pcEntity.CurrentFP = pcEntity.CurrentFP - perk.FPCost(pc, entity.BaseFPCost);
                    _db.SaveChanges();
                    pc.SendMessage(_color.Custom("FP: " + pcEntity.CurrentFP + " / " + pcEntity.MaxFP, 32, 223, 219));

                }

                bool hasChainspell = _customEffect.DoesPCHaveCustomEffect(pc, CustomEffectType.Chainspell) &&
                    executionType == PerkExecutionType.ForceAbility;

                if(!hasChainspell)
                {
                    // Mark cooldown on category
                    _ability.ApplyCooldown(pc, cooldown, perk);
                }
                pc.IsBusy = false;
                pc.SetLocalInt(spellUUID, (int)SpellStatusType.Completed);

                return true;
            });
        }


    }
}
