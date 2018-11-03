﻿using Caliburn.Micro;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using SWLOR.Game.Server.Data;
using SWLOR.Game.Server.Extension;
using SWLOR.Game.Server.Service.Contracts;
using SWLOR.Tools.Editor.Messages;
using SWLOR.Tools.Editor.ViewModels.Contracts;
using SWLOR.Tools.Editor.ViewModels.Data;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Windows.Forms;
using Screen = Caliburn.Micro.Screen;

namespace SWLOR.Tools.Editor.ViewModels
{
    public class DataSyncViewModel :
        Screen,
        IDataSyncViewModel,
        IHandle<DatabaseConnectionSucceeded>,
        IHandle<DatabaseConnectionFailed>,
        IHandle<DatabaseConnecting>
    {
        private IDatabaseConnectionViewModel _dbConnectionVm;
        private readonly IWindowManager _windowManager;
        private readonly IErrorViewModel _errorVM;
        private readonly IYesNoViewModel _yesNo;
        private readonly IEventAggregator _eventAggregator;
        private readonly IDataService _data;
        private readonly BackgroundWorker _worker;

        public DataSyncViewModel(
            IDatabaseConnectionViewModel dbConnectionVm,
            IWindowManager windowManager,
            IErrorViewModel errorVM,
            IYesNoViewModel yesNo,
            IEventAggregator eventAggregator,
            IDataService data)
        {
            _dbConnectionVm = dbConnectionVm;
            _windowManager = windowManager;
            _errorVM = errorVM;
            _yesNo = yesNo;
            _eventAggregator = eventAggregator;
            _data = data;

            _worker = new BackgroundWorker();
            _worker.DoWork += PerformDataSyncAsync;
            _worker.RunWorkerCompleted += DataSyncCompleted;
            _worker.WorkerReportsProgress = true;
            _worker.ProgressChanged += SyncProgressChanged;

            ProgressMax = 100;

            IsCancelEnabled = false;
            DatabaseControlsEnabled = true;
            _eventAggregator.Subscribe(this);
        }


        // WARNING: This is on the worker thread, not the UI thread.
        private int _progress = 0;
        // END WARNING

        public IDatabaseConnectionViewModel DatabaseConnectionVM
        {
            get => _dbConnectionVm;
            set
            {
                _dbConnectionVm = value;
                NotifyOfPropertyChange(() => DatabaseConnectionVM);
            }
        }

        private int _progressMax;

        public int ProgressMax
        {
            get => _progressMax;
            set
            {
                _progressMax = value;
                NotifyOfPropertyChange(() => ProgressMax);
            }
        }

        private int _currentProgress;

        public int CurrentProgress
        {
            get => _currentProgress;
            set
            {
                _currentProgress = value;
                NotifyOfPropertyChange(() => CurrentProgress);
            }
        }

        private bool _isCancelEnabled;

        public bool IsCancelEnabled
        {
            get => _isCancelEnabled;
            set
            {
                _isCancelEnabled = value;
                NotifyOfPropertyChange(() => IsCancelEnabled);
            }
        }

        private bool _databaseControlsEnabled;

        public bool DatabaseControlsEnabled
        {
            get => _databaseControlsEnabled;
            set
            {
                _databaseControlsEnabled = value;
                NotifyOfPropertyChange(() => DatabaseControlsEnabled);
            }
        }

        private bool _syncEnabled;

        public bool SyncEnabled
        {
            get => _syncEnabled;
            set
            {
                _syncEnabled = value;
                NotifyOfPropertyChange(() => SyncEnabled);
            }
        }

        public void Sync()
        {
            _yesNo.Prompt = "WARNING: This will overwrite any local changes you have made. It's highly suggested you back up your data files or push them to the server before syncing. Are you sure you want to continue?";
            _windowManager.ShowDialog(_yesNo);

            if (_yesNo.Result == DialogResult.Yes)
            {
                IsCancelEnabled = false;
                SyncEnabled = false;
                _worker.RunWorkerAsync();
            }
        }

        private void PerformDataSyncAsync(object sender, DoWorkEventArgs e)
        {
            _progress = 0;

            WriteDataFileAsync(_data.GetAll<ApartmentBuilding>());
            WriteDataFileAsync(_data.GetAll<BaseStructure>());
            WriteDataFileAsync(_data.GetAll<BuildingStyle>());
            WriteDataFileAsync(_data.GetAll<CooldownCategory>());
            WriteDataFileAsync(_data.GetAll<CraftBlueprint>());
            WriteDataFileAsync(_data.GetAll<CraftBlueprintCategory>());
            WriteDataFileAsync(_data.GetAll<CraftDevice>());
            WriteDataFileAsync(_data.GetAll<CustomEffect>());
            WriteDataFileAsync(_data.GetAll<Download>());
            WriteDataFileAsync(_data.GetAll<FameRegion>());
            WriteDataFileAsync(_data.GetAll<GameTopic>());
            WriteDataFileAsync(_data.GetAll<GameTopicCategory>());
            WriteDataFileAsync(_data.GetAll<KeyItem>());
            WriteDataFileAsync(_data.GetAll<KeyItemCategory>());

            var ltDictionary = new Dictionary<int, LootTable>();
            var lootTables = _data.StoredProcedure<LootTable, LootTableItem, LootTable>(
                "GetLootTables",
                (table, item) =>
                {
                    if (!ltDictionary.TryGetValue(table.LootTableID, out var lt))
                    {
                        lt = table;
                        lt.LootTableItems = new List<LootTableItem>();
                        ltDictionary.Add(lt.LootTableID, lt);
                    }

                    lt.LootTableItems.Add(item);
                    return lt;
                }, "LootTableItemID")
                .Distinct()
                .ToList();
            WriteDataFileAsync(lootTables);
            ltDictionary.Clear();

            WriteDataFileAsync(_data.GetAll<Mod>());
            WriteDataFileAsync(_data.GetAll<NPCGroup>());

            var perkDictionary = new Dictionary<int, Perk>();
            var perkLevelDictionary = new Dictionary<int, PerkLevel>();
            var perkLevelSkillRequirementDictionary = new Dictionary<int, PerkLevelSkillRequirement>();
            var perkLevelQuestRequirementDictionary = new Dictionary<int, PerkLevelQuestRequirement>();

            var perks = _data.StoredProcedure<Perk, PerkLevel, PerkLevelSkillRequirement, PerkLevelQuestRequirement, Perk>(
                "GetPerks",
                (perk, perkLevel, skillReq, questReq) =>
                {
                    // Process the perk
                    if(!perkDictionary.TryGetValue(perk.PerkID, out var p))
                    {
                        p = perk;
                        p.PerkLevels = new List<PerkLevel>();
                        perkDictionary.Add(p.PerkID, p);
                    }

                    // Process perk levels
                    if(!perkLevelDictionary.TryGetValue(perkLevel.PerkLevelID, out var pl))
                    {
                        pl = perkLevel;
                        pl.PerkLevelSkillRequirements = new List<PerkLevelSkillRequirement>();
                        pl.PerkLevelQuestRequirements = new List<PerkLevelQuestRequirement>();
                        
                        // Add to both the result dictionary as well as the perk object itself.
                        perkLevelDictionary.Add(pl.PerkLevelID, pl);
                    }

                    if (skillReq != null)
                    {
                        // Process the perk skill requirements
                        if (!perkLevelSkillRequirementDictionary.TryGetValue(skillReq.PerkLevelSkillRequirementID, out var sr))
                        {
                            sr = skillReq;

                            // Add to both the result dictionary as well as the perk level object itself.
                            perkLevelSkillRequirementDictionary.Add(sr.PerkLevelSkillRequirementID, sr);
                            pl.PerkLevelSkillRequirements.Add(sr);
                        }
                    }

                    if(questReq != null)
                    {
                        // Process the perk quest requirements
                        if (!perkLevelQuestRequirementDictionary.TryGetValue(questReq.PerkLevelQuestRequirementID, out var qr))
                        {
                            qr = questReq;

                            // Add to both the result dictionary as well as the perk level object itself.
                            perkLevelQuestRequirementDictionary.Add(qr.PerkLevelQuestRequirementID, qr);
                            pl.PerkLevelQuestRequirements.Add(qr);
                        }
                    }

                    p.PerkLevels.Add(pl);
                    return p;
                }, "PerkLevelID,PerkLevelSkillRequirementID,PerkLevelQuestRequirementID")
                .Distinct()
                .ToList();

            WriteDataFileAsync(perks);

            //var perks = db.Perks
            //    .Include(i => i.PerkLevels.Select(x => x.PerkLevelQuestRequirements))
            //    .Include(i => i.PerkLevels.Select(x => x.PerkLevelSkillRequirements))
            //    .ToList();
            //WriteDataFileAsync(perks);

            //WriteDataFileAsync(_data.GetAll<PerkCategory>());
            //WriteDataFileAsync(_data.GetAll<Plant>());

            //var quests = db.Quests
            //    .Include(i => i.QuestStates.Select(x => x.QuestKillTargetLists))
            //    .Include(i => i.QuestStates.Select(x => x.QuestKillTargetLists))
            //    .Include(i => i.QuestPrerequisites)
            //    .Include(i => i.QuestStates.Select(x => x.QuestRequiredItemLists))
            //    .Include(i => i.QuestStates.Select(x => x.QuestRequiredKeyItemLists))
            //    .Include(i => i.QuestRewardItems)
            //    .ToList();
            //WriteDataFileAsync(quests);

            //var skills = db.Skills
            //    .Include(i => i.SkillXPRequirements)
            //    .ToList();
            //WriteDataFileAsync(skills);

            WriteDataFileAsync(_data.GetAll<SkillCategory>());

            //WriteDataFileAsync(db.Spawns.Include(i => i.SpawnObjects).ToList());

        }

        private void SyncProgressChanged(object sender, ProgressChangedEventArgs e)
        {
            CurrentProgress = e.ProgressPercentage;
        }

        private void DataSyncCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            if (e.Error != null)
            {
                _errorVM.ErrorDetails = e.Error.ToMessageAndCompleteStacktrace();
                _windowManager.ShowDialog(_errorVM);
            }
            else
            {
                TryClose();
            }

            CurrentProgress = 0;
            SyncEnabled = true;
            IsCancelEnabled = true;
        }

        private void WriteDataFileAsync<T>(IEnumerable<T> set)
        {
            string Folder = typeof(T).Name;
            string path = "./Data/" + Folder + "/";
            string[] files = Directory.GetFiles(path);

            foreach (var file in files)
            {
                File.Delete(file);
            }

            foreach (var record in set)
            {
                JObject jObj = JObject.FromObject(record);
                jObj.Add(nameof(DBObjectViewModelBase.InternalEditorID), Guid.NewGuid().ToString());
                string fileName = Guid.NewGuid().ToString();
                string json = JsonConvert.SerializeObject(jObj);
                File.WriteAllText("./Data/" + Folder + "/" + fileName + ".json", json);
            }

            _progress++;
            int percentDone = Convert.ToInt32(_progress / 25.0f * 100);
            _worker.ReportProgress(percentDone);
            _eventAggregator.PublishOnBackgroundThread(new DataObjectsLoadedFromDisk(Folder));
        }

        public void Cancel()
        {
            TryClose();
        }

        public void Handle(DatabaseConnectionSucceeded message)
        {
            SyncEnabled = true;
        }

        public void Handle(DatabaseConnectionFailed message)
        {
            _errorVM.ErrorDetails = message.Exception.ToMessageAndCompleteStacktrace();
            _windowManager.ShowDialog(_errorVM);
            DatabaseControlsEnabled = true;
        }

        public void Handle(DatabaseConnecting message)
        {
            DatabaseControlsEnabled = false;
        }
    }
}
