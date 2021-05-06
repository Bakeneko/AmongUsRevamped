using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnhollowerBaseLib;
using UnityEngine;

using BindingFlags = Il2CppSystem.Reflection.BindingFlags;

namespace AmongUsRevamped
{
    public static class Coroutines
    {
        private struct CoroutineTuple
        {
            public object WaitCondition;
            public IEnumerator Coroutine;
        }

        private class Il2CppEnumeratorWrapper : IEnumerator
        {
            private readonly Il2CppSystem.Collections.IEnumerator _il2CPPEnumerator;

            public Il2CppEnumeratorWrapper(Il2CppSystem.Collections.IEnumerator il2CppEnumerator) => _il2CPPEnumerator = il2CppEnumerator;
            public bool MoveNext() => _il2CPPEnumerator.MoveNext();
            public void Reset() => _il2CPPEnumerator.Reset();
            public object Current => _il2CPPEnumerator.Current;
        }

        [RegisterInIl2Cpp]
        internal class Component : MonoBehaviour
        {
            public Component(IntPtr ptr) : base(ptr)
            {
            }

            private void Start()
            {
                Camera.onPostRender = Camera.onPostRender == null
                    ? new Action<Camera>(OnPostRenderM)
                    : Il2CppSystem.Delegate.Combine(Camera.onPostRender, Il2CppSystem.Delegate.CreateDelegate(GetIl2CppType(), GetIl2CppType().GetMethod(nameof(OnPostRenderM), BindingFlags.Static | BindingFlags.Public))).Cast<Camera.CameraCallback>();
            }

            private void FixedUpdate()
            {
                ProcessCoroutineList(_ourWaitForFixedUpdateCoroutines);
            }

            private void Update()
            {
                Process();
            }

            public static Camera OnPostRenderCam { get; private set; }

            private static void OnPostRenderM(Camera camera)
            {
                if (OnPostRenderCam == null)
                {
                    OnPostRenderCam = camera;
                }

                if (OnPostRenderCam == camera)
                {
                    ProcessCoroutineList(_ourWaitForEndOfFrameCoroutines);
                }
            }
        }

        private static readonly List<CoroutineTuple> _ourCoroutinesStore = new();
        private static readonly List<IEnumerator> _ourNextFrameCoroutines = new();
        private static readonly List<IEnumerator> _ourWaitForFixedUpdateCoroutines = new();
        private static readonly List<IEnumerator> _ourWaitForEndOfFrameCoroutines = new();

        private static readonly List<IEnumerator> _tempList = new();

        public static IEnumerator Start(IEnumerator routine)
        {
            if (routine != null) ProcessNextOfCoroutine(routine);
            return routine;
        }

        public static void Stop(IEnumerator enumerator)
        {
            if (_ourNextFrameCoroutines.Contains(enumerator)) //Coroutine is running itself
            {
                _ourNextFrameCoroutines.Remove(enumerator);
            }
            else
            {
                var coroutineTupleIndex = _ourCoroutinesStore.FindIndex(c => c.Coroutine == enumerator);
                if (coroutineTupleIndex != -1) // Coroutine is waiting for a subroutine
                {
                    var waitCondition = _ourCoroutinesStore[coroutineTupleIndex].WaitCondition;
                    if (waitCondition is IEnumerator waitEnumerator)
                    {
                        Stop(waitEnumerator);
                    }

                    _ourCoroutinesStore.RemoveAt(coroutineTupleIndex);
                }
            }
        }

        private static void ProcessCoroutineList(List<IEnumerator> target)
        {
            if (target.Count == 0) return;

            // Use a temp list to make sure waits made during processing are not handled by same processing invocation.
            // Additionally, a temp list reduces allocations compared to an array.
            _tempList.AddRange(target);
            target.Clear();
            foreach (var enumerator in _tempList) ProcessNextOfCoroutine(enumerator);
            _tempList.Clear();
        }

        private static void Process()
        {
            for (var i = _ourCoroutinesStore.Count - 1; i >= 0; i--)
            {
                var tuple = _ourCoroutinesStore[i];
                if (tuple.WaitCondition is WaitForSeconds waitForSeconds)
                {
                    if ((waitForSeconds.m_Seconds -= Time.deltaTime) <= 0)
                    {
                        _ourCoroutinesStore.RemoveAt(i);
                        ProcessNextOfCoroutine(tuple.Coroutine);
                    }
                }
            }

            ProcessCoroutineList(_ourNextFrameCoroutines);
        }

        private static void ProcessNextOfCoroutine(IEnumerator enumerator)
        {
            try
            {
                if (!enumerator.MoveNext()) // Run the next step of the coroutine. If it's done, restore the parent routine
                {
                    var indices = _ourCoroutinesStore.Select((it, idx) => (idx, it)).Where(it => it.it.WaitCondition == enumerator).Select(it => it.idx).ToList();
                    for (var i = indices.Count - 1; i >= 0; i--)
                    {
                        var index = indices[i];
                        _ourNextFrameCoroutines.Add(_ourCoroutinesStore[index].Coroutine);
                        _ourCoroutinesStore.RemoveAt(index);
                    }

                    return;
                }
            }
            catch (Exception ex)
            {
                AmongUsRevamped.LogError($"An exception has occurred processing a coroutine:\r\n{ex}");
                Stop(FindOriginalCoroutine(enumerator)); // Stop the entire coroutine hierarchy
            }

            var next = enumerator.Current;
            switch (next)
            {
                case null:
                    _ourNextFrameCoroutines.Add(enumerator);
                    return;
                case WaitForFixedUpdate _:
                    _ourWaitForFixedUpdateCoroutines.Add(enumerator);
                    return;
                case WaitForEndOfFrame _:
                    _ourWaitForEndOfFrameCoroutines.Add(enumerator);
                    return;
                case WaitForSeconds _:
                    break; // Do nothing, this one is supported in Process
                case Il2CppObjectBase il2CppObjectBase:
                    var nextAsEnumerator = il2CppObjectBase.TryCast<Il2CppSystem.Collections.IEnumerator>();
                    if (nextAsEnumerator != null) // Il2cpp IEnumerator also handles CustomYieldInstruction
                        next = new Il2CppEnumeratorWrapper(nextAsEnumerator);
                    else
                        AmongUsRevamped.LogWarning($"Unknown coroutine yield object of type {il2CppObjectBase} for coroutine {enumerator}");
                    break;
            }

            _ourCoroutinesStore.Add(new CoroutineTuple { WaitCondition = next, Coroutine = enumerator });

            if (next is IEnumerator nextCoroutine)
                ProcessNextOfCoroutine(nextCoroutine);
        }

        private static IEnumerator FindOriginalCoroutine(IEnumerator enumerator)
        {
            var index = _ourCoroutinesStore.FindIndex(ct => ct.WaitCondition == enumerator);
            return index == -1 ? enumerator : FindOriginalCoroutine(_ourCoroutinesStore[index].Coroutine);
        }
    }
}
