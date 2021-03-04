/* 
 * To Do List:
 * - Add all found texts
 * - Add basic MLA query interface
 * - Add admin account certification program
 * - Add device manager utility
 * - Add /banish command
 * - Remove all "goto" statements
 * - Add tags to make coloring and making delays easier
*/

using System;
using System.Threading;
using System.Drawing;
using Console = Colorful.Console;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.ComponentModel;
using System.Reflection;
using System.Linq;
using System.Collections;
//using Terminal.Gui;

namespace mla_terminal
{
    class Program
    {
        // Path variables
        readonly static string workingDir = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
        readonly static string projectDir = Directory.GetParent(workingDir).Parent.Parent.FullName;
        readonly static string globalResourceDir = Path.Join(projectDir, "resources");
        readonly static string programResourceDir = Path.Join(projectDir, "resources", "program");
        readonly static string userResourceDir = Path.Join(projectDir, "resources", "user", "found_texts");
        readonly static string resourceZipPath = Path.Join(projectDir, "resources.zip");
        readonly static string resourceGroPath = Path.Join(projectDir, "resources.gro");
        static string input;

        /* 
         * String array containing all of the user prefixes
         * [0] = '> '
         * [1] = '[guest@local]# '
         * [2] = '[guest@unknown]# '
         * [3] = '[admin@local]# '
        */
        static string[] userPrefixes;
        static string userPrefix;

        // Flag to exit the program
        static bool exitProgram = false;

        // Plays screensaver when 'true'
        static bool playScreensaver = false;

        // Turns debug mode on or off
        static bool debugMode = false;

        // Enable or disable packing resources into a .gro file
        static bool enableResourcePacking = false;

        // Color variables
        readonly static Color customGray = Color.FromArgb(0xE0D8C4);
        readonly static Color customBlue = Color.FromArgb(0x8CCAE6);
        readonly static Color customYellow = Color.FromArgb(0xCDD08D);

        static void Main(string[] args)
        {
            Boot();

            while (true)
            {
                if (exitProgram == true)
                {
                    return;
                }

                GetInput(userPrefix, customGray, customBlue);
            }
        }

        public static void Initialize()
        {
            // Unpack resource files from the .gro file
            if (File.Exists(resourceGroPath))
            {
                if (File.Exists(resourceZipPath))
                    File.Delete(resourceZipPath);

                File.Move(resourceGroPath, resourceZipPath);

                if (Directory.Exists(globalResourceDir))
                    Directory.Delete(globalResourceDir, true);

                ZipFile.ExtractToDirectory(Path.Join(projectDir, "resources.zip"), globalResourceDir);
                File.Delete(resourceZipPath);
            }

            userPrefixes = File.ReadAllLines(Path.Join(programResourceDir, "text", "user_prefix", "user_prefix.txt"));
            ConsoleSettings.SetCurrentFont("DejaVu Sans Mono", 24);
            Console.Title = "not_a_virus.exe";
            Console.WindowWidth = 90;
            Console.WindowHeight = 35;
            Console.BufferWidth = 90;
            Console.BufferHeight = 35;
            Console.CursorSize = 100;
            Console.BackgroundColor = Color.FromArgb(0x000000);
            userPrefix = userPrefixes[1];
            Console.Clear();
        }

        static void Boot()
        {
            Initialize();
            Type(GetResource("start_text.txt"), customGray, 0);
        }
        
        static void Close()
        {
            if (enableResourcePacking == true)
            {
                // Pack resource files into a .gro file
                if (Directory.Exists(globalResourceDir))
                {
                    if (File.Exists(resourceZipPath))
                        File.Delete(resourceZipPath);

                    ZipFile.CreateFromDirectory(globalResourceDir, resourceZipPath);

                    if (File.Exists(resourceGroPath))
                        File.Delete(resourceGroPath);

                    File.Move(resourceZipPath, resourceGroPath);
                    Directory.Delete(globalResourceDir, true);
                    exitProgram = true;
                }
            }
        }

        static void GetInput(string prefix, Color prefixColor, Color inputColor)
        {
            Type(prefix, prefixColor, 0, 0);
            Console.ForegroundColor = inputColor;

            // Discard keypresses when the program is typing
            while (Console.KeyAvailable)
            {
                Console.ReadKey(true);
            }

            input = Console.ReadLine();

            // Split the input into key words
            var words = input.Split(' ', StringSplitOptions.RemoveEmptyEntries);

            if (words.Length == 0)
            {
                return;
            }

            try
            {
                ProcessCommand(words);
            }
            catch (Exception exception)
            {
                if (exception.Message == "operation_cancelled")
                {
                    Type("Operation cancelled.", customGray, 1, 2);
                    return;
                }

                Type("Exception thrown.", customGray, 0);

                if (debugMode == true)
                {
                    Type(exception.Message, Color.Red);
                }
            }
        }

        static void Type(string str, Color textcolor, int emptyLinesStart = 1, int emptyLinesEnd = 1, int delay = 1)
        {
            Console.CursorVisible = false;

            for (int i = 0; i < emptyLinesStart; i++)
            {
                Console.WriteLine();
            }

            Console.ForegroundColor = textcolor;

            for (int i = 0; i < str.Length; i++)
            {
                if (Console.KeyAvailable)
                {
                    delay = 0;
                }

                var c = str[i];

                Console.Write(c);
                Thread.Sleep(delay);
            }

            for (int i = 0; i < emptyLinesEnd; i++)
            {
                Console.WriteLine();
            }

            Console.CursorVisible = true;
        }

        // Process the command entered by the user
        static void ProcessCommand(string[] words)
        {
            switch (words[0].ToLower())
            {
                case "help":
                    Type(GetResource("help_text.txt"), customGray);
                    break;

                case "list":
                    if (words.Length > 1)
                        ListFiles(words[1]);
                    else
                        ListFiles("\\");
                    break;

                case "open":
                    if (words.Length > 1)
                        OpenFile(words[1]);
                    else
                        Type("Please enter a file to open.", customGray, 0);
                    break;

                case "run":
                    if (words.Length > 1)
                        ProcessCommandRun(words[1]);
                    else
                        Type("Please enter a program to run.", customGray, 0);
                    break;

                case "exit":
                    if (words.Length > 1)
                    {
                        if (words[1] == "force")
                        {
                            Close();
                        }
                        else
                        {
                            Type("exit - close the terminal session", customGray, 0, 1);
                            Type("exit force - force the terminal session to close", customGray, 0, 1);
                        }
                    }
                    else
                    {
                        Type(GetResource("exit_text.txt"), customGray, 1, 0);
                        Console.CursorVisible = false;
                        Thread.Sleep(1000);
                        Close();
                    }

                    break;
                    
                case "admin":
                    Type("Administrator authentication program currently unavailable. Please try again later.", customGray, 0);
                    break;

                case "device_manager":
                    DeviceManager();
                    break;

                case "screensaver":
                    playScreensaver = true;
                    Screensaver();
                    break;

                case "crash":
                    CrashScreen();
                    break;

                case "/eternalize":
                    Ending("gates");
                    break;

                case "/transcend":
                    Ending("tower");
                    break;

                case "/messenger":
                    Ending("crypt");
                    break;

                case "debug":
                    if (words.Length == 1)
                    {
                        Type("debug on - Activate debug messages", customGray, 0, 1);
                        Type("debug off - Deactivate debug messages", customGray, 0, 1);
                    }
                    else if (words[1] == "on")
                    {
                        debugMode = true;
                        Type("Debug mode activated.", customGray, 0, 1);
                    }
                    else if (words[1] == "off")
                    {
                        debugMode = false;
                        Type("Debug mode deactivated.", customGray, 0, 1);
                    }
                    
                    break;

                case "/banish":
                    Type("banish_text.txt (coming soon)", customGray, 0, 1);
                    break;

                case "access_comm_portal":
                    Type("access_comm_portal_text.txt (coming soon)", customGray, 0, 1);
                    break;

                default:
                    Type(GetResource("unknown_error_text.txt"), customGray, 0);
                    break;
            }

            // List the files in a given directory
            static void ListFiles(string directory)
            {
                Type("Searching for locally cached resources....", customGray);
                
                string[] dirList = Directory.GetDirectories(Path.Join(userResourceDir, directory));
                string[] fileList = Directory.GetFiles(Path.Join(userResourceDir, directory));

                foreach (string dir in dirList)
                {
                    string dirName = new DirectoryInfo(dir).Name;
                    Type("  " + dirName, customYellow, 1, 0);
                }

                foreach (string file in fileList)
                {
                    Type("  " + Path.GetFileName(file), customYellow, 1, 0);
                }

                Type("", customGray, 2, 1);
            }

            // Open a specified text file
            static void OpenFile(string file)
            {
                if (file.Contains(Path.AltDirectorySeparatorChar) || file.Contains(Path.DirectorySeparatorChar))
                {
                    file = Path.Join(userResourceDir, file);
                }

                var state = ConsoleState.GetState();

                try
                {
                    Console.CursorVisible = false;
                    Console.ForegroundColor = customGray;
                    Console.BufferHeight = 300;
                    Console.Clear();
                    Console.Write(GetResource(file));

                    while (true)
                    {
                        if (Wait(1000) == true)
                        {
                            return;
                        }
                    }
                }
                finally
                {
                    state.RestoreState();
                    Screensaver();
                }
            }

            static void ProcessCommandRun(string program)
            {
                if (program == "mla")
                {
                    Type("Milton Library Assistant currently unavailable. Please try again later.", customGray, 0);
                }
            }
        }

        // Search the program's resource files for the requested text file
        static string GetResource(string file, string resourceFolder = "program")
        {
            if (!Path.IsPathRooted(file))
            {
                var files = Directory.GetFiles(globalResourceDir, file, SearchOption.AllDirectories);
                file = files[0];
            }

            return File.ReadAllText(file);
        }

        static void DeviceManager()
        {
            Type(GetResource("device_manager_text.txt"), customGray);
        }

        // Eye screensaver
        static void Screensaver()
        {
            string frameDir = @"screensaver\eye\dots\";
            string frameName = "eye_frame*.txt";

            var state = ConsoleState.GetState();
            Random random = new Random();
            Console.ForegroundColor = customGray;
            Console.CursorVisible = false;
            int fixedPause = 70;
            List<string> frames = new List<string>();

            foreach (var file in Directory.GetFiles(Path.Join(programResourceDir, frameDir), frameName))
                frames.Add(GetResource(file));

            Thread.Sleep(500);
            Console.Clear();

            while (playScreensaver == true)
            {
                Console.CursorVisible = false;
                int randomPause = random.Next(600, 4000);

                DrawFrame(frames[0]);

                if (Wait(randomPause) == true)
                {
                    state.RestoreState();
                    return;
                }
                
                //ScreensaverSound();

                DrawFrame(frames[1]);

                if (Wait(fixedPause) == true)
                {
                    state.RestoreState();
                    return;
                }

                DrawFrame(frames[2]);

                if (Wait(fixedPause) == true)
                {
                    state.RestoreState();
                    return;
                }

                DrawFrame(frames[1]);

                if (Wait(fixedPause) == true)
                {
                    state.RestoreState();
                    return;
                }
            }

            static void DrawFrame(string frame)
            {
                Console.SetCursorPosition(0, 0);
                Console.WriteLine(frame);
            }

            /*
            async Task<> ScreensaverSound()
            {
                System.Random random = new System.Random();
                int randomPitch = random.Next(400, 450);
                int beepPause = 140;

                Console.Beep(randomPitch, 140);

               if (Wait(beepPause) == true)
                    return;

                Console.Beep(randomPitch + random.Next(-20, 50), 140);
            }
            */
        }

        // Function to wait until interrupted by a keypress
        static bool Wait(int pause)
        {
            var stopwatch = new Stopwatch();
            stopwatch.Start();
            while (stopwatch.ElapsedMilliseconds < pause)
            {
                if (Console.KeyAvailable)
                {
                    Console.ReadKey(true);
                    playScreensaver = false;
                    Console.Clear();
                    Console.CursorVisible = true;
                    return true;
                }

                Thread.Sleep(5);
            }

            return false;
        }

        static void CrashScreen()
        {
            var state = ConsoleState.GetState();
            ConsoleSettings.SetCurrentFont("Lucida Sans Typewriter", 24);
            Console.CursorVisible = false;
            Console.BackgroundColor = Color.FromArgb(0x0000ff);
            Console.Clear();
            Console.Write(GetResource("crash_text.txt"), Color.FromArgb(0xffffff));

            while (true)
            {
                if (Wait(1000) == true)
                {
                    Initialize();
                    state.RestoreState();
                    return;
                }
            }
        }

        static void Ending(string ending)
        {
            userPrefix = userPrefixes[0];

            if (ending == "gates")
            {
                Type(GetResource("gates_ending_text.txt"), customGray, 1, 0);
                Thread.Sleep(3000);
                Close();
            }

            if (ending == "crypt")
            {
                Type(GetResource("crypt_ending_text_1.txt"), customGray, 1, 1);
                Type(userPrefix, customGray, 1, 0);
                Console.ForegroundColor = customBlue;

                while (Console.KeyAvailable)
                {
                    Console.ReadKey(true);
                }

                input = Console.ReadLine();

                if (input == "cancel")
                {
                    throw new Exception("operation_cancelled");
                }
                else
                {
                    Type(GetResource("crypt_ending_text_2.txt"), customGray, 1, 0);
                    Thread.Sleep(3000);
                    Close();
                }
            }

            if (ending == "tower")
            {
                Type(GetResource("ower_ending_text_1.txt"), customGray, 1, 2);
                
                CheckAgain:
                Type(userPrefix, customGray, 0, 0);
                Console.ForegroundColor = customBlue;

                while (Console.KeyAvailable)
                {
                    Console.ReadKey(true);
                }

                input = Console.ReadLine();

                if (input == "cancel")
                {
                    throw new Exception("operation_cancelled");
                }
                else if (input == "/import milton library assistant")
                {
                    goto Part2;
                }
                else
                {
                    goto CheckAgain;
                }

                Part2:
                Type(GetResource("tower_ending_text_2.txt"), customGray, 1, 2);

                CheckAgain2:
                Type(userPrefix, customGray, 0, 0);
                Console.ForegroundColor = customBlue;

                while (Console.KeyAvailable)
                {
                    Console.ReadKey(true);
                }

                input = Console.ReadLine();

                if (input == "cancel")
                {
                    throw new Exception("operation_cancelled");
                }
                else if (input == "/copy library root")
                {
                    goto Part3;
                }
                else
                {
                    goto CheckAgain2;
                }

                Part3:
                Type(GetResource("tower_ending_text_3.txt"), customGray, 1, 2);

                CheckAgain3:
                Type(userPrefix, customGray, 0, 0);
                Console.ForegroundColor = customBlue;

                while (Console.KeyAvailable)
                {
                    Console.ReadKey(true);
                }

                input = Console.ReadLine();

                if (input == "cancel")
                {
                    throw new Exception("operation_cancelled");
                }
                else if (input == "/transcend")
                {
                    goto Part4;
                }
                else
                {
                    goto CheckAgain3;
                }

                Part4:
                Type(GetResource("tower_ending_text_4.txt"), customGray, 1, 0);
                Thread.Sleep(3000);
                Close();
            }
        }

        class ConsoleState
        {
            public ConsoleReader.CHAR_INFO[] data;
            public int cursorX;
            public int cursorY;
            public int bufferX;
            public int bufferY;

            public static ConsoleState GetState()
            {
                var state = new ConsoleState
                {
                    data = ConsoleReader.ReadFromBuffer(0, 0, (short)Console.BufferWidth, (short)Console.BufferHeight),
                    cursorX = Console.CursorLeft,
                    cursorY = Console.CursorTop,
                    bufferX = Console.BufferWidth,
                    bufferY = Console.BufferHeight
                };

                return state;
            }

            public void RestoreState()
            {
                Console.BufferWidth = bufferX;
                Console.BufferHeight = bufferY;
                Console.SetCursorPosition(0, 0);
                ConsoleReader.WriteToBuffer(0, 0, (short)Console.BufferWidth, (short)Console.BufferHeight, data);
                Console.SetCursorPosition(cursorX, cursorY);
                Console.WriteLine();
            }
        }

        //static Dictionary<string, string> resources = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);

        /*static void GetResources()
        {
            var resourcesDir = Path.Join(projectDir, "resources");
            foreach (var file in Directory.GetFiles(resourcesDir, "*.txt", SearchOption.AllDirectories))
            {
                var fileRelativePath = Path.GetRelativePath(resourcesDir, file);
                resources[fileRelativePath] = File.ReadAllText(file);
            }
        }*/
    }

    public static class ConsoleSettings
    {
        private const int FixedWidthTrueType = 54;
        private const int StandardOutputHandle = -11;

        [DllImport("kernel32.dll", SetLastError = true)]
        internal static extern IntPtr GetStdHandle(int nStdHandle);

        [return: MarshalAs(UnmanagedType.Bool)]
        [DllImport("kernel32.dll", SetLastError = true, CharSet = CharSet.Unicode)]
        internal static extern bool SetCurrentConsoleFontEx(IntPtr hConsoleOutput, bool MaximumWindow, ref FontInfo ConsoleCurrentFontEx);

        [return: MarshalAs(UnmanagedType.Bool)]
        [DllImport("kernel32.dll", SetLastError = true, CharSet = CharSet.Unicode)]
        internal static extern bool GetCurrentConsoleFontEx(IntPtr hConsoleOutput, bool MaximumWindow, ref FontInfo ConsoleCurrentFontEx);


        private static readonly IntPtr ConsoleOutputHandle = GetStdHandle(StandardOutputHandle);

        [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Unicode)]

        public struct FontInfo
        {
            internal int cbSize;
            internal int FontIndex;
            internal short FontWidth;
            public short FontSize;
            public int FontFamily;
            public int FontWeight;
            [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 32)]
            //[MarshalAs(UnmanagedType.ByValArray, ArraySubType = UnmanagedType.wc, SizeConst = 32)]
            public string FontName;
        }

        public static FontInfo[] SetCurrentFont(string font, short fontSize = 0)
        {
            //Console.WriteLine("Set Current Font: " + font);

            FontInfo before = new FontInfo
            {
                cbSize = Marshal.SizeOf<FontInfo>()
            };

            if (GetCurrentConsoleFontEx(ConsoleOutputHandle, false, ref before))
            {

                FontInfo set = new FontInfo
                {
                    cbSize = Marshal.SizeOf<FontInfo>(),
                    FontIndex = 0,
                    FontFamily = FixedWidthTrueType,
                    FontName = font,
                    FontWeight = 400,
                    FontSize = fontSize > 0 ? fontSize : before.FontSize
                };

                // Get some settings from current font.
                if (!SetCurrentConsoleFontEx(ConsoleOutputHandle, false, ref set))
                {
                    var ex = Marshal.GetLastWin32Error();
                    Console.WriteLine("Set error " + ex);
                    throw new System.ComponentModel.Win32Exception(ex);
                }

                FontInfo after = new FontInfo
                {
                    cbSize = Marshal.SizeOf<FontInfo>()
                };

                GetCurrentConsoleFontEx(ConsoleOutputHandle, false, ref after);
                return new[] { before, set, after };
            }
            else
            {
                var er = Marshal.GetLastWin32Error();
                Console.WriteLine("Get error " + er);
                throw new System.ComponentModel.Win32Exception(er);
            }
        }
    }

    public static class ConsoleReader
    {
        public static CHAR_INFO[] ReadFromBuffer(short x, short y, short width, short height)
        {
            IntPtr buffer = Marshal.AllocHGlobal(width * height * Marshal.SizeOf(typeof(CHAR_INFO)));
            if (buffer == null)
                throw new OutOfMemoryException();

            try
            {
                COORD coord = new COORD();
                SMALL_RECT rc = new SMALL_RECT
                {
                    Left = x,
                    Top = y,
                    Right = (short)(x + width - 1),
                    Bottom = (short)(y + height - 1)
                };

                COORD size = new COORD
                {
                    X = width,
                    Y = height
                };

                const int STD_OUTPUT_HANDLE = -11;
                if (!ReadConsoleOutput(GetStdHandle(STD_OUTPUT_HANDLE), buffer, size, coord, ref rc))
                {
                    // 'Not enough storage is available to process this command' may be raised for buffer size > 64K (see ReadConsoleOutput doc.)
                    throw new Win32Exception(Marshal.GetLastWin32Error());
                }

                CHAR_INFO[] data = new CHAR_INFO[height * width];
                IntPtr ptr = buffer;
                for (int h = 0; h < height; h++)
                {
                    //StringBuilder sb = new StringBuilder();
                    for (int w = 0; w < width; w++)
                    {
                        CHAR_INFO ci = (CHAR_INFO)Marshal.PtrToStructure(ptr, typeof(CHAR_INFO));
                        data[h * width + w] = ci;
                        //char[] chars = Console.OutputEncoding.GetChars(ci.charData);
                        //sb.Append(chars[0]);
                        ptr += Marshal.SizeOf(typeof(CHAR_INFO));
                    }
                }
                
                return data;
            }
            finally
            {
                Marshal.FreeHGlobal(buffer);
            }
        }

        public static void WriteToBuffer(short x, short y, short width, short height, CHAR_INFO[] data)
        {
            IntPtr buffer = Marshal.AllocHGlobal(width * height * Marshal.SizeOf(typeof(CHAR_INFO)));
            if (buffer == null)
                throw new OutOfMemoryException();

            try
            {
                IntPtr ptr = buffer;
                for (int h = 0; h < height; h++)
                {
                    //StringBuilder sb = new StringBuilder();
                    for (int w = 0; w < width; w++)
                    {
                        Marshal.StructureToPtr(data[h * width + w], ptr, true);
                        //char[] chars = Console.OutputEncoding.GetChars(ci.charData);
                        //sb.Append(chars[0]);
                        ptr += Marshal.SizeOf(typeof(CHAR_INFO));
                    }
                }

                COORD coord = new COORD();
                SMALL_RECT rc = new SMALL_RECT
                {
                    Left = x,
                    Top = y,
                    Right = (short)(x + width - 1),
                    Bottom = (short)(y + height - 1)
                };

                COORD size = new COORD
                {
                    X = width,
                    Y = height
                };

                const int STD_OUTPUT_HANDLE = -11;
                if (!WriteConsoleOutput(GetStdHandle(STD_OUTPUT_HANDLE), data, size, coord, ref rc))
                {
                    // 'Not enough storage is available to process this command' may be raised for buffer size > 64K (see ReadConsoleOutput doc.)
                    throw new Win32Exception(Marshal.GetLastWin32Error());
                }

            }
            finally
            {
                Marshal.FreeHGlobal(buffer);
            }
        }

        [StructLayout(LayoutKind.Sequential)]
        public struct CHAR_INFO
        {
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 2)]
            public byte[] charData;
            public short attributes;
        }

        [StructLayout(LayoutKind.Sequential)]
        private struct COORD
        {
            public short X;
            public short Y;
        }

        [StructLayout(LayoutKind.Sequential)]
        private struct SMALL_RECT
        {
            public short Left;
            public short Top;
            public short Right;
            public short Bottom;
        }

        [StructLayout(LayoutKind.Sequential)]
        private struct CONSOLE_SCREEN_BUFFER_INFO
        {
            public COORD dwSize;
            public COORD dwCursorPosition;
            public short wAttributes;
            public SMALL_RECT srWindow;
            public COORD dwMaximumWindowSize;
        }

        [DllImport("kernel32.dll", SetLastError = true)]
        private static extern bool ReadConsoleOutput(IntPtr hConsoleOutput, IntPtr lpBuffer, COORD dwBufferSize, COORD dwBufferCoord, ref SMALL_RECT lpReadRegion);

        [DllImport("kernel32.dll", SetLastError = true)]
        private static extern IntPtr GetStdHandle(int nStdHandle);

        [DllImport("kernel32.dll", SetLastError = true, EntryPoint = "WriteConsoleOutputW", CharSet = CharSet.Unicode)]
        private static extern bool WriteConsoleOutput(IntPtr hConsoleOutput, CHAR_INFO[] lpBuffer, COORD dwBufferSize, COORD dwBufferCoord, ref SMALL_RECT lpWriteRegion);

    }
}