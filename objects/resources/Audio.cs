using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using ManagedBass;

namespace MapsetVerifierFramework.objects.resources
{
    public static class Audio
    {
        private static readonly ConcurrentDictionary<string, object> locks = new ConcurrentDictionary<string, object>();
        private static bool isInitialized = false;
        
        private static void Initialize()
        {
            if (!isInitialized)
            {
                // 0 = No Output Device
                if (!Bass.Init(0) && Bass.LastError != Errors.Already)
                    throw new BadImageFormatException(
                        $"Could not initialize ManagedBass, error \"{Bass.LastError}\".");

                isInitialized = true;
            }
        }

        private static int CreateStream(string aFileName)
        {
            Initialize();

            int stream = Bass.CreateStream(aFileName, 0, 0, BassFlags.Decode);
            if (stream == 0)
                throw new BadImageFormatException(
                    $"Could not create stream of \"{aFileName}\", error \"{Bass.LastError}\".");

            return stream;
        }

        private static void FreeStream(int aStream)
        {
            Bass.StreamFree(aStream);
        }

        /// <summary> Returns the format of the audio file (e.g. mp3, wav, etc). </summary>
        public static ChannelType GetFormat(string aFileName)
        {
            // Implements a queue to prevent race conditions since Bass is a static library.
            // Also prevents deadlocks through using new object() rather than the file name itself.
            lock (locks.GetOrAdd(aFileName, new object()))
            {
                int stream = CreateStream(aFileName);
                Bass.ChannelGetInfo(stream, out ChannelInfo channelInfo);

                FreeStream(stream);
                return channelInfo.ChannelType;
            }
        }

        /// <summary> Returns the channel amount (1 = mono, 2 = stereo, etc). </summary>
        public static int GetChannels(string aFileName)
        {
            lock (locks.GetOrAdd(aFileName, new object()))
            {
                int stream = CreateStream(aFileName);
                Bass.ChannelGetInfo(stream, out ChannelInfo channelInfo);

                FreeStream(stream);
                return channelInfo.Channels;
            }
        }

        /// <summary> Returns the audio duration in ms. </summary>
        public static double GetDuration(string aFileName)
        {
            lock (locks.GetOrAdd(aFileName, new object()))
            {
                int    stream  = CreateStream(aFileName);
                long   length  = Bass.ChannelGetLength(stream);
                double seconds = Bass.ChannelBytes2Seconds(stream, length);

                FreeStream(stream);
                return seconds * 1000;
            }
        }

        /// <summary> Returns the normalized audio peaks (split by channel) for each ms (List = time, array = channel). </summary>
        public static List<float[]> GetPeaks(string aFileName)
        {
            lock (locks.GetOrAdd(aFileName, new object()))
            {
                int    stream  = CreateStream(aFileName);
                long   length  = Bass.ChannelGetLength(stream);
                double seconds = Bass.ChannelBytes2Seconds(stream, length);

                Bass.ChannelGetInfo(stream, out ChannelInfo channelInfo);

                List<float[]> peaks = new List<float[]>();
                for (int i = 0; i < (int)(seconds * 1000); ++i)
                {
                    float[] levels = new float[channelInfo.Channels];

                    bool success = Bass.ChannelGetLevel(stream, levels, 0.001f, 0);
                    if (!success)
                        throw new BadImageFormatException(
                            "Could not parse audio peak of \"" + aFileName + "\" at " + i * 1000 + " ms.");

                    peaks.Add(levels);
                }

                FreeStream(stream);
                return peaks;
            }
        }

        // These two methods are mostly for converting GetFormat into a readable format.
        public static IEnumerable<Enum> GetFlags(Enum anInput)
        {
            foreach (Enum value in Enum.GetValues(anInput.GetType()))
                if (anInput.HasFlag(value))
                    yield return value;
        }

        public static string EnumToString(Enum anInput)
        {
            bool formatsCorrectly = false;
            try
            { long.Parse(anInput.ToString()); }
            catch
            { formatsCorrectly = true; }

            if (formatsCorrectly)
                return anInput.ToString();
            else
                return String.Join("|", GetFlags(anInput));
        }
    }
}
