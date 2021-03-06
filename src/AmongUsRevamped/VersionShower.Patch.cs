using System;
using AmongUsRevamped.Colors;
using AmongUsRevamped.Extensions;
using AmongUsRevamped.Utils;
using BepInEx;
using HarmonyLib;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace AmongUsRevamped
{
    [HarmonyPatch]
    public static class VersionShowerPatch
    {
        internal static void Load()
        {
            // Handle every scene with the version shower
            SceneManager.add_sceneLoaded((Action<Scene, LoadSceneMode>)((scene, loadMode) =>
            {
                var versionShower = UnityEngine.Object.FindObjectOfType<VersionShower>();
                if (!versionShower)
                    return;

                var gameObject = new GameObject("Revamped Version");
                gameObject.transform.parent = versionShower.transform.parent;

                var aspectPosition = gameObject.AddComponent<AspectPosition>();
                aspectPosition.Alignment = AspectPosition.EdgeAlignments.LeftTop;

                var originalAspectPosition = versionShower.GetComponent<AspectPosition>();
                var originalPosition = originalAspectPosition.DistanceFromEdge;
                originalPosition.y = 0.15f;
                originalAspectPosition.DistanceFromEdge = originalPosition;
                originalAspectPosition.AdjustPosition();

                var position = originalPosition;
                position.x += 10.075f - 0.1f;
                position.y += 2.75f - 0.15f;
                position.z -= 1f;
                aspectPosition.DistanceFromEdge = position;
                aspectPosition.AdjustPosition();

                var text = gameObject.AddComponent<TextMeshPro>();
                text.fontSize = 2f;
                text.text = ColorPalette.Color.Revamped.ToColorTag($"{AmongUsRevamped.Name} v{AmongUsRevamped.VersionString}");
                text.text += $"\nBepInEx: v{Paths.BepInExVersion}";
            }));

            // Handle main menu banner
            SceneManager.add_sceneLoaded((Action<Scene, LoadSceneMode>)((scene, loadMode) =>
            {
                if (scene.name != "MainMenu") return;

                var officialBanner = GameObject.Find("bannerLogo_AmongUs");
                if (officialBanner == null) return;

                var banner = new GameObject("bannerLogo_Revamped");
                banner.transform.parent = officialBanner.transform.parent;
                banner.transform.position = Vector3.up * 0.6f;
                var renderer = banner.AddComponent<SpriteRenderer>();
                renderer.sprite = AssetUtils.LoadSpriteFromResource("AmongUsRevamped.Resources.Sprites.revamped_banner.png");
            }));
        }
    }
}
