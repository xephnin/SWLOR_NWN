﻿using NWN;
using SWLOR.Game.Server.Event;
using SWLOR.Game.Server.GameObject;
using SWLOR.Game.Server.Service.Contracts;
using static NWN.NWScript;

namespace SWLOR.Game.Server.Placeable.Corpse
{
    public class OnDisturbed: IRegisteredEvent
    {
        private readonly INWScript _;
        private readonly IItemService _item;


        public OnDisturbed(INWScript script,
            IItemService item)
        {
            _ = script;
            _item = item;
        }

        public bool Run(params object[] args)
        {
            NWCreature looter = _.GetLastDisturbed();
            NWItem item = _.GetInventoryDisturbItem();
            int type = _.GetInventoryDisturbType();

            looter.AssignCommand(() =>
            {
                _.ActionPlayAnimation(ANIMATION_LOOPING_GET_LOW, 1.0f, 1.0f);
            });

            if (type == INVENTORY_DISTURB_TYPE_ADDED)
            {
                _item.ReturnItem(looter, item);
                looter.SendMessage("You cannot place items inside of corpses.");
            }
            else if (type == INVENTORY_DISTURB_TYPE_REMOVED)
            {
                NWItem copy = item.GetLocalObject("CORPSE_ITEM_COPY");

                if (copy.IsValid)
                {
                    copy.Destroy();
                }

                item.DeleteLocalObject("CORPSE_ITEM_COPY");
            }


            return true;
        }
    }
}
