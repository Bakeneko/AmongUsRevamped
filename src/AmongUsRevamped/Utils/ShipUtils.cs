using System.Linq;
using UnityEngine;

namespace AmongUsRevamped.Utils
{
    public static class ShipUtils
    {
        /// <summary>
        /// Get the ship system for a given position
        /// </summary>
        /// <returns>Corresponding <see cref="SystemTypes"/></returns>
        public static SystemTypes GetSystem(Vector2 position)
        {
            var room = ShipStatus.Instance.AllRooms.FirstOrDefault(room => room.roomArea?.OverlapPoint(position) ?? false);
            return room != default ? room.RoomId : SystemTypes.Hallway;
        }

        /// <summary>
        /// Get the system name
        /// </summary>
        public static string GetSystemName(int system)
        {
            return system == 0 ? "Button" : ((SystemTypes)(system - 1)).ToString();
        }
    }
}
