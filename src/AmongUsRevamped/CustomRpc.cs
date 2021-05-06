using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Reflection;
using HarmonyLib;
using Hazel;
using Il2CppSystem.Security.Cryptography.X509Certificates;
using InnerNet;

namespace AmongUsRevamped
{
    public enum RpcLocalHandling
    {
        None,
        Before,
        After
    }

    public abstract class UnsafeCustomRpc
    {
        internal CustomRpcManager Manager { get; set; }

        public uint Id { get; }

        public abstract Type InnerNetObjectType { get; }

        public virtual SendOption SendOption { get; } = SendOption.Reliable;
        public virtual RpcLocalHandling LocalHandling { get; } = RpcLocalHandling.None;

        protected UnsafeCustomRpc(uint id)
        {
            Id = id;
        }

        public abstract void UnsafeWrite(MessageWriter writer, object data);
        public abstract object UnsafeRead(MessageReader reader);
        public abstract void UnsafeHandle(InnerNetObject innerNetObject, object data);

        public void UnsafeSend(InnerNetObject netObject, object data, bool immediately = false, int targetClientId = -1)
        {
            if (netObject == null) throw new ArgumentNullException(nameof(netObject));

            if (Manager == null)
            {
                throw new InvalidOperationException("Can't send unregistered CustomRpc");
            }

            if (LocalHandling == RpcLocalHandling.Before)
            {
                UnsafeHandle(netObject, data);
            }

            var writer = immediately switch
            {
                false => AmongUsClient.Instance.StartRpc(netObject.NetId, CustomRpcManager.CallId, SendOption),
                true => AmongUsClient.Instance.StartRpcImmediately(netObject.NetId, CustomRpcManager.CallId, SendOption, targetClientId)
            };

            writer.WritePacked(Id);

            writer.StartMessage(0);
            UnsafeWrite(writer, data);
            writer.EndMessage();

            if (immediately)
            {
                AmongUsClient.Instance.FinishRpcImmediately(writer);
            }
            else
            {
                writer.EndMessage();
            }

            if (LocalHandling == RpcLocalHandling.After)
            {
                UnsafeHandle(netObject, data);
            }
        }
    }

    public abstract class CustomRpc<TInnerNetObject, TData> : UnsafeCustomRpc where TInnerNetObject : InnerNetObject
    {
        protected CustomRpc(uint id) : base(id)
        {
        }

        public override Type InnerNetObjectType => typeof(TInnerNetObject);

        public abstract void Write(MessageWriter writer, TData data);
        public abstract TData Read(MessageReader reader);
        public abstract void Handle(TInnerNetObject innerNetObject, TData data);

        public override void UnsafeWrite(MessageWriter writer, object data)
        {
            Write(writer, (TData)data);
        }

        public override object UnsafeRead(MessageReader reader)
        {
            return Read(reader);
        }

        public override void UnsafeHandle(InnerNetObject innerNetObject, object data)
        {
            Handle((TInnerNetObject)innerNetObject, (TData)data);
        }

        public void Send(InnerNetObject netObject, TData data, bool immediately = false)
        {
            UnsafeSend(netObject, data, immediately);
        }

        public void SendTo(InnerNetObject netObject, int targetId, TData data)
        {
            UnsafeSend(netObject, data, true, targetId);
        }
    }

    public abstract class PlayerCustomRpc<TData> : CustomRpc<PlayerControl, TData>
    {
        protected PlayerCustomRpc(uint id) : base(id)
        {
        }

        public void Send(TData data, bool immediately = false)
        {
            Send(PlayerControl.LocalPlayer, data, immediately);
        }

        public void SendTo(int targetId, TData data)
        {
            SendTo(PlayerControl.LocalPlayer, targetId, data);
        }
    }

    public class CustomRpcManager
    {
        public const byte CallId = byte.MaxValue;

        private readonly Dictionary<uint, UnsafeCustomRpc> _idMap = new();
        private readonly Dictionary<Type, List<UnsafeCustomRpc>> _typeMap = new();

        public IReadOnlyDictionary<uint, UnsafeCustomRpc> IdMap => new ReadOnlyDictionary<uint, UnsafeCustomRpc>(_idMap);

        public ILookup<Type, UnsafeCustomRpc> TypeMap =>
            _typeMap.SelectMany(pair => pair.Value, (pair, value) => new { pair.Key, Value = value })
                .ToLookup(pair => pair.Key, pair => pair.Value);

        public CustomRpcManager()
        {
            foreach (var type in HandleRpcPatch.InnerNetObjectTypes)
            {
                _typeMap[type] = new List<UnsafeCustomRpc>();
            }
        }

        public UnsafeCustomRpc Register(UnsafeCustomRpc customRpc)
        {
            if (_idMap.ContainsKey(customRpc.Id)) {
                throw new ArgumentException("Rpc with that id was already registered");
            }

            customRpc.Manager = this;
            _idMap.Add(customRpc.Id, customRpc);
            _typeMap[customRpc.InnerNetObjectType].Add(customRpc);

            typeof(Rpc<>).MakeGenericType(customRpc.GetType()).GetProperty("Instance")!.SetValue(null, customRpc);

            return customRpc;
        }

        [HarmonyPrefix]
        [HarmonyPatch(typeof(X509Certificate2Collection), nameof(X509Certificate2Collection.Contains))]
        private static bool AcceptCustomCertificatesPatch(X509Certificate2Collection __instance, ref bool __result)
        {
            if (AuthManager.Instance && AuthManager.Instance.connection != null && AuthManager.Instance.connection.serverCertificates.Equals(__instance))
            {
                __result = true;
                return false;
            }
            return true;
        }

        [HarmonyPatch]
        private static class HandleRpcPatch
        {
            internal static List<Type> InnerNetObjectTypes { get; } = typeof(InnerNetObject).Assembly.GetTypes().Where(x => x.IsSubclassOf(typeof(InnerNetObject)) && x != typeof(LobbyBehaviour)).ToList();

            public static IEnumerable<MethodBase> TargetMethods()
            {
                return InnerNetObjectTypes.Select(x => x.GetMethod(nameof(InnerNetObject.HandleRpc), AccessTools.all));
            }

            public static bool Prefix(InnerNetObject __instance, [HarmonyArgument(0)] byte callId, [HarmonyArgument(1)] MessageReader reader)
            {
                if (callId == CallId)
                {
                    var manager = AmongUsRevamped.Instance.CustomRpcManager;

                    var id = reader.ReadPackedUInt32();

                    // Custom rpc not found, should not happen
                    if (!manager.IdMap.TryGetValue(id, out UnsafeCustomRpc customRpc)) return false;

                    customRpc.UnsafeHandle(__instance, customRpc.UnsafeRead(reader.ReadMessage()));

                    return false;
                }

                return true;
            }
        }
    }

    public static class Rpc<T> where T : UnsafeCustomRpc
    {
        private static T _instance;

        public static T Instance
        {
            get
            {
                if (_instance == null)
                {
                    throw new Exception($"{typeof(T).FullName} isn't registered");
                }

                return _instance;
            }

            internal set
            {
                if (_instance != null)
                {
                    throw new Exception($"{typeof(T).FullName} is already registered");
                }

                _instance = value;
            }
        }
    }
}
