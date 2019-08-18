using NAudio.Wave;
using System.IO;

namespace MapsetVerifierFramework.objects.resources
{
    public class AudioFile
    {
        private readonly string fileName;
        
        private uint? sampleRate;
        private bool? hasXingHeader;

        private double? averageBitrate;
        private double? lowestBitrate;
        private double? highestBitrate;

        public AudioFile(string aFileName)
        {
            fileName = aFileName;
        }

        /// <summary> Returns the highest bitrate, populates if not already present. </summary>
        public double GetHighestBitrate()
        {
            if (highestBitrate != null)
                return highestBitrate.GetValueOrDefault();

            LoadBitrates();
            return highestBitrate.GetValueOrDefault();
        }

        /// <summary> Returns the lowest bitrate, populates if not already present. </summary>
        public double GetLowestBitrate()
        {
            if (lowestBitrate != null)
                return lowestBitrate.GetValueOrDefault();

            LoadBitrates();
            return lowestBitrate.GetValueOrDefault();
        }

        /// <summary> Returns the average bitrate, populates if not already present. </summary>
        public double GetAverageBitrate()
        {
            if (averageBitrate != null)
                return averageBitrate.GetValueOrDefault();

            LoadBitrates();
            return averageBitrate.GetValueOrDefault();
        }

        /// <summary> Returns whether the audio file contains a Xing header which is skipped over. </summary>
        public bool HasXingHeader()
        {
            if (hasXingHeader != null)
                return hasXingHeader.GetValueOrDefault();

            LoadBitrates();
            return hasXingHeader.GetValueOrDefault();
        }

        /// <summary> Reads through all frames of the mp3 and populates the lowest, highest and average bitrate values. </summary>
        private void LoadBitrates()
        {
            long frameCount = 0;
            long totalBitrate = 0;

            using (FileStream fileStream = File.OpenRead(fileName))
            {
                Mp3Frame frame = Mp3Frame.LoadFromStream(fileStream);

                // Skip over the Xing header if one exists (would otherwise cause 128 kbps first frame).
                XingHeader xingHeader = XingHeader.LoadXingHeader(frame);
                hasXingHeader = xingHeader != null;
                if (xingHeader != null)
                    for (int i = 0; i < xingHeader.Frames; ++i)
                        frame = Mp3Frame.LoadFromStream(fileStream);
                else
                    // Even if there isn't a xing header, there's still going to be a header so we'll skip it.
                    frame = Mp3Frame.LoadFromStream(fileStream);

                while (frame != null)
                {
                    if (frame.SampleCount == 0)
                        continue;

                    sampleRate = (uint)frame.SampleRate;

                    if (lowestBitrate == null || frame.BitRate < lowestBitrate)
                        lowestBitrate = frame.BitRate;

                    if (highestBitrate == null || frame.BitRate > highestBitrate)
                        highestBitrate = frame.BitRate;

                    totalBitrate += frame.BitRate;

                    ++frameCount;
                    try
                    { frame = Mp3Frame.LoadFromStream(fileStream); }
                    catch (EndOfStreamException)
                    { break; }
                }
            }

            averageBitrate = totalBitrate / frameCount;
        }

        /// <summary> Returns the sample rate of the first frame in the mp3. This is usually constant. </summary>
        public uint GetSampleRate()
        {
            if (sampleRate != null)
                return sampleRate.GetValueOrDefault();

            using (FileStream fs = File.OpenRead(fileName))
            {
                Mp3Frame frame = Mp3Frame.LoadFromStream(fs);

                if (frame != null)
                    sampleRate = (uint)frame.SampleRate;
            }

            return sampleRate.GetValueOrDefault();
        }
    }
}
