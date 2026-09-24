using System;
using System.Collections.Generic;
using System.ComponentModel;
using AugatonLib.Arbitration;
using Exiled.API.Enums;
using Exiled.API.Features;
using MEC;
using UnityEngine;

namespace TeamGenocide.API
{
    public sealed class LightEffect
    {
        private const string Owner = "TeamGenocide";

        private const ZoneType Facility = ZoneType.Surface | ZoneType.Entrance | ZoneType.HeavyContainment | ZoneType.LightContainment;

        private static readonly List<Room> Tinted = new List<Room>(64);

        [Description("Couleur appliquee aux lumieres, au format hexadecimal.")]
        public string Color { get; set; } = "#000000";

        [Description("Zones affectees.")]
        public List<ZoneType> Zones { get; set; } = new List<ZoneType>
        {
            ZoneType.Surface,
            ZoneType.Entrance,
            ZoneType.HeavyContainment,
            ZoneType.LightContainment,
        };

        [Description("Duree de l'effet en secondes.")]
        public float Duration { get; set; } = 5f;

        public CoroutineHandle Apply()
        {
            if (Zones is null)
                return default;

            if (!ColorUtility.TryParseHtmlString(Color, out Color color))
                color = UnityEngine.Color.black;

            if (CoversFacility())
            {
                LightArbiter.Tint(Owner, color);
                return Timing.CallDelayed(Mathf.Max(0.5f, Duration), () => Guard(() => LightArbiter.ReleaseTint(Owner)));
            }

            List<Room> affected = new List<Room>(32);

            foreach (ZoneType zone in Zones)
            {
                foreach (Room room in Room.Get(zone))
                {
                    if (room is null)
                        continue;

                    room.Color = color;
                    affected.Add(room);
                    Tinted.Add(room);
                }
            }

            if (affected.Count == 0)
                return default;

            return Timing.CallDelayed(Mathf.Max(0.5f, Duration), () => Guard(() => ResetRooms(affected)));
        }

        public static void Release()
        {
            try
            {
                LightArbiter.ReleaseTint(Owner);

                foreach (Room room in Tinted)
                {
                    if (room is not null)
                        room.ResetColor();
                }
            }
            finally
            {
                Tinted.Clear();
            }
        }

        private bool CoversFacility()
        {
            ZoneType covered = ZoneType.Unspecified;

            foreach (ZoneType zone in Zones)
            {
                if (zone == ZoneType.Unspecified)
                    return true;

                covered |= zone;
            }

            return (covered & Facility) == Facility;
        }

        private static void ResetRooms(List<Room> rooms)
        {
            foreach (Room room in rooms)
            {
                if (room is null || !Tinted.Remove(room))
                    continue;

                room.ResetColor();
            }
        }

        private static void Guard(Action action)
        {
            try
            {
                action();
            }
            catch (Exception e)
            {
                Log.Error($"LightEffect: {e}");
            }
        }
    }
}
