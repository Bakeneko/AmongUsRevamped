using System;
using Reactor;
using UnityEngine;

namespace AmongUsRevamped.Mod.PlayerCustomization
{
    [RegisterInIl2Cpp]
    public class PlayerColorBehaviour : MonoBehaviour
    {
        private static readonly int BackColor = Shader.PropertyToID("_BackColor");
        private static readonly int BodyColor = Shader.PropertyToID("_BodyColor");
        private static readonly int VisorColor = Shader.PropertyToID("_VisorColor");

        public static float PingPong(float min, float max, float speed)
        {
            return min + Mathf.PingPong(Time.time * speed, max - min);
        }

        public Renderer Renderer;

        public void SetRenderer(Renderer renderer)
        {
            Renderer = renderer;
        }

        protected virtual void Render()
        {
        }

        public void Update()
        {
            if (Renderer == null) return;

            Render();
        }

        public PlayerColorBehaviour(IntPtr ptr) : base(ptr)
        {
        }
    }
}
