using System;
using Exiled.API.Features;
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

            if (Config.ActivationDelay < 0f)
            {
                Log.Warn($"ActivationDelay ({Config.ActivationDelay}) negatif, remis a 0.");
                Config.ActivationDelay = 0f;
            }

            HintBridge.YCoordinate = Config.HintYCoordinate;
            HintBridge.FontSize = Config.HintFontSize;

            genocideHandlers = new GenocideHandlers(Config);

            PlayerEvents.ChangingRole += genocideHandlers.OnChangingRole;
            ServerEvents.RoundStarted += genocideHandlers.OnRoundStarted;
            ServerEvents.RoundEnded += genocideHandlers.OnRoundEnded;
            ServerEvents.RestartingRound += genocideHandlers.OnRestartingRound;

            base.OnEnabled();
        }

        public override void OnDisabled()
        {
            PlayerEvents.ChangingRole -= genocideHandlers.OnChangingRole;
            ServerEvents.RoundStarted -= genocideHandlers.OnRoundStarted;
            ServerEvents.RoundEnded -= genocideHandlers.OnRoundEnded;
            ServerEvents.RestartingRound -= genocideHandlers.OnRestartingRound;

            genocideHandlers?.Reset();
            genocideHandlers = null;
            Instance = null;

            base.OnDisabled();
        }
    }
}
