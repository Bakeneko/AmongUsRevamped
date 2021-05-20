using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using AmongUsRevamped.Colors;
using AmongUsRevamped.Extensions;
using AmongUsRevamped.Mod.Modifiers;
using AmongUsRevamped.Mod.Roles;
using AmongUsRevamped.UI;
using AmongUsRevamped.Utils;
using HarmonyLib;
using UnityEngine;

namespace AmongUsRevamped.Mod
{
    [HarmonyPatch]
    public partial class Game
    {
        private static float BasePlayerSpeed = AmongUsClient.Instance?.PlayerPrefab?.MyPhysics?.Speed ?? 2.5f;

        private static GameOverData GameOver = null;

        private static bool Anonymized = false;

        private static void OnExit()
        {
            Reset();
        }

        private static void OnLobbyStart(LobbyBehaviour lobby)
        {
            Reset();
        }

        private static void Reset()
        {
            Anonymized = false;
            StopNameScrambler();
            ScrambledNames.Clear();
            BasePlayerSpeed = AmongUsClient.Instance?.PlayerPrefab?.MyPhysics?.Speed ?? 2.5f;
            GameOver = null;
            Role.AllRoles.ForEach(r => r.Dispose());
            Modifier.AllModifiers.ForEach(m => m.Dispose());
        }

        private static bool OnBegin()
        {
            return true;
        }

        private static bool CalculateLightRadius(ShipStatus shipStatus, Player player, ref float radius)
        {
            ISystemType systemType = shipStatus.Systems.ContainsKey(SystemTypes.Electrical) ? shipStatus.Systems[SystemTypes.Electrical] : null;
            SwitchSystem switchSystem = systemType?.TryCast<SwitchSystem>();
            if (switchSystem == null) return true;

            float visionRange = player.VisionRange;
            if (visionRange <= 0) _ = PlayerControl.GameOptions.CrewLightMod;

            float light = switchSystem.Value / 255f;

            if (player == null || player.Dead) // Ghost
            {
                visionRange = 1f;
                light = 1f;
            }
            else if (player.HasNightVision) // Role or modifier with night vision
            {
                light = 1f;
            }

            radius = Mathf.Lerp(shipStatus.MinLightRadius, shipStatus.MaxLightRadius, light) * visionRange;

            return false;
        }

        private static void OnGameStart()
        {

        }

        private static void OnPlayerStart(Player player)
        {

        }

        private static void OnPlayerFixedUpdate(Player player)
        {
            player?.OnFixedUpdate();
        }

        /// <summary>
        /// Mess with player physic
        /// </summary>
        private static void OnPlayerPhysicsFixedUpdate(Player player, PlayerPhysics physics)
        {
            if (physics.AmOwner)
            {
                physics.body.velocity *= player?.MoveSpeed ?? 1f;
            }
        }

        /// <summary>
        /// Mess with player movement
        /// </summary>
        private static void OnPlayerNetworkTransformFixedUpdate(Player player, CustomNetworkTransform networkTransform)
        {
            if (!networkTransform.AmOwner)
            {
                networkTransform.body.velocity *= player?.MoveSpeed ?? 1f;
            }
        }

        private static void OnPingTrackerUpdate(PingTracker pingTracker)
        {
            pingTracker.text.fontSize = 2.5f;
            pingTracker.text.transform.localPosition = new HudPosition(2.5f, 0.35f, HudAlignment.TopRight);

            pingTracker.text.text = ColorPalette.Color.Revamped.ToColorTag($"{AmongUsRevamped.Name} v{AmongUsRevamped.VersionString}");
            pingTracker.text.text += $"\nPing: {AmongUsClient.Instance.Ping} ms";
        }

        private static void OnHudStart(HudManager hudManager)
        {

        }

        private static void OnHudUpdate(HudManager hudManager)
        {
            foreach(Player p in Player.AllPlayers) { p.HudUpdate(hudManager); };
            Player.CurrentPlayerHudUpdate(hudManager);

            var anonymize = (Options.Values.AnonOnCommsSabotage && !AreCommsActive()) || Role.GetRoles<Camouflager>(RoleType.Camouflager).FirstOrDefault(c => c.CamouflageActive) != null;

            if (anonymize && !Anonymized)
            {
                Anonymized = true;
                StartNameScrambler();
            }
            
            if (Anonymized)
            {
                foreach (Role r in Role.AllRoles)
                {
                    if (!ScrambledNames.TryGetValue(r.Player.Id, out string name)) name = "";

                    if (r.RoleType == RoleType.Swooper && (r as Swooper).Swooping)
                    {
                        r.Disguise.Name = name;
                        r.Disguise.Color = Color.grey;
                        r.Disguise.PetRenderColor = Color.clear;
                        r.Disguise.OtherRenderColor = Color.clear;
                        r.ApplyDisguise();
                        continue;
                    }

                    r.Disguise = new Disguise(name, r.Player.Data.ColorId, Color.grey, r.Player.Data.HatId, r.Player.Data.SkinId, r.Player.Data.PetId)
                    {
                        PetRenderColor = Color.clear,
                        OtherRenderColor = Color.clear
                    };
                    r.ApplyDisguise();
                }
            }
            
            if (!anonymize && Anonymized)
            {
                Anonymized = false;
                StopNameScrambler();

                foreach (Role r in Role.AllRoles)
                {
                    if (r.RoleType == RoleType.Swooper && (r as Swooper).Swooping) continue;

                    r.Disguise = null;
                    r.ApplyDisguise();
                }

                Role.GetRoles<Morphling>(RoleType.Morphling).ForEach(r => r.MorphUpdate());
            }
        }

        private static void OnDistributeRoles()
        {
            if (!AmongUsClient.Instance.AmHost || DestroyableSingleton<TutorialManager>.InstanceExists) return;

            List<Player> crewmates = new(), impostors = new(), players = Player.AllPlayers.ToList();
            foreach (Player p in players)
            {
                if (p.IsImpostor) impostors.Add(p);
                else crewmates.Add(p);
            }

            // Distribute crewmates roles
            int maxCrewmateRoles = Mathf.Min(crewmates.Count, Options.Values.MaxCrewmateRoles);
            var generator = new DistributedRandomNumberGenerator<byte>();
            if (Options.Values.EngineerSpawnRate > 0) generator.AddNumber((byte)RoleType.Engineer, Options.Values.EngineerSpawnRate);
            if (Options.Values.SheriffSpawnRate > 0) generator.AddNumber((byte)RoleType.Sheriff, Options.Values.SheriffSpawnRate);
            if (Options.Values.SnitchSpawnRate > 0) generator.AddNumber((byte)RoleType.Snitch, Options.Values.SnitchSpawnRate);
            if (Options.Values.SpySpawnRate > 0) generator.AddNumber((byte)RoleType.Spy, Options.Values.SpySpawnRate);
            if (Options.Values.TimeLordSpawnRate > 0) generator.AddNumber((byte)RoleType.TimeLord, Options.Values.TimeLordSpawnRate);

            for (int i = 0; i < maxCrewmateRoles; i++)
            {
                if (generator.GetNumberCount() == 0) break; // No more special roles

                var role = generator.GetDistributedRandomNumber();
                generator.RemoveNumber(role);
                AssignRoleRandomly((RoleType)role, crewmates);
            }

            // Distribute neutral roles
            int maxNeutralRoles = Mathf.Min(crewmates.Count, Options.Values.MaxNeutralRoles);
            generator = new DistributedRandomNumberGenerator<byte>();
            if (Options.Values.JesterSpawnRate > 0) generator.AddNumber((byte)RoleType.Jester, Options.Values.JesterSpawnRate);

            for (int i = 0; i < maxNeutralRoles; i++)
            {
                if (generator.GetNumberCount() == 0) break; // No more special roles

                var role = generator.GetDistributedRandomNumber();
                generator.RemoveNumber(role);
                AssignRoleRandomly((RoleType)role, crewmates);
            }

            // Distribute base crewmate role
            crewmates.ForEach(p => AssignPlayerRole(p, RoleType.Crewmate));

            // Distribute impostor roles
            int maxImpostorRoles = Mathf.Min(impostors.Count, Options.Values.MaxImpostorRoles);
            generator = new DistributedRandomNumberGenerator<byte>();
            if (Options.Values.CamouflagerSpawnRate > 0) generator.AddNumber((byte)RoleType.Camouflager, Options.Values.CamouflagerSpawnRate);
            if (Options.Values.CleanerSpawnRate > 0) generator.AddNumber((byte)RoleType.Cleaner, Options.Values.CleanerSpawnRate);
            if (Options.Values.MorphlingSpawnRate > 0) generator.AddNumber((byte)RoleType.Morphling, Options.Values.MorphlingSpawnRate);
            if (Options.Values.SwooperSpawnRate > 0) generator.AddNumber((byte)RoleType.Swooper, Options.Values.SwooperSpawnRate);

            for (int i = 0; i < maxImpostorRoles; i++)
            {
                if (generator.GetNumberCount() == 0) break; // No more special roles

                var role = generator.GetDistributedRandomNumber();
                generator.RemoveNumber(role);
                AssignRoleRandomly((RoleType)role, impostors);
            }

            // Distribute base impostor role
            impostors.ForEach(p => AssignPlayerRole(p, RoleType.Impostor));

            int maxModifiers = Mathf.Min(players.Count, Options.Values.MaxModifiers);
            generator = new DistributedRandomNumberGenerator<byte>();
            if (Options.Values.DrunkSpawnRate > 0) generator.AddNumber((byte)ModifierType.Drunk, Options.Values.DrunkSpawnRate);
            if (Options.Values.FlashSpawnRate > 0) generator.AddNumber((byte)ModifierType.Flash, Options.Values.FlashSpawnRate);
            if (Options.Values.GiantSpawnRate > 0) generator.AddNumber((byte)ModifierType.Giant, Options.Values.GiantSpawnRate);
            if (Options.Values.TinySpawnRate > 0) generator.AddNumber((byte)ModifierType.Tiny, Options.Values.TinySpawnRate);
            if (Options.Values.TorchSpawnRate > 0) generator.AddNumber((byte)ModifierType.Torch, Options.Values.TorchSpawnRate);

            for (int i = 0; i < maxModifiers; i++)
            {
                if (generator.GetNumberCount() > 0)
                {
                    var role = generator.GetDistributedRandomNumber();
                    generator.RemoveNumber(role);
                    AssignModifierRandomly((ModifierType)role, players);
                }
            }
        }

        public static void AssignRoleRandomly(RoleType type, List<Player> players)
        {
            int index = AmongUsRevamped.Rand.Next(0, players.Count);
            Player player = players[index];
            players.RemoveAt(index);
            AssignPlayerRole(player, type);
        }

        public static void AssignPlayerRole(Player player, RoleType type)
        {
            RoleAssignationRpc.Instance.Send(new Tuple<byte, byte>(player.Id, (byte)type));
            OnPlayerRoleAssigned(player, type);
        }

        public static void AssignModifierRandomly(ModifierType type, List<Player> players)
        {
            int index = AmongUsRevamped.Rand.Next(0, players.Count);
            Player player = players[index];
            AssignPlayerModifier(player, type);
        }

        public static void AssignPlayerModifier(Player player, ModifierType type)
        {
            ModifierAssignationRpc.Instance.Send(new Tuple<byte, byte>(player.Id, (byte)type));
            OnPlayerModifierAssigned(player, type);
        }

        private static void OnPlayerRoleAssigned(Player player, RoleType type)
        {
            switch (type)
            {
                case RoleType.Crewmate:
                    new Crewmate(player);
                    break;
                case RoleType.Engineer:
                    new Engineer(player);
                    break;
                case RoleType.Sheriff:
                    new Sheriff(player);
                    break;
                case RoleType.Snitch:
                    new Snitch(player);
                    break;
                case RoleType.Spy:
                    new Spy(player);
                    break;
                case RoleType.TimeLord:
                    new TimeLord(player);
                    break;
                case RoleType.Camouflager:
                    new Camouflager(player);
                    break;
                case RoleType.Cleaner:
                    new Cleaner(player);
                    break;
                case RoleType.Impostor:
                    new Impostor(player);
                    break;
                case RoleType.Morphling:
                    new Morphling(player);
                    break;
                case RoleType.Swooper:
                    new Swooper(player);
                    break;
                case RoleType.Jester:
                    new Jester(player);
                    break;
                default:
                    AmongUsRevamped.LogWarning($"Player {player} was assigned unhandled role {type}");
                    break;
            }
            player.UpdateImportantTasks();
        }

        private static void OnPlayerModifierAssigned(Player player, ModifierType type)
        {
            switch (type)
            {
                case ModifierType.Drunk:
                    new Drunk(player);
                    break;
                case ModifierType.Flash:
                    new Flash(player);
                    break;
                case ModifierType.Giant:
                    new Giant(player);
                    break;
                case ModifierType.Tiny:
                    new Tiny(player);
                    break;
                case ModifierType.Torch:
                    new Torch(player);
                    break;
                default:
                    AmongUsRevamped.LogWarning($"Player {player} was assigned unhandled modifier {type}");
                    break;
            }
            player.UpdateImportantTasks();
        }

        private static void OnIntroStart(IntroCutscene introCutScene, ref Il2CppSystem.Collections.Generic.List<PlayerControl> team)
        {
            Player.OnIntroStart(introCutScene, ref team);
        }

        private static void OnIntroUpdate(IntroCutscene introCutScene)
        {
            Player.OnIntroUpdate(introCutScene);
        }

        private static void OnIntroEnd(IntroCutscene introCutScene)
        {
            foreach (Player p in Player.AllPlayers) { p.OnIntroEnd(introCutScene); };
        }

        private static void OnExileBegin(Player exiled, bool tie)
        {
            exiled?.OnExiled();
        }

        private static void OnExileEnd(ExileController exileController)
        {
            foreach (Player p in Player.AllPlayers) { p.OnExileEnd(exileController); };
        }

        private static void OnEmergencyButtonUpdate(EmergencyMinigame emButton)
        {
            // Check whether role can call for a meeting or not
            Role role = Player.CurrentPlayer?.Role;
            if (role?.CanCallMeeting() ?? true) return;

            emButton.StatusText.text = $"{role.Name} can't call for a emergency meeting";
            emButton.NumberText.text = string.Empty;
            emButton.ClosedLid.gameObject.SetActive(true);
            emButton.OpenLid.gameObject.SetActive(false);
            emButton.ButtonActive = false;
        }

        /// <summary>
        /// Update med scan info
        /// </summary>
        private static void OnMedScanMinigameBegin(MedScanMinigame medScan)
        {
            var player = Player.CurrentPlayer;
            var size = player.Size;
            var data = medScan.completeString;

            string sizePattern = @":( ?\d)'(( ?\d)"")?";
            Match sizeMatch = Regex.Match(data, sizePattern, RegexOptions.Multiline);
            var feet = 0f;
            var inches = 0f;
            if (sizeMatch.Groups.Count > 1)
            {
                feet = float.Parse(sizeMatch.Groups[1].Value);
            }
            if (sizeMatch.Groups.Count > 3)
            {
                inches = float.Parse(sizeMatch.Groups[3].Value);
            }
            feet = (feet + inches * 0.083333f) * size;
            inches = Mathf.Round(feet % 1 / 0.083333f);
            feet = Mathf.Floor(feet);
            data = Regex.Replace(data, sizePattern, $":{feet}'{(inches > 0 ? $" {inches}\"" : "")}");

            string weightPattern = @":( ?\d+)lb";
            Match weightMatch = Regex.Match(data, weightPattern, RegexOptions.Multiline);
            var weight = 0f;
            if (weightMatch.Groups.Count > 1)
            {
                weight = float.Parse(weightMatch.Groups[1].Value);
            }
            weight = Mathf.Round(weight * size);

            data = Regex.Replace(data, weightPattern, $":{weight:0}lb");
            medScan.completeString = data;
        }

        public static bool AreCommsActive()
        {
            if (ShipStatus.Instance == null) return true;

            var commsSys = ShipStatus.Instance.Systems.ContainsKey(SystemTypes.Comms) ? ShipStatus.Instance.Systems[SystemTypes.Comms] : null;
            if (PlayerControl.GameOptions.MapId == 1)
            {
                var comms = commsSys.TryCast<HqHudSystemType>();
                return comms?.IsActive != true; // IsActive means sabotaged
            }
            else
            {
                var comms = commsSys.TryCast<HudOverrideSystemType>();
                return comms?.IsActive != true; // IsActive means sabotaged
            }
        }

        private static bool OnAdminPanelUpdate(MapCountOverlay overlay, ref Dictionary<SystemTypes, List<Color>> telemetry)
        {
            if (Role.GetPlayerRole<Spy>(Player.CurrentPlayer.Id) == null) return true;

            overlay.timer += Time.deltaTime;

            if (overlay.timer < 0.1f) return false;

            overlay.timer = 0f;
            telemetry = new Dictionary<SystemTypes, List<Color>>();

            var commsActive = AreCommsActive();

            if (!overlay.isSab && !commsActive)
            {
                overlay.isSab = true;
                overlay.BackgroundColor.SetColor(Palette.DisabledGrey);
                overlay.SabotageText.gameObject.SetActive(true);
                return false;
            }
            else if (overlay.isSab && commsActive)
            {
                overlay.isSab = false;
                overlay.BackgroundColor.SetColor(Color.green);
                overlay.SabotageText.gameObject.SetActive(false);
            }

            for (int i = 0; i < overlay.CountAreas.Length; i++)
            {
                CounterArea counterArea = overlay.CountAreas[i];
                List<Color> roomColors = new();
                telemetry.Add(counterArea.RoomType, roomColors);

                if (!commsActive)
                {
                    counterArea.UpdateCount(0);
                    continue;
                }

                PlainShipRoom plainShipRoom = ShipStatus.Instance.FastRooms[counterArea.RoomType];

                if (plainShipRoom?.roomArea == null) continue;

                int resCount = plainShipRoom.roomArea.OverlapCollider(overlay.filter, overlay.buffer);
                int count = resCount;
                for (int j = 0; j < resCount; j++)
                {
                    Collider2D obj = overlay.buffer[j];
                    if (obj.tag != "DeadBody")
                    {
                        PlayerControl player = obj.GetComponent<PlayerControl>();
                        if (!player || player.Data == null || player.Data.Disconnected || player.Data.IsDead)
                        {
                            count--;
                        }
                        else if (player?.myRend?.material != null)
                        {
                            roomColors.Add(player.myRend.material.GetColor("_BodyColor"));
                        }
                    }
                    else
                    {
                        DeadBody body = obj.GetComponent<DeadBody>();
                        if (body == null) continue;

                        GameData.PlayerInfo playerInfo = GameData.Instance.GetPlayerById(body.ParentId);
                        if (playerInfo != null)
                        {
                            roomColors.Add(Palette.PlayerColors[playerInfo.ColorId]);
                        }
                    }
                }
                counterArea.UpdateCount(count);
            }

            return false;
        }

        private static void OnAdminPanelUpdateCount(CounterArea counterArea, Dictionary<SystemTypes, List<Color>> telemetry, ref Material defaultMat, ref Material colorMat)
        {
            if (!telemetry.ContainsKey(counterArea.RoomType)) return;

            var spy = Role.GetPlayerRole<Spy>(Player.CurrentPlayer.Id);

            List<Color> colors = telemetry[counterArea.RoomType];

            for (int i = 0; i < counterArea.myIcons.Count; i++)
            {
                PoolableBehavior icon = counterArea.myIcons[i];
                SpriteRenderer rend = icon.GetComponent<SpriteRenderer>();

                if (rend == null) continue;

                if (defaultMat == null) defaultMat = rend.material;
                if (colorMat == null) colorMat = UnityEngine.Object.Instantiate(defaultMat);

                if (colors.Count > i && spy?.GadgetActive == true)
                {
                    rend.material = colorMat;
                    var color = colors[i];
                    rend.material.SetColor("_BodyColor", color);
                    var id = Palette.PlayerColors.IndexOf(color);
                    if (id < 0)
                    {
                        rend.material.SetColor("_BackColor", color);
                    }
                    else
                    {
                        rend.material.SetColor("_BackColor", Palette.ShadowColors[id]);
                    }
                    rend.material.SetColor("_VisorColor", Palette.VisorColor);
                }
                else
                {
                    rend.material = defaultMat;
                }
            }
        }

        private static bool OnPlayerCanUseConsole(Player player, Console console, ref float distance, out bool canUse, out bool couldUse)
        {
            canUse = couldUse = false;

            // Prevent roles faking tasks to actually complete them
            if (console.AllowImpostor || !player.FakesTasks) return true;

            distance = float.MaxValue;

            return false;
        }

        private static void OnPlayerCanUseVent(Player player, Vent vent, ref float distance, out bool canUse, out bool couldUse)
        {
            float dist = float.MaxValue;
            var usableDistance = vent.UsableDistance;
            canUse = couldUse = player?.CanUseVent(vent) ?? false;

            if (canUse)
            {
                Vector2 truePosition = player.Control.GetTruePosition();
                Vector3 position = vent.transform.position;
                dist = Vector2.Distance(truePosition, position);
                canUse &= dist <= usableDistance && !PhysicsHelpers.AnythingBetween(truePosition, position, Constants.ShipOnlyMask, false);
            }
            
            distance = dist;
        }

        public static void PlayerMurderedPlayer(Player killer, Player target)
        {
            if (killer == null && target == null) return;
            MurderRpc.Instance.Send(new Tuple<byte, byte>(killer.Id, target.Id));
        }

        private static void OnPlayerMurderedPlayer(Player killer, Player victim)
        {
            victim.OnMurdered(killer);
        }

        public static void RevivePlayer(Player revived)
        {
            if (revived == null) return;
            ReviveRpc.Instance.Send(revived.Id);
        }

        public static void RemoveBody(DeadBody body)
        {
            if (body == null) return;
            RemoveBodyRpc.Instance.Send(body.ParentId);
            Coroutines.Start(RemoveBodyCoroutine(body.ParentId));
        }

        private static bool OnPlayerCallMeeting(Player player)
        {
            return true;
        }

        private static void OnPlayerCalledMeeting(Player player)
        {

        }

        private static bool OnPlayerReportBody(Player player, Player victim)
        {
            return true;
        }

        private static void OnPlayerReportedBody(Player player, Player victim)
        {

        }

        private static bool OnPlayerDie(Player player)
        {

            return true;
        }

        private static void OnPlayerCompletedTask(Player player, GameData.TaskInfo task)
        {
            player?.OnCompletedTask(task);
        }

        private static bool OnPlayerExile(Player player)
        {
            return true;
        }

        private static void OnPlayerExiled(Player player)
        {

        }

        private static string GetTextForExile(Player player, StringNames id)
        {
            if (player == null) return null;

            var role = player.Role;

            // Crewmate exile
            if (id == StringNames.ExileTextPN || id == StringNames.ExileTextSN)
            {
                if (role != null) return role.ExileDescription();
                return player.Name + " was not The Impostor.";
            }

            // Impostor exile
            if (id == StringNames.ExileTextPP || id == StringNames.ExileTextSP)
            {
                if (role != null) return role.ExileDescription();
                return player.Name + " was The Impostor.";
            }

            return null;
        }
    }
}
