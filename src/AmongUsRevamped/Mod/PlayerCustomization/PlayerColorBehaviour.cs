using System;
using UnityEngine;

namespace AmongUsRevamped.Mod.PlayerCustomization
{
    public class PlayerColorBehaviour : MonoBehaviour
    {
        public Renderer Renderer;
        public StringNames ColorName;

        public void SetRenderer(Renderer renderer, StringNames colorName)
        {
            Renderer = renderer;
            ColorName = colorName;
        }

        public void Update()
        {
            if (Renderer == null) return;

            if (PlayerColorUtils.IsRainbow(ColorName))
            {
                PlayerColorUtils.SetRainbow(Renderer);
            }
        }

        public PlayerColorBehaviour(IntPtr ptr) : base(ptr)
        {
        }
    }
}
