using CommandLine;

using HazdryxEngine.Drawing;

using Microsoft.Win32.SafeHandles;

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.Drawing.Text;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

using static AsciiPlayer.NativeMethods;

namespace AsciiPlayer
{
    internal static class Program
    {
        public static Dictionary<Color, CharInfo> chars = new Dictionary<Color, CharInfo>()
        {
        };

        public static int Delay;
        public static int framePosition = 0;
        public static Dictionary<int, (CharInfo[], Coord)> frames = new Dictionary<int, (CharInfo[], Coord)>();
        public static int framesTotal = 0;
        public static Options options;
        public static DateTime start;
        public static SafeFileHandle Console;

        public static int fps = 0;

        public static Color ConsoleBlack = Color.Black;
        public static Color ConsoleDarkGray = Color.FromArgb(118, 118, 118);
        public static Color ConsoleGray = Color.FromArgb(204, 204, 204);
        public static Color ConsoleWhite = Color.White;
        public static Color ConsoleRed = Color.Red;
        public static Color ConsoleGreen = Color.Lime;
        public static Color ConsoleBlue = Color.Blue;
        public static Color ConsoleDarkRed = Color.FromArgb(128, 0, 0);
        public static Color ConsoleDarkGreen = Color.FromArgb(0, 128, 0);
        public static Color ConsoleDarkBlue = Color.FromArgb(0, 0, 128);
        public static Color ConsoleYellow = Color.FromArgb(255, 255, 0);

        public static int RealTimePosition
        {
            get
            {
                if (options.SkipFrames)
                {
                    if (start == null || start == DateTime.MaxValue)
                        return 0;

                    return (int)((DateTime.Now - start).TotalMilliseconds / Delay);
                }

                return framePosition;
            }
            set
            {
                if (value == -1)
                {
                    start = DateTime.Now;
                }
                else if (!options.SkipFrames)
                {
                    framePosition = value;
                }
            }
        }

        public static void AnalyzeCharacters()
        {
            using (var font = new Font("Small Fonts", 4, GraphicsUnit.Pixel))
            using (var format = new StringFormat()
            {
                LineAlignment = StringAlignment.Center,
                Alignment = StringAlignment.Center
            })
            {
                /// <summary>
                /// Takes a look at the provided char, and figures out the average color of it.ssssssss
                /// </summary>
                Color AnalyzeCharacter(char @char, Color background, Color foreground)
                {
                    using (var bitmap = new Bitmap(4, 6))
                    using (var graphics = Graphics.FromImage(bitmap))
                    using (var brush = new SolidBrush(foreground))
                    {
                        graphics.TextRenderingHint = TextRenderingHint.SingleBitPerPixel;
                        graphics.Clear(background);

                        var rect = new RectangleF(0, 0, bitmap.Width, bitmap.Height);
                        graphics.DrawString(@char.ToString(), font, brush, rect, format);

                        using (var pixelBitmap = new FastBitmap(ResizeImage(bitmap, 1, 1)))
                            return pixelBitmap[0, 0];

                        //bitmap.Save($"{(int)c}.bmp");
                    }
                }

                for (var i = 32; i < 126; i++)
                {
                    char @char = (char)i;
                    System.Console.WriteLine($"Analyzing {@char}...");
                    System.Console.CursorTop--;

                    var bg = new (Color, CharAttr)[]
                    {
                        (ConsoleBlack, 0),
                        //(ConsoleDarkGray, CharAttr.BG_R | CharAttr.BG_G | CharAttr.BG_B),
                        //(ConsoleGray, CharAttr.BG_I),
                        //(ConsoleWhite,  CharAttr.BG_R | CharAttr.BG_G | CharAttr.BG_B| CharAttr.BG_I),
                        //(ConsoleRed,  CharAttr.BG_R | CharAttr.BG_I),
                        //(ConsoleGreen,  CharAttr.BG_G | CharAttr.BG_I),
                        //(ConsoleBlue,  CharAttr.BG_B | CharAttr.BG_I),
                        //(ConsoleYellow,  CharAttr.BG_R | CharAttr.BG_G | CharAttr.BG_I),
                        //(ConsoleDarkRed,  CharAttr.BG_R),
                        //(ConsoleDarkGreen,  CharAttr.BG_G),
                        //(ConsoleDarkBlue,  CharAttr.BG_B),
                    };

                    var fg = new (Color, CharAttr)[]
                    {
                        //(ConsoleBlack, 0),
                        //(ConsoleDarkGray, CharAttr.FG_R | CharAttr.FG_G | CharAttr.FG_B),
                        //(ConsoleGray, CharAttr.FG_I),
                        (ConsoleWhite,  CharAttr.FG_R | CharAttr.FG_G | CharAttr.FG_B| CharAttr.FG_I),
                        //(ConsoleRed,  CharAttr.FG_R | CharAttr.FG_I),
                        //(ConsoleGreen,  CharAttr.FG_G | CharAttr.FG_I),
                        //(ConsoleBlue,  CharAttr.FG_B | CharAttr.FG_I),
                        //(ConsoleYellow,  CharAttr.FG_R | CharAttr.FG_G | CharAttr.FG_I),
                        //(ConsoleDarkRed,  CharAttr.FG_R),
                        //(ConsoleDarkGreen,  CharAttr.FG_G),
                        //(ConsoleDarkBlue,  CharAttr.FG_B),
                    };

                    for (int bgi = 0; bgi < bg.Length; bgi++)
                    {
                        for (int fgi = 0; fgi < fg.Length; fgi++)
                        {
                            var color = AnalyzeCharacter(@char, bg[bgi].Item1, fg[fgi].Item1);
                            chars[color] = new CharInfo(@char, bg[bgi].Item2 | fg[fgi].Item2);
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Returns the fitting character to display of a color
        /// </summary>
        public static CharInfo GetChar(Color c)
        {
            return chars[GetClosestColor(chars.Keys, c)];
        }

        /// <summary>
        /// Figures out what character to use for a specific grayscale float.
        /// </summary>
        ///public static CharInfo GetChar(float f)
        ///{
        ///    float lastFloat = float.MaxValue;
        ///    float lastDistance = float.MaxValue;
        ///    foreach (Color color in chars.Keys)
        ///    {
        ///        float distance = GetDistance(f, @float);
        ///        if (lastDistance > distance)
        ///        {
        ///            lastDistance = distance;
        ///            lastFloat = @float;
        ///        }
        ///    }
        ///
        ///    return chars[lastFloat];
        ///}

        /// <summary>
        /// Returns how far <paramref name="b"/> is away from <paramref name="a"/>. Used for sorting, seeing which item fits best the target.
        /// </summary>
        public static float GetDistance(float a, float b)
        {
            if (a == b)
            {
                return 0;
            }
            else if (a < b)
            {
                return b - a;
            }
            else
            {
                return a - b;
            }
        }

        public static int GetDistance(int a, int b)
        {
            if (a == b)
            {
                return 0;
            }
            else if (a < b)
            {
                return b - a;
            }
            else
            {
                return a - b;
            }
        }

        /// <summary>
        /// Turns a color into grayscale
        /// </summary>
        /// <param name="c">The color</param>
        /// <returns>A <see cref="float"/> between <see cref="0f"/> and <see cref="1f"/></returns>
        public static float GetFloat(System.Drawing.Color c) => ((c.R * .3f) + (c.G * .59f) + (c.B * .11f));

        /// <summary>
        /// Returns the console title representing the current status of the application
        /// </summary>
        public static string GetTitle()
        {
            var builder = new StringBuilder();

            builder.Append($"Rendering: {frames.Count}/{framesTotal} ");
            builder.Append($"| Playing: {RealTimePosition} ({fps} FPS)");
            builder.Append($" ({frames.Count - RealTimePosition} frames overhead)");

            fps = 0;

            return builder.ToString();
        }

        public static void PrintFrame((CharInfo[], Coord) frame)
        {
            var rect = new SmallRect()
            {
                Top = 0,
                Left = 0,
                Right = (short)System.Console.WindowWidth,
                Bottom = (short)System.Console.WindowHeight
            };

            WriteConsoleOutput(Console, frame.Item1, frame.Item2, new Coord(0, 0), ref rect);
        }

        public static void PrintFrames(Dictionary<int, (CharInfo[], Coord)> frames)
        {
            void frame(int position)
            {
                if (options.SkipFrames && !frames.ContainsKey(position))
                    return;
                else
                    while (!frames.ContainsKey(position))
                        Thread.Sleep(10);

                PrintFrame(frames[position]);
                fps++;
            }

            for (RealTimePosition = -1; RealTimePosition < framesTotal; RealTimePosition++)
            {
                DateTime start = DateTime.Now;
                frame(RealTimePosition);

                var span = DateTime.Now - start;

                var delay = Delay - ((int)span.TotalMilliseconds);

                if (delay < 0)
                    delay = 0;

                if (!options.SkipFrames)
                    Thread.Sleep(delay);
            }
        }

        /// <summary>
        /// Resize the image to the specified width and height.
        /// </summary>
        /// <param name="bitmap">The image to resize.</param>
        /// <param name="width">The width to resize to.</param>
        /// <param name="height">The height to resize to.</param>
        /// <returns>The resized image.</returns>
        public static Bitmap ResizeImage(this Bitmap bitmap, int width, int height)
        {
            if (bitmap == null)
                throw new ArgumentNullException(nameof(bitmap));

            //Check if a resize is needed.
            if (bitmap.Size.Equals(new Size(width, height)))
                return (Bitmap)bitmap;

            var destRect = new Rectangle(0, 0, width, height);
            var destImage = new Bitmap(width, height, PixelFormat.Format32bppArgb);
            destImage.SetResolution(bitmap.HorizontalResolution, bitmap.VerticalResolution);
            using (var graphics = Graphics.FromImage(destImage))
            {
                graphics.SmoothingMode = SmoothingMode.HighQuality;
                graphics.CompositingQuality = CompositingQuality.HighQuality;
                graphics.InterpolationMode = InterpolationMode.High;
                graphics.PixelOffsetMode = PixelOffsetMode.HighQuality;
                using (var wrapMode = new ImageAttributes())
                {
                    wrapMode.SetWrapMode(WrapMode.TileFlipXY);
                    graphics.DrawImage(bitmap, destRect, 0, 0, bitmap.Width, bitmap.Height, GraphicsUnit.Pixel, wrapMode);
                }
            }
            return destImage;
        }

        private static void Main(string[] args)
        {
            Parser.Default.ParseArguments<Options>(args).WithParsed(o =>
            {
                options = o;

                Console = CreateFile("CONOUT$", 0x40000000, 2, IntPtr.Zero, FileMode.Open, 0, IntPtr.Zero);

                if (Console.IsInvalid)
                {
                    throw new Exception("handle is invalid");
                }

                framesTotal = Directory.GetFiles(options.Directory, $"*.{options.Extension}").Length;
                Delay = 1000 / options.FrameRate;

                AnalyzeCharacters();

                var drawThread = new Thread(() =>
                {
                    System.Console.CursorVisible = false;

                    PrintFrames(frames);

                    System.Console.CursorVisible = true;
                })
                {
                    Priority = ThreadPriority.Highest,
                    Name = "Draw Thread"
                };

                Process.GetCurrentProcess().PriorityBoostEnabled = true;

                if (options.Streaming)
                    drawThread.Start();

                var renderingThread = new Thread(RenderFrames)
                {
                    Name = "Renderer Thread",
                    IsBackground = true,
                    Priority = ThreadPriority.AboveNormal
                };

                renderingThread.Start();

                while (true)
                {
                    System.Console.Title = GetTitle();
                    Thread.Sleep(1000);

                    if (!options.Streaming && !renderingThread.IsAlive && !drawThread.IsAlive)
                    {
                        System.Console.WriteLine("Finished rendering, press enter to start playing.");
                        System.Console.ReadLine();
                        System.Console.Clear();

                        drawThread.Start();
                    }
                }
            });
        }

        private static void RenderFrames()
        {
            void Step(int i)
            {
                if (options.Streaming && options.SkipFrames && i < (RealTimePosition + options.FrameRate))
                    return; //skip frame

                string path = Path.Combine(options.Directory, $"{i + 1}.{options.Extension}");

                while (!File.Exists(path))
                    Thread.Sleep(250);

                using (var fileBitmap = (Bitmap)Image.FromFile(path))
                    frames[i] = RenderFrame(fileBitmap);
            }

            if (!options.Streaming)
                Process.GetCurrentProcess().PriorityClass = ProcessPriorityClass.RealTime;

            if (options.WorkMode == WorkMode.Single)
            {
                for (int i = 0; i < framesTotal; i++)
                    Step(i);
            }
            else if (options.WorkMode == WorkMode.Threaded)
            {
                if (options.Streaming)
                {
                    int chunkSize = options.FrameRate / 2;
                    int chunks = framesTotal / chunkSize;
                    for (int i = 0; i < chunks; i++)
                    {
                        int start = i * chunkSize;
                        Parallel.For(start, start + chunkSize, (i2) => Step(i2));
                    }
                }
                else
                {
                    Parallel.For(0, framesTotal, new ParallelOptions() { MaxDegreeOfParallelism = Environment.ProcessorCount },(i) => Step(i));
                }
            }

            Process.GetCurrentProcess().PriorityClass = ProcessPriorityClass.AboveNormal;
        }

        private static Color GetClosestColor(IEnumerable<Color> colorArray, Color baseColor)
        {
            var lastColor = Color.Transparent;
            var lastDistance = float.MaxValue;
            foreach (Color color in chars.Keys)
            {
                float distance = GetDiff(baseColor, color);
                if (lastDistance > distance)
                {
                    lastDistance = distance;
                    lastColor = color;
                }
            }

            return lastColor;
        }

        private static int GetDiff(Color color, Color baseColor)
        {
            return GetDistance(baseColor.R, color.R) + GetDistance(baseColor.G, color.G) + GetDistance(baseColor.B, color.B);
        }

        private static (CharInfo[], Coord) RenderFrame(Bitmap originalBitmap)
        {
            double width = originalBitmap.Width;
            double height = originalBitmap.Height;

            if (!options.NoCharacterHeightAdjustment)
            {
                /// The reason why height is being mutiplied by 0.7 is because a
                /// square only takes up two thirds of the console character.
                ///
                ///
                /// (4x6)
                /// +--+
                /// |  |  <- Pixel
                /// |  |
                /// |__|
                /// |  |  <- Console character
                /// +--+
                
                height *= 0.7;
            }
           

            double scale = (options.Scale < 0) ? ((System.Console.WindowHeight - 1) / height) : options.Scale;

            width *= scale;
            height *= scale;

            using (var resizedBitmap = ResizeImage(originalBitmap, (int)width, (int)height))
            using (var bitmap = new FastBitmap(resizedBitmap))
            {
                var pixels = new CharInfo[bitmap.Width * bitmap.Height];

                for (int y = 0; y < bitmap.Height; y++)
                {
                    for (int x = 0; x < bitmap.Width; x++)
                    {
                        pixels[(y * bitmap.Width) + x] = GetChar(bitmap[x, y]);
                    }
                }

                return (pixels, new Coord((short)bitmap.Width, (short)bitmap.Height));
            }
        }
    }
}