using System.ComponentModel;
using Exiled.API.Features;
using MEC;

namespace TeamGenocide.API
{
    public sealed class Announcement
    {
        [Description("Annonce C.A.S.S.I.E. Vide = desactivee.")]
        public string Cassie { get; set; } = string.Empty;

        [Description("Sous-titres C.A.S.S.I.E. Vide = pas de sous-titres.")]
        public string Subtitle { get; set; } = string.Empty;

        [Description("Broadcast diffuse a tous. Vide = desactive.")]
        public string Broadcast { get; set; } = string.Empty;

        [Description("Hint affiche a tous. Vide = desactive.")]
        public string Hint { get; set; } = string.Empty;

        [Description("Duree d'affichage en secondes.")]
        public ushort DisplayTime { get; set; } = 10;

        [Description("Effet lumineux accompagnant l'annonce. Laisser vide pour desactiver.")]
        public LightEffect Lights { get; set; }

        public CoroutineHandle Announce()
        {
            if (!string.IsNullOrEmpty(Cassie))
            {
                if (string.IsNullOrEmpty(Subtitle))
                    Exiled.API.Features.Cassie.Message(Cassie, false, false, false);
                else
                    Exiled.API.Features.Cassie.MessageTranslated(Cassie, Subtitle, false, false, false);
            }

            if (!string.IsNullOrEmpty(Broadcast))
                Map.Broadcast(DisplayTime, Broadcast);

            if (!string.IsNullOrEmpty(Hint))
            {
                foreach (Player player in Player.List)
                    HintBridge.Show(player, Hint, DisplayTime);
            }

            return Lights is null ? default : Lights.Apply();
        }
    }
}
