using System;
using System.IO;
using UnityEngine;

namespace GraphProcessor
{
    public static class GraphExportUtils
    {
        /// <summary>
        /// Export AudioClip to wav file
        /// </summary>
        /// <ref>https://gist.github.com/mokhabadi/b525f5c2c48970e2f790a16721c78a97</ref>
        /// <returns></returns>
        public static bool ExportAudioClip(AudioClip clip, string outPath)
        {
            if (clip == null)
                return false;
            float[] floats = new float[clip.samples * clip.channels];
            clip.GetData(floats, 0);

            byte[] bytes = new byte[floats.Length * 2];

            for (int ii = 0; ii < floats.Length; ii++)
            {
                short uint16 = (short)(floats[ii] * short.MaxValue);
                byte[] vs = BitConverter.GetBytes(uint16);
                bytes[ii * 2] = vs[0];
                bytes[ii * 2 + 1] = vs[1];
            }

            byte[] wav = new byte[44 + bytes.Length];

            byte[] header = {0x52, 0x49, 0x46, 0x46, 0x00, 0x00, 0x00, 0x00,
                         0x57, 0x41, 0x56, 0x45, 0x66, 0x6D, 0x74, 0x20,
                         0x10, 0x00, 0x00, 0x00, 0x01, 0x00, 0x00, 0x00,
                         0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00,
                         0x04, 0x00, 0x10, 0x00, 0x64, 0x61, 0x74, 0x61 };

            Buffer.BlockCopy(header, 0, wav, 0, header.Length);
            Buffer.BlockCopy(BitConverter.GetBytes(36 + bytes.Length), 0, wav, 4, 4);
            Buffer.BlockCopy(BitConverter.GetBytes(clip.channels), 0, wav, 22, 2);
            Buffer.BlockCopy(BitConverter.GetBytes(clip.frequency), 0, wav, 24, 4);
            Buffer.BlockCopy(BitConverter.GetBytes(clip.frequency * clip.channels * 2), 0, wav, 28, 4);
            Buffer.BlockCopy(BitConverter.GetBytes(clip.channels * 2), 0, wav, 32, 2);
            Buffer.BlockCopy(BitConverter.GetBytes(bytes.Length), 0, wav, 40, 4);
            Buffer.BlockCopy(bytes, 0, wav, 44, bytes.Length);

            File.WriteAllBytes(outPath, wav);
            return true;
        }
    }
}