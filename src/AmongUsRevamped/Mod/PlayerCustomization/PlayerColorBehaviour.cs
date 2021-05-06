using System;
using UnityEngine;

namespace AmongUsRevamped.Mod.PlayerCustomization
{
    [RegisterInIl2Cpp]
    public class PlayerColorBehaviour : MonoBehaviour
    {
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
