using System;
using Reactor;

namespace AmongUsRevamped.Mod.PlayerCustomization
{
    [RegisterInIl2Cpp]
    public class PlayerColorRainbowBehaviour : PlayerColorBehaviour
    {
        protected override void Render()
        {
            PlayerColorUtils.SetRainbow(Renderer);
        }

        public PlayerColorRainbowBehaviour(IntPtr ptr) : base(ptr)
        {
        }
    }
}
