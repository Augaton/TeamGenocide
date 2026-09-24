using System;
using System.Collections.Generic;
using Exiled.API.Features;
using AugatonLib.Runtime;
using PlayerRoles;
using TeamGenocide.API;
using TeamGenocide.Handlers;
using PlayerEvents = Exiled.Events.Handlers.Player;
using ServerEvents = Exiled.Events.Handlers.Server;

namespace TeamGenocide
{
    public sealed class Plugin : Plugin<Config>
    {
        public override string Name => "TeamGenocide";

        public override string Author => "Zone-Shilari (base: Heisenberg3666)";

        public override string Prefix => "teamgenocide";

        public override Version Version => new Version(3, 0, 0);

        public override Version RequiredExiledVersion => new Version(9, 14, 2);

        public static Plugin Instance { get; private set; }

        private GenocideHandlers genocideHandlers;

        public override void OnEnabled()
        {
            Instance = this;

            ValidateConfig();

            HintBridge.YCoordinate = Config.HintYCoordinate;
            HintBridge.FontSize = Config.HintFontSize;

            genocideHandlers = new GenocideHandlers(Config);

            PlayerEvents.ChangingRole += genocideHandlers.OnChangingRole;
            ServerEvents.RoundStarted += genocideHandlers.OnRoundStarted;
            ServerEvents.RoundEnded += genocideHandlers.OnRoundEnded;
            ServerEvents.RestartingRound += genocideHandlers.OnRestartingRound;

            ServerEvents.ReloadedConfigs += OnReloadedConfigs;

            PluginDirectory.Register(
                this,
                Capability.Hints,
                Capability.Light,
                Capability.Genocide,
                Capability.Bus);

            base.OnEnabled();
        }

        public override void OnDisabled()
        {
            if (genocideHandlers is not null)
            {
                PlayerEvents.ChangingRole -= genocideHandlers.OnChangingRole;
                ServerEvents.RoundStarted -= genocideHandlers.OnRoundStarted;
                ServerEvents.RoundEnded -= genocideHandlers.OnRoundEnded;
                ServerEvents.RestartingRound -= genocideHandlers.OnRestartingRound;

                genocideHandlers.Reset();
            }

            ServerEvents.ReloadedConfigs -= OnReloadedConfigs;
            PluginDirectory.Unregister(this);

            genocideHandlers = null;
            Instance = null;

            base.OnDisabled();
        }

        private void OnReloadedConfigs()
        {
            try
            {
                ValidateConfig();
            }
            catch (Exception e)
            {
                Log.Error($"OnReloadedConfigs: {e}");
            }
        }

        private void ValidateConfig()
        {
            if (Config.ActivationDelay < 0f)
            {
                Log.Warn($"ActivationDelay ({Config.ActivationDelay}) negatif, remis a 0.");
                Config.ActivationDelay = 0f;
            }

            if (Config.Announcements is null)
            {
                Log.Warn("Announcements est vide, aucune extinction ne sera annoncee.");
                Config.Announcements = new Dictionary<Team, List<Announcement>>();
            }
        }
    }
}
