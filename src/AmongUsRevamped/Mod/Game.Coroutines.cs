using System.Collections;
using AmongUsRevamped.Extensions;
using UnityEngine;

namespace AmongUsRevamped.Mod
{
    public partial class Game
    {
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
