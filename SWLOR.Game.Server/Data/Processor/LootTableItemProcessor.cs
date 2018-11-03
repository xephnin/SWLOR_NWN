﻿using FluentValidation;
using SWLOR.Game.Server.Data.Contracts;
using SWLOR.Game.Server.Data.Entity;
using SWLOR.Game.Server.Data.Validator;
using SWLOR.Game.Server.Service.Contracts;
using SWLOR.Game.Server.ValueObject;

namespace SWLOR.Game.Server.Data.Processor
{
    public class LootTableItemProcessor : IDataProcessor<LootTableItem>
    {
        public IValidator Validator => new LootTableItemValidator();

        public DatabaseAction Process(IDataService data, LootTableItem dataObject)
        {
            return null;
        }
    }
}
