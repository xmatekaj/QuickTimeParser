using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;

namespace QuickTimeParser
{
    public class Parser
    {
        private const int ATOM_HEADER_SIZE = 8;
        private const int DEEP_SCAN_CHUNK_SIZE = 16;

        private bool DeepScan { get; set; }
        private bool ShowData { get; set; }
        private int ChunkSize { get; set; }

        private List<TrackInfo> tracks = new List<TrackInfo>();
        private List<(long position, ulong size)> mdatAtoms = new List<(long, ulong)>();

        public Parser(bool deepScan, bool showData)
        {
            DeepScan = deepScan;
            ShowData = showData;
            ChunkSize = deepScan ? DEEP_SCAN_CHUNK_SIZE : ATOM_HEADER_SIZE;
        }

        public void ParseFile(string filePath)
        {
            using (FileStream fs = new FileStream(filePath, FileMode.Open, FileAccess.Read))
            using (BinaryReader reader = new BinaryReader(fs))
            {
                long fileSize = fs.Length;
                PrintAdditionalInformation($"File size: {fileSize} bytes");
                ParseAtoms(reader, fileSize);
                ReportTracks();
                ReportMdatAtoms();
            }
        }

        private void PrintAdditionalInformation(string info)
        {
            if (!ShowData)
                return;
            Console.WriteLine(info);
        }

        private void ParseAtoms(BinaryReader reader, long endPosition, string parentAtom = "")
        {
            while (reader.BaseStream.Position < endPosition)
            {
                long startPosition = reader.BaseStream.Position;

                if (endPosition - startPosition < ATOM_HEADER_SIZE)
                    break;

                byte[] sizeBytes = reader.ReadBytes(4);
                Array.Reverse(sizeBytes);
                uint atomSize = BitConverter.ToUInt32(sizeBytes, 0);
                string atomType = new string(reader.ReadChars(4));

                if (QuickTimeAtom.IsValidAtom(atomType))
                {
                    PrintAdditionalInformation($"Atom: {atomType}, Size: {atomSize}, Position: {startPosition:X} ({startPosition})");

                    if (atomSize == 1)
                    {
                        sizeBytes = reader.ReadBytes(8);
                        Array.Reverse(sizeBytes);
                        ulong extendedSize = BitConverter.ToUInt64(sizeBytes, 0);
                        PrintAdditionalInformation($"Extended size: {extendedSize}");
                        atomSize = (uint)Math.Min(extendedSize, uint.MaxValue);
                    }
                    if (atomSize == 0)
                    {
                        atomSize = (uint)(endPosition - startPosition);
                    }

                    long atomDataSize = atomSize - (reader.BaseStream.Position - startPosition);
                    bool isContainer = QuickTimeAtom.IsContainerAtom(atomType);

                    if (atomType == "trak")
                    {
                        tracks.Add(new TrackInfo());
                        long dataPosition = reader.BaseStream.Position;
                        long dataSize = atomSize > 8 ? atomSize - 8 : (endPosition - dataPosition);
                        ParseAtoms(reader, reader.BaseStream.Position + atomDataSize, atomType);
                    }
                    else if (atomType == "mdat")
                    {
                        // Special mdat handling
                        long dataPosition = reader.BaseStream.Position;
                        long dataSize = atomSize > 8 ? atomSize - 8 : (endPosition - dataPosition);
                        mdatAtoms.Add((dataPosition, (ulong)dataSize));
                        PrintAdditionalInformation($"Media data atom (mdat) found at position {dataPosition}, size: {dataSize}");
                        reader.BaseStream.Seek(dataSize, SeekOrigin.Current);
                    }
                    else if (isContainer)
                    {
                        ParseAtoms(reader, reader.BaseStream.Position + atomDataSize, atomType);
                    }
                    else
                    {
                        switch (atomType)
                        {
                            case "hdlr":
                                ParseHandlerAtom(reader);
                                break;
                            case "tkhd":
                                ParseTrackHeaderAtom(reader);
                                break;
                            case "stsd":
                                ParseSampleDescriptionAtom(reader);
                                break;
                            default:
                                reader.BaseStream.Seek(atomDataSize, SeekOrigin.Current);
                                break;
                        }
                    }
                }

                reader.BaseStream.Seek(startPosition + atomSize, SeekOrigin.Begin);
            }
        }

        private void ReportMdatAtoms()
        {
            PrintAdditionalInformation("\nMedia Data (mdat) Atoms:");
            if (mdatAtoms.Count == 0)
            {
                Console.WriteLine("No mdat atoms detected.");
            }
            else
            {
                for (int i = 0; i < mdatAtoms.Count; i++)
                {
                    var (position, size) = mdatAtoms[i];
                    PrintAdditionalInformation($"mdat {i + 1}: Position: {position:X} ({position}), Size: {size} bytes");
                }
            }
        }

        private void ParseHandlerAtom(BinaryReader reader)
        {
            reader.BaseStream.Seek(8, SeekOrigin.Current); // Skip version and flags
            string handlerType = new string(reader.ReadChars(4));
            if (tracks.Count > 0)
            {
                var currentTrack = tracks[tracks.Count - 1];
                if (handlerType == "vide")
                    currentTrack.Type = TrackType.Video;
                else if (handlerType == "soun")
                    currentTrack.Type = TrackType.Audio;
            }
        }

        private void ParseTrackHeaderAtom(BinaryReader reader)
        {
            if (tracks.Count == 0) return;

            var currentTrack = tracks[tracks.Count - 1];

            byte version = reader.ReadByte();
            reader.BaseStream.Seek(3, SeekOrigin.Current); // Skip flags

            if (version == 1)
            {
                reader.BaseStream.Seek(32, SeekOrigin.Current); // Skip creation time, modification time, track ID, reserved, duration for version 1
            }
            else
            {
                reader.BaseStream.Seek(20, SeekOrigin.Current); // Skip creation time, modification time, track ID, reserved, duration for version 0
            }

            reader.BaseStream.Seek(50, SeekOrigin.Current); // Skip other fields we don't need

            // Read track width and height (16.16 fixed point)
            uint width = BitConverter.ToUInt32(reader.ReadBytes(4).Reverse().ToArray(), 0);
            uint height = BitConverter.ToUInt32(reader.ReadBytes(4).Reverse().ToArray(), 0);

            // Convert from fixed point to integer and always create VideoInfo
            currentTrack.VideoInfo = new VideoInfo
            {
                Width = (ushort)width,
                Height = (ushort)height
            }; 

            // If this is the first time we've encountered video info for this track, set its type to Video
            if (currentTrack.Type == TrackType.Unknown)
            {
                currentTrack.Type = TrackType.Video;
            }

            PrintAdditionalInformation($"Track dimensions: {currentTrack.VideoInfo.Width}x{currentTrack.VideoInfo.Height}");
        }

        private void ParseSampleDescriptionAtom(BinaryReader reader)
        {
            long startPosition = reader.BaseStream.Position;
            PrintAdditionalInformation($"Parsing stsd atom at position: {startPosition:X}");

            reader.BaseStream.Seek(4, SeekOrigin.Current); // Skip version
            uint numberOfEntries = BitConverter.ToUInt32(reader.ReadBytes(4).Reverse().ToArray(), 0);
            PrintAdditionalInformation($"Number of entries in stsd: {numberOfEntries}");

            if (numberOfEntries > 0 && tracks.Count > 0)
            {
                var currentTrack = tracks[tracks.Count - 1];
                uint sampleDescriptionSize = BitConverter.ToUInt32(reader.ReadBytes(4).Reverse().ToArray(), 0);
                string dataFormat = new string(reader.ReadChars(4));
                PrintAdditionalInformation($"Data format: {dataFormat}");

                reader.BaseStream.Seek(6, SeekOrigin.Current); // Skip reserved bytes
                ushort dataReferenceIndex = BitConverter.ToUInt16(reader.ReadBytes(2).Reverse().ToArray(), 0);

                if (currentTrack.Type == TrackType.Audio)
                {
                    reader.BaseStream.Seek(8, SeekOrigin.Current); // Skip version and revision level, vendor
                    ushort channelCount = BitConverter.ToUInt16(reader.ReadBytes(2).Reverse().ToArray(), 0);
                    ushort sampleSize = BitConverter.ToUInt16(reader.ReadBytes(2).Reverse().ToArray(), 0);
                    reader.BaseStream.Seek(4, SeekOrigin.Current); // Skip compression ID and packet size
                    uint sampleRate = BitConverter.ToUInt32(reader.ReadBytes(4).Reverse().ToArray(), 0) >> 16; // Sample rate is fixed-point 16.16

                    currentTrack.AudioInfo = new AudioInfo
                    {
                        SampleRate = sampleRate,
                        ChannelCount = channelCount,
                        SampleSize = sampleSize
                    };

                    PrintAdditionalInformation($"Audio info - Sample Rate: {sampleRate}, Channels: {channelCount}, Sample Size: {sampleSize}");
                }
            }
        }

        private void ReportTracks()
        {
            Console.WriteLine("\nTrack Summary:");

            if (tracks.Count == 0)
            {
                Console.WriteLine("No tracks detected.");
            }
            else
            {
                for (int i = 0; i < tracks.Count; i++)
                {
                    var track = tracks[i];
                    Console.WriteLine($"Track {i + 1}:");
                    Console.WriteLine($"Type: {track.Type}");
                    if (track.Type == TrackType.Audio && track.AudioInfo != null)
                    {
                        Console.WriteLine($"Sample Rate: {track.AudioInfo.SampleRate} Hz");
                    }
                    else if (track.Type == TrackType.Video && track.VideoInfo != null)
                    {
                        Console.WriteLine($"Width: {track.VideoInfo.Width} pixels");
                        Console.WriteLine($"Height: {track.VideoInfo.Height} pixels");
                    }
                    Console.WriteLine();
                }
            }
        }
    }
}