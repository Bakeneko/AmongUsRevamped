using System.Collections;
using System.Collections.Generic;
using AmongUsRevamped.Extensions;
using UnityEngine;

namespace AmongUsRevamped.Mod
{
    public partial class Game
    {
        private static IEnumerator NameScrambler = null;
        private static readonly Dictionary<int, string> ScrambledNames = new();

        private static IEnumerator StartNameScrambler(float update = 0.1f)
        {
            StopNameScrambler();
            NameScrambler = Coroutines.Start(NameScramblerCoroutine(update));
            return NameScrambler;
        }

        private static void StopNameScrambler()
        {
            if (NameScrambler != null) Coroutines.Stop(NameScrambler);
        }

        protected internal static IEnumerator NameScramblerCoroutine(float update = 0.1f)
        {
            while (true)
            {
                yield return new WaitForSeconds(update);

                foreach (Player p in Player.AllPlayers)
                {
                    if (p.IsCurrentPlayer || Player.CurrentPlayer?.Dead == true)
                    {
                        ScrambledNames[p.Id] = p?.Control.Data.PlayerName;
                    }
                    else
                    {
                        var bits = "";
                        for (var i = 0; i < 8; i++)
                        {
                            bits += AmongUsRevamped.Rand.Next(0, 2);
                        }
                        ScrambledNames[p.Id] = bits;
                    }
                }
            }
        }

        protected internal static IEnumerator RemoveBodyCoroutine(byte bodyId)
        {
            DeadBody body = null;
            var bodies = Object.FindObjectsOfType<DeadBody>();
            for (int i = 0; i < bodies.Length; i++)
            {
                if (bodies[i].ParentId == bodyId)
                {
                    body = bodies[i];
                    break;
                }
            }

            if (body == null) yield return null;

            var rend = body.GetComponent<SpriteRenderer>();

            if (rend == null)
            {
                body.gameObject.Destroy();
                yield return null;
            }

            // Fade body out
            int BodyColor = Shader.PropertyToID("_BodyColor");
            int BackColor = Shader.PropertyToID("_BackColor");
            var backColor = rend.material.GetColor(BackColor);
            var bodyColor = rend.material.GetColor(BodyColor);
            var newColor = new Color(1f, 1f, 1f, 0f);

            for (var i = 0; i < 60; i++)
            {
                if (body == null) yield break;
                rend.color = Color.Lerp(backColor, newColor, i / 60f);
                rend.color = Color.Lerp(bodyColor, newColor, i / 60f);
                yield return null;
            }

            body.gameObject.Destroy();
        }
    }
}
