namespace QuickTimeParser
{
    public enum TrackType { Unknown, Audio, Video }
    public class TrackInfo
    {
        public TrackType Type { get; set; } = TrackType.Unknown;
        public AudioInfo AudioInfo { get; set; }
        public VideoInfo VideoInfo { get; set; }
    }
    public class AudioInfo
    {
        public uint SampleRate { get; set; }
        public ushort ChannelCount { get; set; }
        public ushort SampleSize { get; set; }
    }
    public class VideoInfo
    {
        public ushort Width { get; set; }
        public ushort Height { get; set; }
    }
}
