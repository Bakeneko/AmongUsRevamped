using System;

namespace AmongUsRevamped.Extensions
{
    public static class EventHandlerExtensions
    {
        /// <summary>
        /// Safely invokes event handlers by catching exceptions. Should mainly be used in game patches to prevent an exception from causing the game to hang.
        /// </summary>
        /// <typeparam name="T">Event arguments type</typeparam>
        /// <param name="eventHandler">Event to invoke</param>
        /// <param name="sender">Object invoking the event</param>
        /// <param name="args">Event arguments</param>
        public static void SafeInvoke<T>(this EventHandler<T> eventHandler, object sender, T args) where T : EventArgs
        {
            SafeInvoke(eventHandler, sender, args, eventHandler.GetType().Name);
        }

        /// <summary>
        /// Safely invokes event handlers by catching exceptions. Should mainly be used in game patches to prevent an exception from causing the game to hang.
        /// </summary>
        /// <typeparam name="T">Event arguments type</typeparam>
        /// <param name="eventHandler">Event to invoke</param>
        /// <param name="sender">Object invoking the event</param>
        /// <param name="args">Event arguments</param>
        /// <param name="eventName">Event name (logged in errors)</param>
        public static void SafeInvoke<T>(this EventHandler<T> eventHandler, object sender, T args, string eventName) where T : EventArgs
        {
            if (eventHandler == null) return;

            Delegate[] handlers = eventHandler.GetInvocationList();
            for (int i = 0; i < handlers.Length; i++)
            {
                try
                {
                    ((EventHandler<T>)handlers[i])?.Invoke(sender, args);
                }
                catch (Exception ex)
                {
                    AmongUsRevamped.Logger.LogWarning($"Exception in event handler index {i} for event \"{eventName}\": {ex}");
                }
            }
        }
    }
}
