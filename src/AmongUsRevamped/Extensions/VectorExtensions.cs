using UnityEngine;

namespace AmongUsRevamped.Extensions
{
    public static class VectorExtensions
    {
        /// <summary>
        /// Converts X, Y of <see cref="Vector3"/> to <see cref="Vector2"/>.
        /// </summary>
        public static Vector2 ToVector2(this Vector3 vector)
        {
            return vector;
        }

        /// <summary>
        /// Converts X, Y of <see cref="Vector2"/> to <see cref="Vector3"/> with specified Z.
        /// </summary>
        public static Vector3 ToVector3(this Vector2 vector, float z)
        {
            return new Vector3(vector.x, vector.y, z);
        }

        /// <summary>
        /// Converts X, Y of <see cref="Vector2"/> to <see cref="Vector3"/> (Z = 0).
        /// </summary>
        public static Vector3 ToVector3(this Vector2 vector)
        {
            return vector.ToVector3(0);
        }
    }
}
