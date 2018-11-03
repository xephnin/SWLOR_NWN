﻿using FluentValidation;
using SWLOR.Game.Server.Data.Contracts;
using SWLOR.Game.Server.Data.Validator;

namespace SWLOR.Game.Server.Data.Processor
{
    public class ModProcessor : IDataProcessor<Entity.Mod>
    {
        public IValidator Validator => new ModValidator();

        public void Process(IDataContext db, Entity.Mod dataObject)
        {
        }
    }
}
