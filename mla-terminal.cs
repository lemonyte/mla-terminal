/* 
 * To Do List:
 * - Add all found texts
 * - Add basic MLA query interface
 * - Add admin account certification program
 * - Remove all "goto" statements
 * - Add tags to make coloring and making delays easier
*/

using System;
using System.Threading;
using System.Drawing;
using Console = Colorful.Console;
using System.Collections.Generic;
using System.IO;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using System.Linq;
//using System.IO.Compression;
//using System.ComponentModel;
//using System.Reflection;
//using System.Collections;
//using Terminal.Gui;

namespace mla_terminal
{
    class Program
    {
        readonly static string workingDir = Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location);
        //readonly static string projectDir = Directory.GetParent(workingDir).Parent.Parent.FullName;
        readonly static string projectDir = workingDir;
        readonly static string globalResourceDir = Path.Join(projectDir, "resources");
        readonly static string programResourceDir = Path.Join(projectDir, "resources", "program");
        readonly static string userResourceDir = Path.Join(projectDir, "resources", "user", "found_texts");
        readonly static string resourceZipPath = Path.Join(projectDir, "resources.zip");
        readonly static string resourceGroPath = Path.Join(projectDir, "resources.gro");
        static string input;

        /* 
         * String array containing all of the user prefixes
         * [0] = "> "
         * [1] = "[guest@local]# "
         * [2] = "[guest@unknown]# "
         * [3] = "[admin@local]# "
         * [4] = "[1-4] "
        */
        static string[] userPrefixes;
        static string userPrefix;
        static bool exitProgram = false;
        static bool debugMode = false;
        static bool isAdmin = false;
        static bool enableResourcePacking = true;

        public static class Floors
        {
            public static Dictionary<string, string> floors = new()
            {
                {"0", "unlocked"},
                {"1", "unlocked"},
                {"2", "locked - requires code"},
                {"3", "locked - requires code"},
                {"4", "locked - requires code"},
                {"5", "locked - requires code"},
                {"6", "locked - requires code"},
            };

            public static Dictionary<string, string> floorCodes = new()
            {
                {"0", "000"},
                {"1", "000"},
                {"2", "000"},
                {"3", "000"},
                {"4", "000"},
                {"5", "000"},
                {"6", "000"},
            };
        }

        readonly static Color customGray = Color.FromArgb(224, 216, 196);
        readonly static Color customBlue = Color.FromArgb(140, 202, 230);
        readonly static Color customYellow = Color.FromArgb(205, 208, 141);

        static void Main(string[] args)
        {
            Boot(args);

            while (true)
            {
                if (exitProgram)
                {
                    return;
                }

                GetCommand(userPrefix, customGray, customBlue);
            }
        }

        public static int Initialize(string[] args)
        {
            try
            {
                if (args.Contains("--debug") || args.Contains("-d"))
                {
                    debugMode = true;
                }

                if (args.Contains("--noresourcepack") || args.Contains("-n"))
                {
                    enableResourcePacking = false;
                }

                if (File.Exists(resourceGroPath))
                {
                    if (File.Exists(resourceZipPath))
                        File.Delete(resourceZipPath);

                    File.Move(resourceGroPath, resourceZipPath);              
                }

                if (File.Exists(resourceZipPath))
                {
                    if (Directory.Exists(globalResourceDir))
                        Directory.Delete(globalResourceDir, true);

                    System.IO.Compression.ZipFile.ExtractToDirectory(Path.Join(projectDir, "resources.zip"), globalResourceDir);
                    File.Delete(resourceZipPath);
                }

                if (!File.Exists(Path.Join(programResourceDir, "text", "user_prefix", "user_prefix.txt")))
                {
                    Typewrite("Initialization failed. Could not find resource files. Press any key to exit.", emptyLinesStart: 1, emptyLinesEnd: 2);
                    Console.ReadKey();
                    Close();
                    return 0;
                }

                userPrefixes = File.ReadAllLines(Path.Join(programResourceDir, "text", "user_prefix", "user_prefix.txt"));
                ConsoleSettings.SetCurrentFont("DejaVu Sans Mono", 24);
                Console.Title = "MLA Terminal";
                Console.CursorSize = 100;
                Console.BackgroundColor = Color.FromArgb(0x000000);
                userPrefix = userPrefixes[1];
                Console.Clear();
                ConsoleSettings.EnableVTMode();

                try
                {
                    Console.WindowWidth = 90;
                    Console.WindowHeight = 35;
                    Console.BufferWidth = 90;
                    Console.BufferHeight = 350;
                }
                catch
                {
                    if (debugMode)
                    {
                        Typewrite("Failed to set console window and buffer size.", emptyLinesEnd: 1);
                    }
                }

                return 1;
            }
            catch (Exception exception)
            {
                Typewrite("Initialization failed. Press any key to exit.", emptyLinesStart: 1, emptyLinesEnd: 2);

                if (debugMode)
                {
                    Typewrite(exception.Message, Color.Red, emptyLinesEnd: 2);
                }

                Console.ReadKey();
                Close();
                return 0;
            }
        }

        static void Boot(string[] args)
        {
            if (Initialize(args) == 1)
            {
                Typewrite(GetResource("boot.txt"));
            }
        }
        
        static void Exit()
        {
            exitProgram = true;
        }

        static void Close()
        {
            if (enableResourcePacking)
            {
                if (Directory.Exists(globalResourceDir))
                {
                    if (File.Exists(resourceZipPath))
                        File.Delete(resourceZipPath);

                    System.IO.Compression.ZipFile.CreateFromDirectory(globalResourceDir, resourceZipPath);

                    if (File.Exists(resourceGroPath))
                        File.Delete(resourceGroPath);

                    File.Move(resourceZipPath, resourceGroPath);
                    Directory.Delete(globalResourceDir, true);
                }
            }

            // ConsoleSettings.SetCurrentFont("Consolas", 14);
            Console.ResetColor();
            Exit();
        }

        static void Typewrite(string str = "", Color? textColor = null, int emptyLinesStart = 0, int emptyLinesEnd = 0, int delay = 1)
        {
            Console.CursorVisible = false;
            Console.ForegroundColor = textColor ?? customGray;

            for (int i = 0; i < emptyLinesStart; i++)
            {
                Console.WriteLine();
            }

            if (str == "")
            {
                Console.CursorVisible = true;
                return;
            }

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

        static void GetCommand(string prefix = "", Color? prefixColor = null, Color? inputColor = null, bool lower = true)
        {
            /*Type(prefix, prefixColor ?? customGray);
            Console.ForegroundColor = inputColor ?? customBlue;

            // Discard keypresses while the program is typing
            while (Console.KeyAvailable)
            {
                Console.ReadKey(true);
            }*/

            input = Input(prefix, lower: lower);

            // Split the input into keywords
            //var words = input.Split(' ', StringSplitOptions.RemoveEmptyEntries);
            var words = input.Split(new char[] {'"', '\''}).Select((element, index) => index % 2 == 0 ? element.Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries) : new string[] { element }).SelectMany(element => element).ToArray();

            if (debugMode)
            {
                Console.ForegroundColor = customGray;
                Typewrite("Input strings:", emptyLinesStart: 1, emptyLinesEnd: 1);
                foreach (var word in words)
                {
                    Typewrite(word.ToString(), emptyLinesEnd: 1);
                }
                Typewrite(emptyLinesEnd: 1);
            }

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
                if (exception.Message == "cancelled")
                {
                    Typewrite("Operation cancelled.", emptyLinesStart: 1, emptyLinesEnd: 2);
                    userPrefix = userPrefixes[1];
                    return;
                }

                Typewrite("Exception thrown.", emptyLinesStart: 1, emptyLinesEnd: 2);

                if (debugMode)
                {
                    Typewrite(exception.Message, Color.Red, emptyLinesEnd: 2);
                }
            }
        }

        static string Input(string prefix = "", Color? prefixColor = null, Color? inputColor = null, bool lower = true)
        {
            Typewrite(prefix, prefixColor ?? customGray);
            Console.ForegroundColor = inputColor ?? customBlue;

            // Discard keypresses while the program is typing
            while (Console.KeyAvailable)
            {
                Console.ReadKey(true);
            }

            if (lower)
                return Console.ReadLine().ToLower();

            else
                return Console.ReadLine();
        }

        // Process the command entered by the user
        static void ProcessCommand(string[] words)
        {
            switch (words[0].ToLower())
            {
                case "help":
                    Typewrite(GetResource("help.txt"));
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
                        Typewrite("Please enter a file to open.", emptyLinesEnd: 1);
                    break;

                case "run":
                    if (words.Length > 1)
                        ProcessCommandRun(words[1]);
                    else
                        Typewrite("Please enter a program to run.", emptyLinesEnd: 1);
                    break;

                case "exit":
                    if (words.Length > 1)
                    {
                        if (words[1] == "force")
                        {
                            Exit();
                        }
                        else
                        {
                            Typewrite("exit - close the terminal session", emptyLinesStart: 1, emptyLinesEnd: 1);
                            Typewrite("exit force - force the terminal session to close", emptyLinesEnd: 2);
                        }
                    }
                    else
                    {
                        Typewrite(GetResource("exit.txt"));
                        Console.CursorVisible = false;
                        Thread.Sleep(1000);
                        Close();
                    }

                    break;
                    
                case "admin":
                    AdminAuth();
                    break;

                case "device_manager":
                    DeviceManager();
                    break;

                case "screensaver":
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
                    if (words.Length > 1 && words[1] != "on" && words[1] != "off" || words.Length < 2)
                    {
                        Typewrite("debug on - Activate debug messages", emptyLinesStart: 1 , emptyLinesEnd: 1);
                        Typewrite("debug off - Deactivate debug messages", emptyLinesEnd: 2);
                    }
                    else if (words[1] == "on")
                    {
                        debugMode = true;
                        Typewrite("Debug mode activated.", emptyLinesEnd: 1);
                    }
                    else if (words[1] == "off")
                    {
                        debugMode = false;
                        Typewrite("Debug mode deactivated.", emptyLinesEnd: 1);
                    }
                    
                    break;

                case "/banish":
                    Banish();
                    break;

                case "access_comm_portal":
                    Typewrite(GetResource("access_comm_portal.txt"));
                    break;

                default:
                    Typewrite(GetResource("unknown_error.txt"));
                    break;
            }

            // List the files in a given directory
            static void ListFiles(string directory)
            {
                Typewrite("Searching for locally cached resources....", emptyLinesEnd: 1);
                
                try
                {   
                    string[] dirList = Directory.GetDirectories(Path.Join(userResourceDir, directory));
                    string[] fileList = Directory.GetFiles(Path.Join(userResourceDir, directory));

                    foreach (string dir in dirList)
                    {
                        string dirName = new DirectoryInfo(dir).Name;
                        Typewrite("  " + dirName, customYellow, emptyLinesStart: 1);
                    }

                    foreach (string file in fileList)
                    {
                        Typewrite("  " + Path.GetFileName(file), customYellow, emptyLinesStart: 1);
                    }

                    Typewrite("", emptyLinesStart: 3);
                }
                catch (Exception exception)
                {
                    if (exception is DirectoryNotFoundException or IOException)
                    {
                        Typewrite($"Could not find folder: '{directory}'", emptyLinesEnd: 1);
                    }
                }
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
                    try
                    {
                        Console.CursorVisible = false;
                        Console.ForegroundColor = customGray;
                        Console.BufferHeight = 300;
                        Console.Clear();
                        Console.Write(GetResource(file));

                        while (true)
                        {
                            if (Wait(1000))
                            {
                                return;
                            }
                        }
                    }
                    finally
                    {
                        state.RestoreState(emptyLinesEnd: 0);
                    }
                }
                catch (FileNotFoundException)
                {
                    Typewrite($"Could not find file: '{file}'", emptyLinesEnd: 1);
                }
            }

            static void ProcessCommandRun(string program)
            {
                if (program == "mla")
                {
                    MLAInterface();
                }
                else
                {
                    Typewrite($"Could not find program: '{program}'", emptyLinesEnd: 1);
                }
            }
        }

        // Search the program's resource files for the requested text file
        static string GetResource(string file)
        {
            try
            {
                if (!Path.IsPathRooted(file))
                {
                    var files = Directory.GetFiles(globalResourceDir, file, SearchOption.AllDirectories);
                    file = files[0];
                }

                return File.ReadAllText(file);
            }
            catch (Exception)
            {
                throw new FileNotFoundException();
            }
        }

        static void MLAInterface()
        {
            Typewrite("Milton Library Assistant is currently unavailable. Please try again later.", emptyLinesEnd: 1);
        }

        static void AdminAuth()
        {
            Typewrite("Administrator authentication program currently unavailable. Please try again later.", emptyLinesEnd: 1);
            userPrefix = userPrefixes[3];
            isAdmin = true;
        }

        static void DeviceManager()
        {
            Typewrite(GetResource("device_manager_boot.txt"));
            DeviceManagerOptions();

            static void DeviceManagerOptions()
            {
                Typewrite(GetResource("device_manager_options.txt"));
                input = Input();
                
                switch (input)
                {
                    case "1":
                        Typewrite(GetResource("device_manager_1.txt"));
                        DeviceManagerOptions();
                        return;

                    case "2":
                        DeviceManagerFloors();
                        return;

                    case "3":
                        Typewrite(GetResource("device_manager_3.txt"));
                        DeviceManagerOptions();
                        return;

                    case "4":
                        Typewrite(GetResource("device_manager_4.txt"));
                        return;

                    default:
                        DeviceManagerOptions();
                        return;
                }
            }

            static void DeviceManagerFloors()
            {
                Typewrite(GetResource("device_manager_2.txt"));

                foreach (KeyValuePair<string, string> floor in Floors.floors)
                {
                    Typewrite($"  floor {floor.Key}: {floor.Value}", emptyLinesEnd: 1);
                }

                Typewrite("Select floor to reconfigure [0-6]: ", emptyLinesStart: 1);

                input = Input();

                if (Floors.floors.ContainsKey(input))
                    {
                    if (Floors.floors[input].Contains("unlocked"))
                    {
                        LockFloor();
                        DeviceManagerOptions();
                    }
                    else
                    {
                        UnlockFloor(input);
                        DeviceManagerOptions();
                    }
                }
                else
                {
                    DeviceManagerOptions();
                }

                static void LockFloor()
                {
                    Typewrite(GetResource("lock_floor_1.txt"));
                    input = Input();

                    if (input != "y")
                    {
                        return;
                    }

                    Typewrite(GetResource("lock_floor_2.txt"));
                    input = Input();

                    if (input != "yes i am sure")
                    {
                        return;
                    }

                    Typewrite(GetResource("lock_floor_3.txt"));
                }

                static void UnlockFloor(string floor)
                {
                    int triesRemaining = 3;
                    Typewrite("Access to that floor is protected with a code.", emptyLinesStart: 1, emptyLinesEnd: 1);

                    CheckAgain:
                    input = Input("Please enter the code: ");

                    if (input == Floors.floorCodes[floor])
                    {
                        Floors.floors[floor] = "unlocked";
                        Typewrite("Floor unlocked.", emptyLinesStart: 1, emptyLinesEnd: 1);
                        return;
                    }
                    else
                    {
                        triesRemaining--;
                        
                        if (triesRemaining < 1)
                        {
                            Typewrite("No more tries left.", emptyLinesStart: 1, emptyLinesEnd: 1);
                            return;
                        }
                        else
                        {
                            Typewrite("Code incorrect!", emptyLinesStart: 1, emptyLinesEnd: 1);
                            Typewrite($"Try again ({triesRemaining} more tries left).", emptyLinesStart: 1, emptyLinesEnd: 1);
                            goto CheckAgain;
                        }
                    }
                }
            }
        }

        static void Banish()
        {
            if (isAdmin)
            {
                Typewrite(GetResource("banish.txt"));
                Input();
                Typewrite(GetResource("banish_2.txt"));
                CheckAgain:

                if (Input() != "resume")
                {
                    Typewrite(GetResource("banished.txt"));
                    goto CheckAgain;
                }
                else
                {
                    Typewrite(GetResource("resume_session.txt"));
                }
            }
            else
            {
                Typewrite("Administrator permissions required.", emptyLinesEnd: 1);
            }
        }

        static void Screensaver()
        {
            string frameDir = @"screensaver\eye\dots\";
            string frameName = "eye_frame*.txt";
            var state = ConsoleState.GetState();
            Random random = new();
            Console.ForegroundColor = customGray;
            Console.CursorVisible = false;
            int fixedPause = 70;
            List<string> frames = new();
            Thread.Sleep(500);
            Console.Clear();

            foreach (var file in Directory.GetFiles(Path.Join(programResourceDir, frameDir), frameName))
                frames.Add(GetResource(file));

            while (true)
            {
                Console.CursorVisible = false;
                int randomPause = random.Next(600, 4000);

                DrawFrame(frames[0]);

                if (Wait(randomPause))
                {
                    state.RestoreState();
                    return;
                }
                
                Task.Run(() => {
                    ScreensaverSound();
                });

                DrawFrame(frames[1]);

                if (Wait(fixedPause))
                {
                    state.RestoreState();
                    return;
                }

                DrawFrame(frames[2]);

                if (Wait(fixedPause))
                {
                    state.RestoreState();
                    return;
                }

                DrawFrame(frames[1]);

                if (Wait(fixedPause))
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

            static void ScreensaverSound()
            {
                System.Random random = new();
                int randomPitch = random.Next(400, 450);
                int beepPause = 140;

                Console.Beep(randomPitch, 140);
                Thread.Sleep(beepPause);
                Console.Beep(randomPitch + random.Next(-20, 50), 140);
            }
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
            Console.Write(GetResource("crash.txt"), Color.FromArgb(0xffffff));

            while (true)
            {
                if (Wait(1000))
                {
                    Initialize(new string[] {""});
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
                Typewrite(GetResource("gates_ending.txt"));
                Thread.Sleep(3000);
                Close();
            }

            if (ending == "crypt")
            {
                Typewrite(GetResource("crypt_ending_1.txt"));
                Input();
                Typewrite(GetResource("crypt_ending_2.txt"));
                Thread.Sleep(3000);
                Close();
            }

            if (ending == "tower")
            {
                Typewrite(GetResource("tower_ending_1.txt"));
                
                CheckAgain:
                input = Input(userPrefix).ToLower();

                if (input == "cancel")
                {
                    throw new Exception("cancelled");
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
                Typewrite(GetResource("tower_ending_2.txt"));

                CheckAgain2:
                input = Input(userPrefix).ToLower();

                if (input == "cancel")
                {
                    throw new Exception("cancelled");
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
                Typewrite(GetResource("tower_ending_3.txt"));

                CheckAgain3:
                input = Input(userPrefix).ToLower();

                if (input == "cancel")
                {
                    throw new Exception("cancelled");
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
                Typewrite(GetResource("tower_ending_4.txt"));
                Thread.Sleep(3000);
                Close();
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

            public void RestoreState(int emptyLinesEnd = 1)
            {
                Console.BufferWidth = bufferX;
                Console.BufferHeight = bufferY;
                Console.SetCursorPosition(0, 0);
                ConsoleReader.WriteToBuffer(0, 0, (short)Console.BufferWidth, (short)Console.BufferHeight, data);
                Console.SetCursorPosition(cursorX, cursorY);
                Typewrite(emptyLinesEnd: emptyLinesEnd);
            }
        }
    }

    public static class ConsoleSettings
    {
        private const int FIXED_WIDTH_TRUE_TYPE = 54;
        private const int STD_OUTPUT_HANDLE = -11;
        private const uint ENABLE_VIRTUAL_TERMINAL = 0x0004;
        private const uint DISABLE_NEWLINE_AUTO_RETURN = 0x0008;

        [DllImport("kernel32.dll", SetLastError = true)]
        internal static extern IntPtr GetStdHandle(int nStdHandle);

        [DllImport("kernel32.dll")]
#pragma warning disable CA1401 // P/Invokes should not be visible
        public static extern uint GetLastError();
#pragma warning restore CA1401 // P/Invokes should not be visible

        [DllImport("kernel32.dll")]
        private static extern bool GetConsoleMode(IntPtr hConsoleHandle, out uint lpMode);

        [DllImport("kernel32.dll")]
        private static extern bool SetConsoleMode(IntPtr hConsoleHandle, uint dwMode);

        [return: MarshalAs(UnmanagedType.Bool)]
        [DllImport("kernel32.dll", SetLastError = true, CharSet = CharSet.Unicode)]
        internal static extern bool SetCurrentConsoleFontEx(IntPtr hConsoleOutput, bool MaximumWindow, ref FontInfo ConsoleCurrentFontEx);

        [return: MarshalAs(UnmanagedType.Bool)]
        [DllImport("kernel32.dll", SetLastError = true, CharSet = CharSet.Unicode)]
        internal static extern bool GetCurrentConsoleFontEx(IntPtr hConsoleOutput, bool MaximumWindow, ref FontInfo ConsoleCurrentFontEx);
        private static readonly IntPtr ConsoleOutputHandle = GetStdHandle(STD_OUTPUT_HANDLE);

        [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Unicode)]

        public struct FontInfo
        {
            internal int cbSize;
            internal int FontIndex;
            internal short FontWidth;
            public short FontSize;
            public int FontFamily;
            public int FontWeight;
            //[MarshalAs(UnmanagedType.ByValArray, ArraySubType = UnmanagedType.wc, SizeConst = 32)]
            [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 32)]
            public string FontName;
        }

        public static FontInfo[] SetCurrentFont(string font, short fontSize = 0)
        {
            //Console.WriteLine("Set Current Font: " + font);

            FontInfo before = new()
            {
                cbSize = Marshal.SizeOf<FontInfo>()
            };

            if (GetCurrentConsoleFontEx(ConsoleOutputHandle, false, ref before))
            {
                FontInfo set = new()
                {
                    cbSize = Marshal.SizeOf<FontInfo>(),
                    FontIndex = 0,
                    FontFamily = FIXED_WIDTH_TRUE_TYPE,
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

                FontInfo after = new()
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
        public static void EnableVTMode()
        {
            //var iStdOut = GetStdHandle(StdOutHandle);
            if (!GetConsoleMode(ConsoleOutputHandle, out uint outConsoleMode))
            {
                Console.WriteLine("Failed to get output console mode");
                Console.ReadKey();
                return;
            }

            outConsoleMode |= ENABLE_VIRTUAL_TERMINAL | DISABLE_NEWLINE_AUTO_RETURN;
            if (!SetConsoleMode(ConsoleOutputHandle, outConsoleMode))
            {
                Console.WriteLine("Failed to set output console mode, error code: " + GetLastError());
                Console.ReadKey();
                return;
            }
        }
    }

    public static class ConsoleReader
    {
        public static CHAR_INFO[] ReadFromBuffer(short x, short y, short width, short height)
        {
            IntPtr buffer = Marshal.AllocHGlobal(width * height * Marshal.SizeOf(typeof(CHAR_INFO)));
            if (buffer == IntPtr.Zero)
                throw new OutOfMemoryException();

            try
            {
                COORD coord = new();
                SMALL_RECT rc = new()
                {
                    Left = x,
                    Top = y,
                    Right = (short)(x + width - 1),
                    Bottom = (short)(y + height - 1)
                };

                COORD size = new()
                {
                    X = width,
                    Y = height
                };

                const int STD_OUTPUT_HANDLE = -11;
                if (!ReadConsoleOutput(GetStdHandle(STD_OUTPUT_HANDLE), buffer, size, coord, ref rc))
                {
                    // 'Not enough storage is available to process this command' may be raised for buffer size > 64K (see ReadConsoleOutput doc.)
                    throw new System.ComponentModel.Win32Exception(Marshal.GetLastWin32Error());
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
            if (buffer == IntPtr.Zero)
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

                COORD coord = new();
                SMALL_RECT rc = new()
                {
                    Left = x,
                    Top = y,
                    Right = (short)(x + width - 1),
                    Bottom = (short)(y + height - 1)
                };

                COORD size = new()
                {
                    X = width,
                    Y = height
                };

                const int STD_OUTPUT_HANDLE = -11;
                if (!WriteConsoleOutput(GetStdHandle(STD_OUTPUT_HANDLE), data, size, coord, ref rc))
                {
                    // 'Not enough storage is available to process this command' may be raised for buffer size > 64K (see ReadConsoleOutput doc.)
                    throw new System.ComponentModel.Win32Exception(Marshal.GetLastWin32Error());
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