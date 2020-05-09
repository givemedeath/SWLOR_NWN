﻿using System;
using System.Linq;
using SWLOR.Game.Server.GameObject;

using NWN;
using SWLOR.Game.Server.Data.Entity;
using SWLOR.Game.Server.Enumeration;
using SWLOR.Game.Server.Event.Area;
using SWLOR.Game.Server.Event.Module;
using SWLOR.Game.Server.Messaging;
using SWLOR.Game.Server.NWNX;
using SWLOR.Game.Server.ValueObject;
using static SWLOR.Game.Server.NWN.NWScript;
using NWScript = SWLOR.Game.Server.NWN.NWScript;

namespace SWLOR.Game.Server.Service
{
    public static class MapService
    {
        public static void SubscribeEvents()
        {
            MessageHub.Instance.Subscribe<OnAreaEnter>(message => OnAreaEnter());
            MessageHub.Instance.Subscribe<OnAreaExit>(message => OnAreaExit());
            MessageHub.Instance.Subscribe<OnAreaHeartbeat>(message => OnAreaHeartbeat());
            MessageHub.Instance.Subscribe<OnModuleLeave>(message => OnModuleLeave());
        }
        
        private static void OnAreaEnter()
        {
            using (new Profiler("MapService.OnAreaEnter"))
            {
                NWArea area = (NWScript.OBJECT_SELF);
                NWPlayer player = NWScript.GetEnteringObject();

                if (!player.IsPlayer) return;

                if (area.GetLocalInt("AUTO_EXPLORED") == TRUE)
                {
                    NWScript.ExploreAreaForPlayer(area.Object, player);
                }

                LoadMapProgression(area, player);
            }
        }

        private static void OnAreaExit()
        {
            using(new Profiler("MapService.OnAreaExit"))
            {
                NWArea area = NWScript.OBJECT_SELF;
                NWPlayer player = NWScript.GetExitingObject();
                if (!player.IsPlayer) return;

                SaveMapProgression(area, player);
            }
        }

        private static void OnModuleLeave()
        {
            NWPlayer player = NWScript.GetExitingObject();
            NWArea area = NWScript.GetArea(player);
            if (!player.IsPlayer || !area.IsValid) return;

            SaveMapProgression(area, player);
        }

        private static void SaveMapProgression(NWArea area, NWPlayer player)
        {
            var map = DataService.PCMapProgression.GetByPlayerIDAndAreaResrefOrDefault(player.GlobalID, area.Resref);
            DatabaseActionType action = DatabaseActionType.Update;

            if (map == null)
            {
                map = new PCMapProgression
                {
                    PlayerID = player.GlobalID,
                    AreaResref = area.Resref,
                    Progression = string.Empty
                };

                action = DatabaseActionType.Insert;
            }

            map.Progression = NWNXPlayer.GetAreaExplorationState(player, area);
            DataService.SubmitDataChange(map, action);
        }

        private static void LoadMapProgression(NWArea area, NWPlayer player)
        {
            var map = DataService.PCMapProgression.GetByPlayerIDAndAreaResrefOrDefault(player.GlobalID, area.Resref);

            // No progression set - do a save which will create the record.
            if (map == null)
            {
                SaveMapProgression(area, player);
                return;
            }
            
            NWNXPlayer.SetAreaExplorationState(player, area, map.Progression);
        }


        private static void OnAreaHeartbeat()
        {
            NWArea area = NWScript.OBJECT_SELF;
            
            if (area.GetLocalInt("HIDE_MINIMAP") == NWScript.TRUE)
            {
                var players = NWModule.Get().Players.Where(x => x.Area.Equals(area) && x.IsPlayer);

                foreach (var player in players)
                {
                    NWScript.ExploreAreaForPlayer(area, player, NWScript.FALSE);
                }
            }

        }

    }
}
