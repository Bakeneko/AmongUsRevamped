﻿using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Il2CppSystem.Text;
using Reactor.Extensions;
using UnhollowerBaseLib;
using UnityEngine;
using Object = UnityEngine.Object;

namespace AmongUsRevamped.Options
{
    /// <summary>
    /// A derivative of <see cref="CustomButtonOption"/>, handling options exports.
    /// </summary>
    public class CustomExporterOption : CustomButtonOption
    {

        public List<CustomButtonOption> SlotButtons = new List<CustomButtonOption>();
        public List<OptionBehaviour> PreviousOptions;
        public CustomButtonOption Loading;

        /// <summary>
        /// Adds an export "button" in the options menu.
        /// </summary>
        /// <param name="id">Id of the option</param>
        /// <param name="title">Title of the header</param>
        public CustomExporterOption(string id, string title) : base(id, title, true, false)
        {
            ActionOnClick = Export;
        }

        protected internal void Export()
        {
            StartImportExport();

            var __instance = Object.FindObjectOfType<GameOptionsMenu>();
            float y = __instance.GetComponentsInChildren<OptionBehaviour>()
                     .Max(option => option.transform.localPosition.y);

            PreviousOptions = __instance.Children.ToList();

            var options = new List<OptionBehaviour>();

            ToggleOption toggleOption = Object.FindObjectsOfType<ToggleOption>().FirstOrDefault();

            SlotButtons.Clear();
            SlotButtons.Add(new CustomButtonOption("exportSlot1", "Export to Slot 1", true, false, delegate
            {
                ExportSlot(1);
            }));
            SlotButtons.Add(new CustomButtonOption("exportSlot2", "Export to Slot 2", true, false, delegate
            {
                ExportSlot(2);
            }));
            SlotButtons.Add(new CustomButtonOption("exportSlot3", "Export to Slot 3", true, false, delegate
            {
                ExportSlot(3);
            }));
            SlotButtons.Add(new CustomButtonOption("exportSlot4", "Export to Slot 4", true, false, delegate
            {
                ExportSlot(4);
            }));
            SlotButtons.Add(new CustomButtonOption("exportCancel", "Cancel", true, false, delegate
            {
                ExportEnd(FlashWhite);
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

        private void ExportSlot(int slotId)
        {
            foreach (var option in SlotButtons)
            {
                option.ActionOnClick = null;
            }

            var sb = new StringBuilder();

            foreach(var option in Options)
            {
                if (!option.Persist) continue;

                if (option is CustomNumberOption number)
                {
                    sb.AppendLine(number.ConfigId);
                    sb.AppendLine(number.GetValue().ToString());
                }
                else if (option is CustomToggleOption toggle)
                {
                    sb.AppendLine(toggle.ConfigId);
                    sb.AppendLine(toggle.GetValue().ToString());
                }
                else if (option is CustomStringOption str)
                {
                    sb.AppendLine(str.ConfigId);
                    sb.AppendLine(str.GetValue().ToString());
                }
            }

            var tmp = Path.Combine(Application.persistentDataPath, $"{PluginId}.Settings.Slot{slotId}.tmp");
            try
            {
                File.WriteAllText(tmp, sb.ToString());
                var file = Path.Combine(Application.persistentDataPath, $"{PluginId}.Settings.Slot{slotId}");
                File.Delete(file);
                File.Move(tmp, file);
                ExportEnd(FlashGreen);
            }
            catch (Exception ex)
            {
                AmongUsRevamped.Logger.LogError($"An exception has occurred exporting settings: {ex}");
                ExportEnd(FlashRed);
            }
        }

        protected internal void ExportEnd(Func<IEnumerator> flashCoroutine)
        {
            Reactor.Coroutines.Start(ExportEndCoroutine(flashCoroutine));
        }

        protected internal IEnumerator ExportEndCoroutine(Func<IEnumerator> flashCoroutine)
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