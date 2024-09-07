using QuickTimeParser;

namespace QuickTimeParserTests
{
    public class ExpectedResult
    {
        public string FileName { get; set; }
        public bool ExpectVideo { get; set; }
        public int? VideoWidth { get; set; }
        public int? VideoHeight { get; set; }
        public bool ExpectAudio { get; set; }
        public int? AudioSampleRate { get; set; }
    }

    [TestClass]
    public class QuickTimeParserTests
    {
        private static readonly List<ExpectedResult> TestCases = new List<ExpectedResult>();
        

        [TestInitialize]
        public void Initialize()
        {
            TestCases.AddRange(new[] {
                new ExpectedResult { FileName = "AUDIO_chalkie.mov", ExpectAudio = true, AudioSampleRate = 11127, ExpectVideo = false },
                new ExpectedResult { FileName = "AUDIO_mr_elect_stairs_dn.mov", ExpectAudio = true, AudioSampleRate = 11127, ExpectVideo = false },
                new ExpectedResult { FileName = "AUDIO_protect.mov", ExpectAudio = true, AudioSampleRate = 22050, ExpectVideo = false },
                new ExpectedResult { FileName = "AUDIO_tomyhead.mov", ExpectAudio = true, AudioSampleRate = 22254, ExpectVideo = false },
                new ExpectedResult { FileName = "AUDIO_whatsay.mov", ExpectAudio = true, AudioSampleRate = 22254, ExpectVideo = false },
                new ExpectedResult { FileName = "MOVIE_apple-intermediate-codec.mov", ExpectVideo = true, VideoWidth = 320, VideoHeight = 240, ExpectAudio = false },
                new ExpectedResult { FileName = "MOVIE_example.mov", ExpectVideo = true, VideoWidth = 1920, VideoHeight = 1080, ExpectAudio = true, AudioSampleRate = 48000 },
                new ExpectedResult { FileName = "MOVIE_example_hevc.mov", ExpectVideo = true, VideoWidth = 1920, VideoHeight = 1080, ExpectAudio = true, AudioSampleRate = 44100 },
                new ExpectedResult { FileName = "MOVIE_example_small.mov", ExpectVideo = true, VideoWidth = 500, VideoHeight = 281, ExpectAudio = true, AudioSampleRate = 44100 },
                new ExpectedResult { FileName = "MOVIE_NNM3022_LiquidArmor_ISDN.mov", ExpectVideo = true, VideoWidth = 240, VideoHeight = 180, ExpectAudio = true, AudioSampleRate = 22050 },
                new ExpectedResult { FileName = "MOVIE_sample_1920x1080.mov", ExpectVideo = true, VideoWidth = 1920, VideoHeight = 1080, ExpectAudio = false },
                new ExpectedResult { FileName = "MOVIE_WISHBONE.mov", ExpectVideo = true, VideoWidth = 160, VideoHeight = 120, ExpectAudio = true, AudioSampleRate = 11127 }
            });
        }

        [TestMethod]
        public void TestQuickTimeParser()
        {
            foreach (var testCase in TestCases)
            {
                bool deepScan = false;
                bool showAdditionalInfo = false;

                var parser = new Parser(deepScan, showAdditionalInfo);
                string testFilePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "TestFiles", testCase.FileName);

                parser.ParseFile(testFilePath);

                if (testCase.ExpectVideo)
                {
                    var videoTrack = parser.Tracks.FirstOrDefault(t => t.Type == TrackType.Video);
                    Assert.IsNotNull(videoTrack, $"Expected video track not found in {testCase.FileName}");
                    Assert.AreEqual(testCase.VideoWidth, videoTrack.VideoInfo.Width, $"Video width mismatch in {testCase.FileName}");
                    Assert.AreEqual(testCase.VideoHeight, videoTrack.VideoInfo.Height, $"Video height mismatch in {testCase.FileName}");
                }
                else
                {
                    Assert.IsFalse(parser.Tracks.Any(t => t.Type == TrackType.Video), $"Unexpected video track found in {testCase.FileName}");
                }

                if (testCase.ExpectAudio)
                {
                    var audioTrack = parser.Tracks.FirstOrDefault(t => t.Type == TrackType.Audio);
                    Assert.IsNotNull(audioTrack, $"Expected audio track not found in {testCase.FileName}");
                    Assert.AreEqual((int)testCase.AudioSampleRate, (int)audioTrack.AudioInfo.SampleRate, 0, $"Audio sample rate mismatch in {testCase.FileName}");
                }
                else
                {
                    Assert.IsFalse(parser.Tracks.Any(t => t.Type == TrackType.Audio), $"Unexpected audio track found in {testCase.FileName}");
                }
            }
        }
        
    }
}