﻿using System;
using System.Collections.Generic;
using System.Linq;
using NWN;
using SWLOR.Game.Server.CustomEffect.Contracts;
using SWLOR.Game.Server.Data.Contracts;
using SWLOR.Game.Server.Data;
using SWLOR.Game.Server.Data.Entity;
using SWLOR.Game.Server.Enumeration;
using SWLOR.Game.Server.GameObject;
using SWLOR.Game.Server.NWNX.Contracts;
using SWLOR.Game.Server.Processor.Contracts;
using SWLOR.Game.Server.Service.Contracts;
using SWLOR.Game.Server.ValueObject;

namespace SWLOR.Game.Server.Processor
{
    public class CustomEffectProcessor: IEventProcessor
    {
        private readonly IDataContext _db;
        private readonly IErrorService _error;
        private readonly INWScript _;
        private readonly AppCache _cache;
        private readonly INWNXObject _nwnxObject;
        private readonly ICustomEffectService _customEffect;

        public CustomEffectProcessor(IDataContext db,
            IErrorService error,
            INWScript script,
            AppCache cache,
            INWNXObject nwnxObject,
            ICustomEffectService customEffect)
        {
            _db = db;
            _error = error;
            _ = script;
            _cache = cache;
            _nwnxObject = nwnxObject;
            _customEffect = customEffect;
        }


        public void Run(object[] args)
        {
            ProcessPCCustomEffects();
            ProcessNPCCustomEffects();
            ClearRemovedPCEffects();
        }
        private void ProcessPCCustomEffects()
        {
            foreach (var player in NWModule.Get().Players)
            {
                if (!player.IsInitializedAsPlayer) continue; // Ignored to prevent a timing issue where new characters would be included in this processing.

                List<PCCustomEffect> effects = _db.PCCustomEffects.Where(x => x.PlayerID == player.GlobalID && 
                                                                              x.CustomEffect.CustomEffectCategoryID != (int)CustomEffectCategoryType.Stance).ToList();

                foreach (var effect in effects)
                {
                    if (player.CurrentHP <= -11)
                    {
                        _customEffect.RemovePCCustomEffect(player, effect.CustomEffectID);
                        return;
                    }

                    PCCustomEffect result = RunPCCustomEffectProcess(player, effect);
                    if (result == null)
                    {
                        string message = effect.CustomEffect.WornOffMessage;
                        string scriptHandler = effect.CustomEffect.ScriptHandler;
                        player.SendMessage(message);
                        player.DeleteLocalInt("CUSTOM_EFFECT_ACTIVE_" + effect.CustomEffectID);
                        _db.PCCustomEffects.Remove(effect);
                        _db.SaveChanges();

                        App.ResolveByInterface<ICustomEffect>("CustomEffect." + scriptHandler, (handler) =>
                        {
                            handler?.WearOff(null, player, effect.EffectiveLevel, effect.Data);
                        });
                    }
                }
            }

            _db.SaveChanges();
        }

        private void ProcessNPCCustomEffects()
        {
            for (int index = _cache.NPCEffects.Count - 1; index >= 0; index--)
            {
                var entry = _cache.NPCEffects.ElementAt(index);
                CasterSpellVO casterModel = entry.Key;
                _cache.NPCEffects[entry.Key] = entry.Value - 1;
                Data.Entity.CustomEffect entity = _db.CustomEffects.Single(x => x.CustomEffectID == casterModel.CustomEffectID);
                App.ResolveByInterface<ICustomEffect>("CustomEffect." + entity.ScriptHandler, (handler) =>
                {
                    try
                    {
                        handler?.Tick(casterModel.Caster, casterModel.Target, _cache.NPCEffects[entry.Key], casterModel.EffectiveLevel, casterModel.Data);
                    }
                    catch (Exception ex)
                    {
                        _error.LogError(ex, "CustomEffectService processor was unable to run specific effect script: " + entity.ScriptHandler);
                    }


                    // Kill the effect if it has expired, target is invalid, or target is dead.
                    if (entry.Value <= 0 ||
                        !casterModel.Target.IsValid ||
                        casterModel.Target.CurrentHP <= -11)
                    {
                        handler?.WearOff(casterModel.Caster, casterModel.Target, casterModel.EffectiveLevel, casterModel.Data);

                        if (casterModel.Caster.IsValid && casterModel.Caster.IsPlayer)
                        {
                            casterModel.Caster.SendMessage("Your effect '" + casterModel.EffectName + "' has worn off of " + casterModel.Target.Name);
                        }

                        casterModel.Target.DeleteLocalInt("CUSTOM_EFFECT_ACTIVE_" + casterModel.CustomEffectID);

                        _cache.NPCEffects.Remove(entry.Key);
                    }
                });

            }
        }

        private PCCustomEffect RunPCCustomEffectProcess(NWPlayer oPC, PCCustomEffect effect)
        {
            NWCreature caster = oPC;
            if (!string.IsNullOrWhiteSpace(effect.CasterNWNObjectID))
            {
                var obj = _nwnxObject.StringToObject(effect.CasterNWNObjectID);
                if (obj.IsValid)
                {
                    caster = obj.Object;
                }
            }

            if(effect.Ticks > 0)
                effect.Ticks = effect.Ticks - 1;

            if (effect.Ticks == 0) return null;
            
            if (!string.IsNullOrWhiteSpace(effect.CustomEffect.ContinueMessage) &&
                effect.Ticks % 6 == 0) // Only show the message once every six seconds
            {
                oPC.SendMessage(effect.CustomEffect.ContinueMessage);
            }

            App.ResolveByInterface<ICustomEffect>("CustomEffect." + effect.CustomEffect.ScriptHandler, (handler) =>
            {
                handler?.Tick(caster, oPC, effect.Ticks, effect.EffectiveLevel, effect.Data);
            });
            
            return effect;
        }

        private void ClearRemovedPCEffects()
        {
            var records = _db.PCCustomEffects.Where(x => _cache.PCEffectsForRemoval.Contains(x.PCCustomEffectID)).ToList();
            _db.PCCustomEffects.RemoveRange(records);
            _db.SaveChanges();
            _cache.PCEffectsForRemoval.Clear();
        }
    }
}
