﻿using NWN;
using SWLOR.Game.Server.Event;
using SWLOR.Game.Server.GameObject;

namespace SWLOR.Game.Server.Placeable.FuelBay
{
    public class OnClosed: IRegisteredEvent
    {
        public bool Run(params object[] args)
        {
            NWPlaceable objSelf = (Object.OBJECT_SELF);
            NWObject parent = (objSelf.GetLocalObject("CONTROL_TOWER_PARENT"));
            parent.DeleteLocalObject("CONTROL_TOWER_FUEL_BAY");
            objSelf.DestroyAllInventoryItems();
            objSelf.Destroy();
            return true;
        }
    }
}
