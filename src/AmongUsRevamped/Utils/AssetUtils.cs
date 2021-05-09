using System;
using System.Linq;
using System.Reflection;
using AmongUsRevamped.Extensions;
using UnityEngine;

namespace AmongUsRevamped.Utils
{
    public static class AssetUtils
    {
        public static byte[] GetBytesFromEmbeddedResource(string embeddedResourcePath)
        {
            return GetBytesFromEmbeddedResource(Assembly.GetCallingAssembly(), embeddedResourcePath);
        }

        public static byte[] GetBytesFromEmbeddedResource(Assembly assembly, string embeddedResourcePath)
        {
            string embeddedResourceFullPath = assembly.GetManifestResourceNames().FirstOrDefault(resourceName => resourceName.EndsWith(embeddedResourcePath, StringComparison.Ordinal));
            if (string.IsNullOrEmpty(embeddedResourceFullPath)) throw new ArgumentNullException(nameof(embeddedResourcePath), $"Embedded resource \"{embeddedResourcePath}\" not found in assembly \"{assembly.GetName().Name}\".");

            return assembly.GetManifestResourceStream(embeddedResourceFullPath).ReadFully();
        }

        #region Sprite

        public static Sprite LoadSprite(string path, bool dontDestroy = false)
        {
            WWW www = new(BepInEx.Utility.ConvertToWWWFormat(path));
            Texture2D tex = GUIExtensions.CreateEmptyTexture();
            www.LoadImageIntoTexture(tex);
            Sprite sprite = tex.CreateSprite();

            if (dontDestroy)
            {
                tex.DontDestroy();
                sprite.DontDestroy();
            }

            return sprite;
        }

        public static Sprite LoadSpriteFromResource(string resourcePath, bool dontDestroy = false)
        {
            byte[] bytes = GetBytesFromEmbeddedResource(resourcePath);
            return LoadSpriteFromBytes(bytes, dontDestroy);
        }

        public static Sprite LoadSpriteFromBytes(byte[] bytes, bool dontDestroy = false)
        {
            Texture2D tex = GUIExtensions.CreateEmptyTexture();
            ImageConversion.LoadImage(tex, bytes, false);
            Sprite sprite = tex.CreateSprite();

            if (dontDestroy)
            {
                tex.DontDestroy();
                sprite.DontDestroy();
            }

            return sprite;
        }

        #endregion Sprite

        #region AudioClip

        public static AudioClip LoadAudioClip(string path, bool dontDestroy = false)
        {
            WWW www = new(BepInEx.Utility.ConvertToWWWFormat(path));
            AudioClip clip = www.GetAudioClip();
            // Wait for the clip to be loaded before returning it
            while (clip.loadState != AudioDataLoadState.Loaded) { }
            if (dontDestroy)
            {
                clip.DontDestroy();
            }
            return clip;
        }

        public static AudioClip LoadAudioClipFromResource(string resourcePath, bool dontDestroy = false)
        {
            byte[] bytes = GetBytesFromEmbeddedResource(resourcePath);
            return LoadAudioClipFromBytes(bytes, resourcePath, dontDestroy);
        }

        public static AudioClip LoadAudioClipFromBytes(byte[] bytes, string name, bool dontDestroy = false)
        {
            int subchunk1 = BitConverter.ToInt32(bytes, 16);
            UInt16 format = BitConverter.ToUInt16(bytes, 20);

            if (format != 1 && format != 65534) throw new Exception("Only PCM and WaveFormatExtensable uncompressed formats are currently supported");

            UInt16 channels = BitConverter.ToUInt16(bytes, 22);
            int sampleRate = BitConverter.ToInt32(bytes, 24);
            UInt16 bitDepth = BitConverter.ToUInt16(bytes, 34);

            int headerOffset = 16 + 4 + subchunk1 + 4;
            int subchunk2 = BitConverter.ToInt32(bytes, headerOffset);
            float[] data = bitDepth switch
            {
                8 => Convert8BitByteArrayToAudioClipData(bytes, headerOffset, subchunk2),
                16 => Convert16BitByteArrayToAudioClipData(bytes, headerOffset, subchunk2),
                24 => Convert24BitByteArrayToAudioClipData(bytes, headerOffset, subchunk2),
                32 => Convert32BitByteArrayToAudioClipData(bytes, headerOffset, subchunk2),
                _ => throw new Exception(bitDepth + " bit depth is not supported."),
            };
            AudioClip clip = AudioClip.Create(name, data.Length, channels, sampleRate, false);
            clip.SetData(data, 0);
            if (dontDestroy)
            {
                clip.DontDestroy();
            }
            return clip;
        }

        private static float[] Convert8BitByteArrayToAudioClipData(byte[] source, int headerOffset, int dataSize)
        {
            int wavSize = BitConverter.ToInt32(source, headerOffset);
            headerOffset += sizeof(int);

            float[] data = new float[wavSize];

            sbyte maxValue = sbyte.MaxValue;

            int i = 0;
            while (i < wavSize)
            {
                data[i] = (float)source[i] / maxValue;
                ++i;
            }

            return data;
        }

        private static float[] Convert16BitByteArrayToAudioClipData(byte[] source, int headerOffset, int dataSize)
        {
            int wavSize = BitConverter.ToInt32(source, headerOffset);
            headerOffset += sizeof(int);

            int x = sizeof(Int16); // block size = 2
            int convertedSize = wavSize / x;

            float[] data = new float[convertedSize];

            Int16 maxValue = Int16.MaxValue;

            int offset = 0;
            int i = 0;
            while (i < convertedSize)
            {
                offset = i * x + headerOffset;
                data[i] = (float)BitConverter.ToInt16(source, offset) / maxValue;
                ++i;
            }

            return data;
        }

        private static float[] Convert24BitByteArrayToAudioClipData(byte[] source, int headerOffset, int dataSize)
        {
            int wavSize = BitConverter.ToInt32(source, headerOffset);
            headerOffset += sizeof(int);

            int x = 3; // block size = 3
            int convertedSize = wavSize / x;

            int maxValue = Int32.MaxValue;

            float[] data = new float[convertedSize];

            byte[] block = new byte[sizeof(int)]; // using a 4 byte block for copying 3 bytes, then copy bytes with 1 offset

            int offset;
            int i = 0;
            while (i < convertedSize)
            {
                offset = i * x + headerOffset;
                Buffer.BlockCopy(source, offset, block, 1, x);
                data[i] = (float)BitConverter.ToInt32(block, 0) / maxValue;
                ++i;
            }

            return data;
        }

        private static float[] Convert32BitByteArrayToAudioClipData(byte[] source, int headerOffset, int dataSize)
        {
            int wavSize = BitConverter.ToInt32(source, headerOffset);
            headerOffset += sizeof(int);

            int x = sizeof(float); //  block size = 4
            int convertedSize = wavSize / x;

            Int32 maxValue = Int32.MaxValue;

            float[] data = new float[convertedSize];

            int offset;
            int i = 0;
            while (i < convertedSize)
            {
                offset = i * x + headerOffset;
                data[i] = (float)BitConverter.ToInt32(source, offset) / maxValue;
                ++i;
            }

            return data;
        }

        #endregion AudioClip
    }
}
