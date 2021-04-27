using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnhollowerBaseLib;
using Reactor.Extensions;
using UnityEngine;
using Object = UnityEngine.Object;

namespace AmongUsRevamped.Options
{
    /// <summary>
    /// A derivative of <see cref="CustomButtonOption"/>, handling options imports.
    /// </summary>
    public class CustomImporterOption : CustomButtonOption
    {

        public List<CustomButtonOption> SlotButtons = new();
        public List<OptionBehaviour> PreviousOptions;
        public CustomButtonOption Loading;

        /// <summary>
        /// Adds an import "button" in the options menu.
        /// </summary>
        /// <param name="id">Id of the option</param>
        /// <param name="title">Title of the header</param>
        public CustomImporterOption(string id, string title) : base(id, title, true, false)
        {
            ActionOnClick = Import;
        }

        protected internal void Import()
        {
            StartImportExport();

            var __instance = Object.FindObjectOfType<GameOptionsMenu>();
            float y = __instance.GetComponentsInChildren<OptionBehaviour>()
                     .Max(option => option.transform.localPosition.y);

            PreviousOptions = __instance.Children.ToList();

            var options = new List<OptionBehaviour>();

            ToggleOption toggleOption = Object.FindObjectsOfType<ToggleOption>().FirstOrDefault();

            SlotButtons.Clear();
            SlotButtons.Add(new CustomButtonOption("importSlot1", "Import to Slot 1", true, false, delegate
            {
                ImportSlot(1);
            }));
            SlotButtons.Add(new CustomButtonOption("importSlot2", "Import to Slot 2", true, false, delegate
            {
                ImportSlot(2);
            }));
            SlotButtons.Add(new CustomButtonOption("importSlot3", "Import to Slot 3", true, false, delegate
            {
                ImportSlot(3);
            }));
            SlotButtons.Add(new CustomButtonOption("importSlot4", "Import to Slot 4", true, false, delegate
            {
                ImportSlot(4);
            }));
            SlotButtons.Add(new CustomButtonOption("importCancel", "Cancel", true, false, delegate
            {
                ImportEnd(FlashWhite);
            }));

            int i = 0;
            foreach (var button in SlotButtons)
            {
                if (toggleOption == null) continue;

                ToggleOption toggle = Object.Instantiate(toggleOption, toggleOption.transform.parent);
                toggle.transform.FindChild("CheckBox")?.gameObject?.SetActive(false);
                toggle.transform.FindChild("Background")?.gameObject?.SetActive(true);
                if (!button.OnGameObjectCreated(toggle))
                {
                    toggle?.gameObject?.Destroy();
                }
                else
                {
                    toggle.transform.localPosition = new Vector3(toggle.transform.localPosition.x, y - i++ * 0.5f,
                        toggle.transform.localPosition.z);
                    button.GameObject.gameObject.SetActive(true);
                    options.Add(button.GameObject);
                }
            }

            foreach (var option in __instance.Children)
            {
                option.gameObject.SetActive(false);
            }

            __instance.Children = new Il2CppReferenceArray<OptionBehaviour>(options.ToArray());
        }

        private void ImportSlot(int slotId)
        {
            foreach (var option in SlotButtons)
            {
                option.ActionOnClick = null;
            }

            var issues = 0;
            string content;
            try
            {
                var file = Path.Combine(Application.persistentDataPath, $"{PluginId}.Settings.Slot{slotId}");
                content = File.ReadAllText(file);
            }
            catch (Exception ex)
            {
                AmongUsRevamped.LogWarning($"An exception has occurred reading settings: {ex}");
                ImportEnd(FlashRed);
                return;
            }

            var lines = content.Split("\n").ToList();

            while (lines.Count > 1)
            {
                var configId = lines[0].Trim();
                lines.RemoveAt(0);
                var value = lines[0].Trim();
                lines.RemoveAt(0);

                if (value == null) continue;

                CustomOption option = Options.FirstOrDefault(o => o.ConfigId.Equals(configId, StringComparison.Ordinal));

                if (option == null)
                {
                    AmongUsRevamped.LogWarning($"Parsed a setting that could not be found, configId: \"{configId}\".");
                    issues++;
                    continue;
                }

                try
                {
                    if (option is CustomNumberOption number)
                    {
                        number.SetValue(float.Parse(value));
                    }
                    else if (option is CustomToggleOption toggle)
                    {
                        toggle.SetValue(bool.Parse(value));
                    }
                    else if (option is CustomStringOption str)
                    {
                        str.SetValue(int.Parse(value));
                    }
                }
                catch (Exception ex)
                {
                    AmongUsRevamped.LogError($"An exception has occurred parsing a setting, configId: \"{configId}\" value: \"{value}\": {ex}");
                    issues++;
                }
            }

            ImportEnd(issues == 0 ? FlashGreen : FlashRed);
        }

        protected internal void ImportEnd(Func<IEnumerator> flashCoroutine)
        {
            Reactor.Coroutines.Start(ImportEndCoroutine(flashCoroutine));
        }

        protected internal IEnumerator ImportEndCoroutine(Func<IEnumerator> flashCoroutine)
        {
            var __instance = Object.FindObjectOfType<GameOptionsMenu>();
            foreach (var option in SlotButtons.Skip(1))
            {
                option?.GameObject?.gameObject?.Destroy();
            }

            Loading = SlotButtons[0];
            Loading.GameObject.Cast<ToggleOption>().TitleText.text = "Loading...";

            __instance.Children = new[] { Loading.GameObject };

            yield return new WaitForSeconds(0.5f);

            Loading?.GameObject?.gameObject?.Destroy();

            MenuVisible = true;
            foreach (var option in PreviousOptions)
            {
                option.gameObject.SetActive(true);
            }
            __instance.Children = PreviousOptions.ToArray();
            EndImportExport();

            yield return new WaitForEndOfFrame();
            yield return flashCoroutine();
        }

        private IEnumerator FlashGreen()
        {
            GameObject.Cast<ToggleOption>().TitleText.color = Color.green;
            yield return new WaitForSeconds(0.5f);
            GameObject.Cast<ToggleOption>().TitleText.color = Color.white;
            yield return null;
        }

        private IEnumerator FlashRed()
        {
            GameObject.Cast<ToggleOption>().TitleText.color = Color.red;
            yield return new WaitForSeconds(0.5f);
            GameObject.Cast<ToggleOption>().TitleText.color = Color.white;
            yield return null;
        }

        private IEnumerator FlashWhite()
        {
            yield return null;
        }
    }
}
