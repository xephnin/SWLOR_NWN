﻿using FluentValidation;
using SWLOR.Game.Server.Data.Contracts;
using SWLOR.Game.Server.Data.Validator;
using SWLOR.Game.Server.Service.Contracts;

namespace SWLOR.Game.Server.Data.Processor
{
    public class ModProcessor : IDataProcessor<Entity.Mod>
    {
        public IValidator Validator => new ModValidator();

        public void Process(IDataService data, Entity.Mod dataObject)
        {
        }
    }
}
