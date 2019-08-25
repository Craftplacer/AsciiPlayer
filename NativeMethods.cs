using Microsoft.Win32.SafeHandles;

using System;
using System.IO;
using System.Runtime.InteropServices;
using System.Text;

namespace AsciiPlayer
{
    internal static class NativeMethods
    {
        [DllImport("Kernel32.dll", SetLastError = true, CharSet = CharSet.Auto)]
        public static extern SafeFileHandle CreateFile(
            string fileName,
            [MarshalAs(UnmanagedType.U4)] uint fileAccess,
            [MarshalAs(UnmanagedType.U4)] uint fileShare,
            IntPtr securityAttributes,
            [MarshalAs(UnmanagedType.U4)] FileMode creationDisposition,
            [MarshalAs(UnmanagedType.U4)] int flags,
            IntPtr template);

        [DllImport("kernel32.dll", SetLastError = true)]
        public static extern bool WriteConsoleOutput(
          SafeFileHandle hConsoleOutput,
          CharInfo[] lpBuffer,
          Coord dwBufferSize,
          Coord dwBufferCoord,
          ref SmallRect lpWriteRegion);

        [StructLayout(LayoutKind.Sequential)]
        public struct Coord
        {
            public short X;
            public short Y;

            public Coord(short X, short Y)
            {
                this.X = X;
                this.Y = Y;
            }
        };

        [StructLayout(LayoutKind.Explicit)]
        public struct CharUnion
        {
            [FieldOffset(0)] public char UnicodeChar;
            [FieldOffset(0)] public byte AsciiChar;

            public CharUnion(char @char) : this() => AsciiChar = Encoding.ASCII.GetBytes(new char[1] { UnicodeChar = @char })[0];
        }

        [StructLayout(LayoutKind.Explicit)]
        public struct CharInfo
        {
            [FieldOffset(0)] public CharUnion Char;
            [FieldOffset(2)] public CharAttr Attributes;

            public CharInfo(char @char, CharAttr attributes) : this()
            {
                Char = new CharUnion(@char);
                Attributes = attributes;
            }
        }

        public enum CharAttr : short
        {
            FG_B = 0x0001,
            FG_G = 0x0002,
            FG_R = 0x0004,
            FG_I = 0x0008,
            BG_B = 0x0010,
            BG_G = 0x0020,
            BG_R = 0x0040,
            BG_I = 0x0080,
        }

        [StructLayout(LayoutKind.Sequential)]
        public struct SmallRect
        {
            public short Left;
            public short Top;
            public short Right;
            public short Bottom;

            public SmallRect(short left, short top, short right, short bottom)
            {
                Left = left;
                Top = top;
                Right = right;
                Bottom = bottom;
            }

            public SmallRect(short width, short height) : this(0, 0, width, height)
            {
            }
        }
    }
}