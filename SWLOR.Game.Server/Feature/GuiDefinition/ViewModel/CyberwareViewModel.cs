using System;
using System.Collections.Generic;
using System.Linq;
using SWLOR.Game.Server.Entity;
using SWLOR.Game.Server.Service;
using SWLOR.Game.Server.Service.CyberwareService;
using SWLOR.Game.Server.Service.GuiService;
using Player = SWLOR.Game.Server.Entity.Player;

namespace SWLOR.Game.Server.Feature.GuiDefinition.ViewModel
{
    /// <summary>
    /// The cyberclinic view model. Lists the whole catalogue with an Install/Remove button per row,
    /// shows Essence remaining and nuyen on hand, and applies the chrome-versus-magic tradeoff: each
    /// install spends Essence and, through <see cref="Stat.GetMaxFP(uint, Player)"/>, quietly erodes
    /// Magic.
    ///
    /// Removal is free in this first slice so the tradeoff can be explored freely in playtest; a
    /// removal fee and locked Essence are faithful refinements deferred with the rest (decision D16).
    /// </summary>
    public class CyberwareViewModel : GuiViewModelBase<CyberwareViewModel, GuiPayloadBase>
    {
        // Row index -> cyberware id, kept parallel to the bound lists.
        private readonly List<string> _ids = new();

        public string EssenceText
        {
            get => Get<string>();
            set => Set(value);
        }

        public string NuyenText
        {
            get => Get<string>();
            set => Set(value);
        }

        public GuiBindingList<string> Names
        {
            get => Get<GuiBindingList<string>>();
            set => Set(value);
        }

        public GuiBindingList<string> Details
        {
            get => Get<GuiBindingList<string>>();
            set => Set(value);
        }

        public GuiBindingList<string> ActionText
        {
            get => Get<GuiBindingList<string>>();
            set => Set(value);
        }

        protected override void Initialize(GuiPayloadBase initialPayload)
        {
            LoadData();
        }

        private void LoadData()
        {
            var dbPlayer = DB.Get<Player>(GetObjectUUID(Player));

            var names = new GuiBindingList<string>();
            var details = new GuiBindingList<string>();
            var actions = new GuiBindingList<string>();
            _ids.Clear();

            foreach (var (id, detail) in Cyberware.GetAll().OrderBy(x => x.Value.EssenceCost))
            {
                var installed = dbPlayer.InstalledCyberware.Contains(id);
                _ids.Add(id);
                names.Add(detail.Name);
                details.Add($"{detail.EssenceCost:0.0} Ess   {detail.InstallPrice} nuyen");
                actions.Add(installed ? "Remove" : "Install");
            }

            Names = names;
            Details = details;
            ActionText = actions;

            EssenceText = $"Essence: {Cyberware.GetEssenceAvailable(dbPlayer):0.0} / {Cyberware.MaxEssence:0.0}";
            NuyenText = $"Nuyen: {GetGold(Player)}";
        }

        public Action OnClickCyberware() => () =>
        {
            var index = NuiGetEventArrayIndex();
            if (index < 0 || index >= _ids.Count)
                return;

            var id = _ids[index];
            var detail = Cyberware.GetById(id);
            if (detail == null)
                return;

            var dbPlayer = DB.Get<Player>(GetObjectUUID(Player));

            if (dbPlayer.InstalledCyberware.Contains(id))
                Remove(dbPlayer, id, detail);
            else
                Install(dbPlayer, id, detail);

            LoadData();
        };

        private void Install(Player dbPlayer, string id, CyberwareDetail detail)
        {
            var reason = Cyberware.GetInstallBlockReason(dbPlayer, id, 0);
            if (!string.IsNullOrEmpty(reason))
            {
                SendMessageToPC(Player, reason);
                return;
            }

            if (GetGold(Player) < detail.InstallPrice)
            {
                SendMessageToPC(Player, "You cannot afford that.");
                return;
            }

            TakeGoldFromCreature(detail.InstallPrice, Player, true);
            Cyberware.AddInstalled(dbPlayer, id);
            ClampFPToMax(dbPlayer);
            DB.Set(dbPlayer);

            SendMessageToPC(Player, $"Installed {detail.Name}.");
        }

        private void Remove(Player dbPlayer, string id, CyberwareDetail detail)
        {
            Cyberware.RemoveInstalled(dbPlayer, id);
            // Removing frees Essence, which can only raise the FP ceiling, so no downward clamp needed.
            DB.Set(dbPlayer);

            SendMessageToPC(Player, $"Removed {detail.Name}. Essence recovered.");
        }

        /// <summary>
        /// After spending Essence, a player's Magic ceiling drops; clamp current FP so it never sits
        /// above the new maximum.
        /// </summary>
        private void ClampFPToMax(Player dbPlayer)
        {
            var maxFP = Stat.GetMaxFP(Player, dbPlayer);
            if (dbPlayer.FP > maxFP)
                dbPlayer.FP = maxFP;
        }
    }
}
