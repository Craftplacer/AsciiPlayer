using CommandLine;

namespace AsciiPlayer
{
    public class Options
    {
        [Option(Required = true, HelpText = "Directory of the image files")]
        public string Directory { get; set; }

        [Option('e', "extension", Required = true, HelpText = "File extension of the files")]
        public string Extension { get; set; }

        [Option('f', "framerate", Required = true, HelpText = "The framerate of the video")]
        public int FrameRate { get; set; }

        [Option('x', "scale", Required = false, HelpText = "Scale")]
        public double Scale { get; set; } = -1;

        [Option('k', "skip", Required = false, HelpText = "If frames should be skipped")]
        public bool SkipFrames { get; set; }

        [Option('s', "stream", Required = false, HelpText = "If the video should be displayed while rendering")]
        public bool Streaming { get; set; }

        [Option('w', "workmode", Required = false, HelpText = "How to render efficiently.")]
        public WorkMode WorkMode { get; set; } = WorkMode.Threaded;

        [Option("xcha", Required = false, HelpText = "Doesn't resize the bitmap height to 0.7")]
        public bool NoCharacterHeightAdjustment { get; set; }
    }
    //A:\Desktop\ASCII Player Frames\New folder
    public enum WorkMode
    {
        Single,
        Threaded
    }
}