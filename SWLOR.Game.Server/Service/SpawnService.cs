﻿using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using NWN;
using SWLOR.Game.Server.Data.Contracts;
using SWLOR.Game.Server.Data.Entities;
using SWLOR.Game.Server.GameObject;
using SWLOR.Game.Server.Processor;
using SWLOR.Game.Server.Service.Contracts;
using SWLOR.Game.Server.SpawnRule.Contracts;
using SWLOR.Game.Server.ValueObject;
using static NWN.NWScript;

namespace SWLOR.Game.Server.Service
{
    public class SpawnService : ISpawnService
    {
        private readonly INWScript _;
        private readonly IDataContext _db;
        private readonly IObjectProcessingService _processor;
        private readonly AppState _state;
        private readonly IRandomService _random;

        public SpawnService(
            INWScript script,
            IDataContext db,
            IRandomService random,
            IObjectProcessingService processor,
            AppState state)
        {
            _ = script;
            _db = db;
            _random = random;
            _processor = processor;
            _state = state;
        }

        public void OnModuleLoad()
        {
            InitializeSpawns();
            _processor.RegisterProcessingEvent<SpawnProcessor>();
        }

        private void InitializeSpawns()
        {
            foreach (var area in NWModule.Get().Areas)
            {
                var areaSpawn = new AreaSpawn();

                // Check for manually placed spawns
                NWObject obj = _.GetFirstObjectInArea(area.Object);
                while (obj.IsValid)
                {
                    bool isSpawn = obj.ObjectType == OBJECT_TYPE_WAYPOINT && obj.GetLocalInt("IS_SPAWN") == TRUE;

                    if (isSpawn)
                    {
                        int spawnType = obj.GetLocalInt("SPAWN_TYPE");
                        int objectType = spawnType == 0 || spawnType == OBJECT_TYPE_CREATURE ? OBJECT_TYPE_CREATURE : spawnType;
                        int spawnTableID = obj.GetLocalInt("SPAWN_TABLE_ID");
                        int npcGroupID = obj.GetLocalInt("SPAWN_NPC_GROUP_ID");
                        string behaviourScript = obj.GetLocalString("SPAWN_BEHAVIOUR_SCRIPT");
                        string spawnResref = obj.GetLocalString("SPAWN_RESREF");
                        float respawnTime = obj.GetLocalFloat("SPAWN_RESPAWN_SECONDS");
                        bool useResref = true;

                        // No resref specified but a table was, look in the database for a random record.
                        if (string.IsNullOrWhiteSpace(spawnResref) && spawnTableID > 0)
                        {
                            // Pick a random record.
                            var dbSpawn = _db.SpawnObjects.Where(x => x.SpawnID == spawnTableID)
                                .OrderBy(o => Guid.NewGuid()).First();
                            if (dbSpawn != null)
                            {
                                spawnResref = dbSpawn.Resref;
                                useResref = false;

                                if (dbSpawn.NPCGroupID != null && dbSpawn.NPCGroupID > 0)
                                    npcGroupID = Convert.ToInt32(dbSpawn.NPCGroupID);

                                if (!string.IsNullOrWhiteSpace(dbSpawn.BehaviourScript))
                                    behaviourScript = dbSpawn.BehaviourScript;

                            }
                        }

                        // If we found a resref, spawn the object and add it to the cache.
                        if (!string.IsNullOrWhiteSpace(spawnResref))
                        {
                            // Delay the creation so that the iteration through the area doesn't get thrown off by new entries.
                            Location location = obj.Location;
                            _.DelayCommand(0.5f, () =>
                            {
                                NWCreature creature = _.CreateObject(objectType, spawnResref, location);
                                
                                if(npcGroupID > 0)
                                    creature.SetLocalInt("NPC_GROUP", npcGroupID);

                                if(!string.IsNullOrWhiteSpace(behaviourScript) &&
                                   string.IsNullOrWhiteSpace(creature.GetLocalString("BEHAVIOUR")))
                                    creature.SetLocalString("BEHAVIOUR", behaviourScript);

                                ObjectSpawn newSpawn; 
                                if (useResref)
                                {
                                    newSpawn = new ObjectSpawn(creature, true, spawnResref, respawnTime);
                                }
                                else
                                {
                                    newSpawn = new ObjectSpawn(creature, true, spawnTableID, respawnTime);
                                }

                                if (objectType == OBJECT_TYPE_CREATURE)
                                {
                                    areaSpawn.Creatures.Add(newSpawn);
                                }
                                else if (objectType == OBJECT_TYPE_PLACEABLE)
                                {
                                    areaSpawn.Placeables.Add(newSpawn);
                                }
                            });
                            
                        }
                         
                        // Destroy the waypoint to conserve resources.
                        obj.Destroy();
                    }

                    obj = _.GetNextObjectInArea(area.Object);
                }

                _state.AreaSpawns.Add(area.Resref, areaSpawn);

                _.DelayCommand(1.0f, () =>
                {
                    SpawnResources(area, areaSpawn);
                });
            }
        }
        
        public Location GetRandomSpawnPoint(string areaResref)
        {
            return GetRandomSpawnPoint(areaResref, _db);
        }

        private Location GetRandomSpawnPoint(string areaResref, IDataContext db)
        {
            var area = NWModule.Get().Areas.Single(x => x.Resref == areaResref);
            var spawnPoint = db.AreaWalkmeshes
                .Where(x => x.Area.Resref == areaResref)
                .OrderBy(o => Guid.NewGuid()).First();

            return _.Location(area.Object,
                _.Vector((float)spawnPoint.LocationX, (float)spawnPoint.LocationY, (float)spawnPoint.LocationZ),
                _random.RandomFloat(0, 360));

        }

        private void SpawnResources(NWArea area, AreaSpawn areaSpawn)
        {
            App.Resolve<IDataContext>(db =>
            {
                var dbArea = db.Areas.Single(x => x.Resref == area.Resref);

                if (dbArea.ResourceSpawnTableID <= 0 ||
                    !dbArea.AutoSpawnResources) return;
                Spawn table = db.Spawns.Single(x => x.SpawnID == dbArea.ResourceSpawnTableID);
                var possibleSpawns = table.SpawnObjects;

                // 1024 size = 32x32
                // 256  size = 16x16
                // 64   size = 8x8
                int size = area.Width * area.Height;

                int maxSpawns = 0;
                if (size <= 12)
                {
                    maxSpawns = 2;
                }
                else if (size <= 32)
                {
                    maxSpawns = 6;
                }
                else if (size <= 64)
                {
                    maxSpawns = 10;
                }
                else if (size <= 256)
                {
                    maxSpawns = 25;
                }
                else if (size <= 512)
                {
                    maxSpawns = 40;
                }
                else if (size <= 1024)
                {
                    maxSpawns = 50;
                }

                int[] weights = new int[possibleSpawns.Count];
                for (int x = 0; x < possibleSpawns.Count; x++)
                {
                    weights[x] = possibleSpawns.ElementAt(x).Weight;
                }

                for (int x = 1; x <= maxSpawns; x++)
                {
                    int index = _random.GetRandomWeightedIndex(weights);
                    var dbSpawn = possibleSpawns.ElementAt(index);
                    Location location = GetRandomSpawnPoint(area.Resref, db);
                    NWPlaceable plc = (_.CreateObject(OBJECT_TYPE_PLACEABLE, dbSpawn.Resref, location));

                    if (dbSpawn.NPCGroupID != null && dbSpawn.NPCGroupID > 0)
                        plc.SetLocalInt("NPC_GROUP", Convert.ToInt32(dbSpawn.NPCGroupID));

                    if (!string.IsNullOrWhiteSpace(dbSpawn.BehaviourScript) &&
                       string.IsNullOrWhiteSpace(plc.GetLocalString("BEHAVIOUR")))
                        plc.SetLocalString("BEHAVIOUR", dbSpawn.BehaviourScript);

                    if (!string.IsNullOrWhiteSpace(dbSpawn.SpawnRule))
                    {
                        App.ResolveByInterface<ISpawnRule>("SpawnRule." + dbSpawn.SpawnRule, rule =>
                        {
                            rule.Run(plc);
                        });
                    }

                    ObjectSpawn spawn = new ObjectSpawn(plc, false, dbArea.ResourceSpawnTableID, 600.0f);
                    areaSpawn.Placeables.Add(spawn);
                }
            });
            
        }
        
        public IReadOnlyCollection<ObjectSpawn> GetAreaPlaceableSpawns(string areaResref)
        {
            var areaSpawn = _state.AreaSpawns[areaResref];
            return new ReadOnlyCollection<ObjectSpawn>(areaSpawn.Placeables);
        }
        public IReadOnlyCollection<ObjectSpawn> GetAreaCreatureSpawns(string areaResref)
        {
            var areaSpawn = _state.AreaSpawns[areaResref];
            return new ReadOnlyCollection<ObjectSpawn>(areaSpawn.Creatures);
        }
    }
}
