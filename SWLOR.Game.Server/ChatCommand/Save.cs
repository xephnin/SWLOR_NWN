﻿using NWN;
using SWLOR.Game.Server.ChatCommand.Contracts;
using SWLOR.Game.Server.Enumeration;
using SWLOR.Game.Server.GameObject;
using SWLOR.Game.Server.Service.Contracts;


namespace SWLOR.Game.Server.ChatCommand
{
    [CommandDetails("Manually saves your character. Your character also saves automatically every few minutes.", CommandPermissionType.Player)]
    public class Save: IChatCommand
    {
        private readonly INWScript _;
        private readonly IPlayerService _player;

        public Save(INWScript script,
            IPlayerService player)
        {
            _ = script;
            _player = player;
        }

        /// <summary>
        /// Exports user's character bic file.
        /// </summary>
        /// <param name="user"></param>
        /// <param name="target"></param>
        /// <param name="targetLocation"></param>
        /// <param name="args"></param>
        public void DoAction(NWPlayer user, NWObject target, NWLocation targetLocation, params string[] args)
        {
            _player.SaveCharacter(user);
            _player.SaveLocation(user);
            _.ExportSingleCharacter(user.Object);
            _.SendMessageToPC(user.Object, "Character saved successfully.");
        }

        public string ValidateArguments(NWPlayer user, params string[] args)
        {
            return string.Empty;
        }

        public bool RequiresTarget => false;
    }
}
