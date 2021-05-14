using System.Collections.Generic;
using System.Linq;
using AmongUsRevamped.Colors;
using AmongUsRevamped.Extensions;
using AmongUsRevamped.UI;
using AmongUsRevamped.Utils;
using UnityEngine;

namespace AmongUsRevamped.Mod.Roles
{
    public class Snitch : Crewmate
    {
        private Message CurrentMessage;
        private AudioClip AnnounceSound = null;

        private readonly int TasksLeftBeforeBusted = (int)Options.Values.SnitchTasksLeftBeforeBusted;
        private readonly List<Arrow> Arrows = new();
        private int RemainingTasks = int.MaxValue;
        private bool BustWarningDisplayed;
        private bool UncoverWarningDisplayed;

        public Snitch(Player player) : base(player)
        {
            Name = "Snitch";
            RoleType = RoleType.Snitch;
            Color = ColorPalette.Color.RoleSnitch;
            IntroDescription = () => $"Complete your tasks to uncover the {ColorPalette.Color.RoleImpostor.ToColorTag("Impostors")}";
            TaskDescription = () => Color.ToColorTag(RemainingTasks > 0 ? $"{Name}: Complete your tasks to uncover the Impostors" : $"{Name}: Uncover the impostors before they get you!");
            ExileDescription = () => $"{Player.Name} was The {Name}";
            Init();
        }

        protected void Init()
        {
            AnnounceSound = AssetUtils.LoadAudioClipFromResource("AmongUsRevamped.Resources.Sounds.event_announce.wav");
        }

        public override void HudUpdate(HudManager hudManager)
        {
            if (Disposed) return;

            base.HudUpdate(hudManager);

            List<Arrow> Availables = new(Arrows);

            var currentPlayer = Player.CurrentPlayer;
            if (Player.IsCurrentPlayer && !Player.Dead && !Player.Disconnected)
            {
                if (!BustWarningDisplayed && TasksLeftBeforeBusted > 0 && RemainingTasks <= TasksLeftBeforeBusted)
                {
                    DisplayMessage(6f, $"Snitches get stiches: {ColorPalette.Color.RoleImpostor.ToColorTag("Impostors")} are coming for you !");
                    SoundManager.Instance.PlaySound(AnnounceSound, false, 0.6f);
                    BustWarningDisplayed = true;
                }
                if (RemainingTasks == 0)
                {
                    if (!UncoverWarningDisplayed)
                    {
                        DisplayMessage(6f, $"Uncover the {ColorPalette.Color.RoleImpostor.ToColorTag("Impostors")} before they get you !");
                        UncoverWarningDisplayed = true;
                    }

                    var impostors = AllRoles.Where(r => r.Faction == Faction.Impostors && !r.Player.Dead && !r.Player.Disconnected).ToList();
                    foreach (Role imp in impostors)
                    {
                        Arrow arrow = null;
                        if (Availables.Count > 0)
                        {
                            arrow = Availables[0];
                            Availables.RemoveAt(0);
                        }
                        if (arrow == null)
                        {
                            arrow = new("AmongUsRevamped.Resources.Sprites.arrow.png", ColorPalette.Color.SnitchArrow);
                            Arrows.Add(arrow);
                        }
                        arrow.Target = imp?.Player.Control?.transform?.position ?? Vector3.zero;
                        arrow.Visible = true;
                    }
                }
            } 
            else if (currentPlayer.Role?.Faction == Faction.Impostors && !currentPlayer.Dead && !currentPlayer.Disconnected)
            {
                if (RemainingTasks <= TasksLeftBeforeBusted)
                {
                    if (!Player.Dead && !Player.Disconnected)
                    {
                        if (!BustWarningDisplayed)
                        {
                            DisplayMessage(6f, $"Snitches get stiches: Find the {ColorPalette.Color.RoleSnitch.ToColorTag("Snitch")} !");
                            SoundManager.Instance.PlaySound(AnnounceSound, false, 0.6f);
                            BustWarningDisplayed = true;
                        }

                        Arrow arrow = null;
                        if (Availables.Count > 0)
                        {
                            arrow = Availables[0];
                            Availables.RemoveAt(0);
                        }
                        if (arrow == null)
                        {
                            arrow = new("AmongUsRevamped.Resources.Sprites.arrow.png", ColorPalette.Color.SnitchArrow);
                            Arrows.Add(arrow);
                        }
                        arrow.Target = Player.Control?.transform?.position ?? Vector3.zero;
                        arrow.Visible = true;
                    }
                }
            }

            Availables.ForEach(a =>
            {
                AmongUsRevamped.Log($"Remove useless arrow");
                Arrows.Remove(a);
                a.Dispose();
            });
        }

        public override void OnTasksCreated()
        {
            UpdateTasks();
        }

        public override void OnCompletedTask(GameData.TaskInfo task)
        {
            UpdateTasks();
        }

        private void UpdateTasks()
        {
            var tasks = Player.GetTasksStatus();
            RemainingTasks = tasks.Item2 - tasks.Item1;
            if (RemainingTasks == 0) Player.UpdateImportantTasks();
        }

        private void DisplayMessage(float duration, string message)
        {
            CurrentMessage?.Dispose();
            CurrentMessage = new Message(duration, message);
        }

        protected override void Dispose(bool disposing)
        {
            if (Disposed) return;

            if (disposing)
            {
                try
                {
                    RemainingTasks = int.MaxValue;
                    Arrows.ForEach(a => a.Dispose());
                    Arrows.Clear();
                    AnnounceSound?.Destroy();
                    AnnounceSound = null;
                    CurrentMessage?.Dispose();
                    CurrentMessage = null;
                }
                catch
                {
                }
            }
            base.Dispose(disposing);
        }
    }
}
