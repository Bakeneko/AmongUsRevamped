using System;
using UnityEngine;

namespace AmongUsRevamped.Mod.PlayerCustomization
{
    public class PlayerColorBehaviour : MonoBehaviour
    {
        public Renderer Renderer;
        public int Id;

        public void SetRenderer(Renderer renderer, int id)
        {
            Renderer = renderer;
            Id = id;
        }

        public void Update()
        {
            if (Renderer == null) return;

            if (PlayerColorUtils.IsRainbow(Id))
            {
                PlayerColorUtils.SetRainbow(Renderer);
            }
        }

        public PlayerColorBehaviour(IntPtr ptr) : base(ptr)
        {
        }
    }
}
