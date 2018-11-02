﻿using FluentValidation;
using SWLOR.Game.Server.Data.Contracts;
using SWLOR.Game.Server.Data.Validator;

namespace SWLOR.Game.Server.Data.Processor
{
    public class CustomEffectProcessor : IDataProcessor<CustomEffect>
    {
        public IValidator Validator => new CustomEffectValidator();

        public void Process(IDataContext db, CustomEffect dataObject)
        {
        }
    }
}