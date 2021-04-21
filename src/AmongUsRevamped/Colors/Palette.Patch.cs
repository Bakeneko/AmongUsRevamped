using Assets.CoreScripts;

namespace AmongUsRevamped.Colors
{
	public static class PalettePatch
    {
        public static void Load()
        {
			var shortColorNames = new[]
			{
				// Vanilla colors
				ColorPalette.ShortColorName.Red,
				ColorPalette.ShortColorName.Blue,
				ColorPalette.ShortColorName.Green,
				ColorPalette.ShortColorName.Pink,
				ColorPalette.ShortColorName.Orange,
				ColorPalette.ShortColorName.Yellow,
				ColorPalette.ShortColorName.Black,
				ColorPalette.ShortColorName.White,
				ColorPalette.ShortColorName.Purple,
				ColorPalette.ShortColorName.Brown,
				ColorPalette.ShortColorName.Cyan,
				ColorPalette.ShortColorName.Lime,
			    // New colors
			    ColorPalette.ShortColorName.Watermelon,
				ColorPalette.ShortColorName.Chocolate,
				ColorPalette.ShortColorName.SkyBlue,
				ColorPalette.ShortColorName.Beige,
				ColorPalette.ShortColorName.HotPink,
				ColorPalette.ShortColorName.Turquoise,
				ColorPalette.ShortColorName.Lilac,
				ColorPalette.ShortColorName.Amber,
				ColorPalette.ShortColorName.Rainbow
			};

			var colorNames = new[]
			{
				// Vanilla colors
				ColorPalette.ColorName.Red,
				ColorPalette.ColorName.Blue,
				ColorPalette.ColorName.Green,
				ColorPalette.ColorName.Pink,
				ColorPalette.ColorName.Orange,
				ColorPalette.ColorName.Yellow,
				ColorPalette.ColorName.Black,
				ColorPalette.ColorName.White,
				ColorPalette.ColorName.Purple,
				ColorPalette.ColorName.Brown,
				ColorPalette.ColorName.Cyan,
				ColorPalette.ColorName.Lime,
			    // New colors
			    ColorPalette.ColorName.Watermelon,
				ColorPalette.ColorName.Chocolate,
				ColorPalette.ColorName.SkyBlue,
				ColorPalette.ColorName.Beige,
				ColorPalette.ColorName.HotPink,
				ColorPalette.ColorName.Turquoise,
				ColorPalette.ColorName.Lilac,
				ColorPalette.ColorName.Amber,
				ColorPalette.ColorName.Rainbow
			};

			var playerColors = new[]
			{
				// Vanilla colors
				ColorPalette.Color.Red,
				ColorPalette.Color.Blue,
				ColorPalette.Color.Green,
				ColorPalette.Color.Pink,
				ColorPalette.Color.Orange,
				ColorPalette.Color.Yellow,
				ColorPalette.Color.Black,
				ColorPalette.Color.White,
				ColorPalette.Color.Purple,
				ColorPalette.Color.Brown,
				ColorPalette.Color.Cyan,
				ColorPalette.Color.Lime,
			    // New colors
			    ColorPalette.Color.Watermelon,
				ColorPalette.Color.Chocolate,
				ColorPalette.Color.SkyBlue,
				ColorPalette.Color.Beige,
				ColorPalette.Color.HotPink,
				ColorPalette.Color.Turquoise,
				ColorPalette.Color.Lilac,
				ColorPalette.Color.Amber,
				ColorPalette.Color.Rainbow
			};

			var shadowColors = new[]
			{
				// Vanilla colors
				ColorPalette.Color.RedShadow,
				ColorPalette.Color.BlueShadow,
				ColorPalette.Color.GreenShadow,
				ColorPalette.Color.PinkShadow,
				ColorPalette.Color.OrangeShadow,
				ColorPalette.Color.YellowShadow,
				ColorPalette.Color.BlackShadow,
				ColorPalette.Color.WhiteShadow,
				ColorPalette.Color.PurpleShadow,
				ColorPalette.Color.BrownShadow,
				ColorPalette.Color.CyanShadow,
				ColorPalette.Color.LimeShadow,
			    // New colors
			    ColorPalette.Color.WatermelonShadow,
				ColorPalette.Color.ChocolateShadow,
				ColorPalette.Color.SkyBlueShadow,
				ColorPalette.Color.BeigeShadow,
				ColorPalette.Color.HotPinkShadow,
				ColorPalette.Color.TurquoiseShadow,
				ColorPalette.Color.LilacShadow,
				ColorPalette.Color.AmberShadow,
				ColorPalette.Color.RainbowShadow
			};

			Palette.ShortColorNames = shortColorNames;
            Palette.PlayerColors = playerColors;
            Palette.ShadowColors = shadowColors;
            MedScanMinigame.ColorNames = colorNames;
            Telemetry.ColorNames = colorNames;
        }
    }
}
