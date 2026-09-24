using System;
using System.Collections.Generic;
using Exiled.API.Enums;
using Exiled.API.Features;
using Exiled.Events.EventArgs.Player;
using Exiled.Events.EventArgs.Server;
using MEC;
using PlayerRoles;
using AugatonLib.Arbitration;
using AugatonLib.Bus;
using TeamGenocide.API;

namespace TeamGenocide.Handlers
{
    public sealed class GenocideHandlers
    {
        private readonly Config config;
        private readonly HashSet<Team> announced = new HashSet<Team>();
        private readonly List<CoroutineHandle> pending = new List<CoroutineHandle>(8);

        private CoroutineHandle activationHandle;
        private bool activationScheduled;
        private bool activated;

        public GenocideHandlers(Config config) => this.config = config;

        public void OnRoundStarted()
        {
            try
            {
                Reset();

                activationScheduled = true;
                activationHandle = Timing.CallDelayed(config.ActivationDelay, () =>
                {
                    activationScheduled = false;
                    activated = true;
                });
            }
            catch (Exception e)
            {
                Log.Error($"OnRoundStarted: {e}");
            }
        }

        public void OnRoundEnded(RoundEndedEventArgs ev) => Reset();

        public void OnRestartingRound() => Reset();

        public void OnChangingRole(ChangingRoleEventArgs ev)
        {
            try
            {
                if (!activated || ev?.Player is null || ev.Reason != SpawnReason.Died)
                    return;

                if (GenocideArbiter.IsSuppressed)
                    return;

                Team leaving = ev.Player.Role.Team;

                if (leaving == Team.Dead)
                    return;

                if (ev.NewRole.GetTeam() == leaving)
                    return;

                if (config.AnnounceOncePerTeam && announced.Contains(leaving))
                    return;

                if (!config.Announcements.TryGetValue(leaving, out List<Announcement> announcements)
                    || announcements is null
                    || announcements.Count == 0)
                {
                    return;
                }

                if (CountTeam(leaving, ev.Player) > 0)
                    return;

                announced.Add(leaving);

                Announcement announcement = announcements[UnityEngine.Random.Range(0, announcements.Count)];

                if (announcement is null)
                    return;

                CoroutineHandle handle = announcement.Announce();

                if (handle.IsRunning)
                    pending.Add(handle);

                PluginBus.Publish(BusTopics.TeamWiped, "TeamGenocide", leaving);

                Log.Info($"[TeamGenocide] Equipe {leaving} entierement eliminee.");
            }
            catch (Exception e)
            {
                Log.Error($"OnChangingRole: {e}");
            }
        }

        private static int CountTeam(Team team, Player excluded)
        {
            int count = 0;

            foreach (Player player in Player.List)
            {
                if (player is null || player == excluded)
                    continue;

                if (player.Role.Team == team)
                    count++;
            }

            return count;
        }

        public void Reset()
        {
            if (activationScheduled)
            {
                Timing.KillCoroutines(activationHandle);
                activationScheduled = false;
            }

            foreach (CoroutineHandle handle in pending)
                Timing.KillCoroutines(handle);

            pending.Clear();
            announced.Clear();
            activated = false;

            try
            {
                HintBridge.Clear();
                LightEffect.Release();
            }
            catch (Exception e)
            {
                Log.Error($"Reset: {e}");
            }
        }
    }
}
