using System.Collections.Generic;
using System.ComponentModel;
using Exiled.API.Enums;
using Exiled.API.Features;
using MEC;
using UnityEngine;

namespace TeamGenocide.API
{
    public sealed class LightEffect
    {
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
            if (!ColorUtility.TryParseHtmlString(Color, out Color color))
                color = UnityEngine.Color.black;

            List<Room> affected = new List<Room>(32);

            foreach (ZoneType zone in Zones)
            {
                foreach (Room room in Room.Get(zone))
                {
                    if (room is null)
                        continue;

                    room.Color = color;
                    affected.Add(room);
                }
            }

            if (affected.Count == 0)
                return default;

            return Timing.CallDelayed(Mathf.Max(0.5f, Duration), () =>
            {
                foreach (Room room in affected)
                {
                    if (room is null)
                        continue;

                    room.ResetColor();
                }
            });
        }
    }
}
