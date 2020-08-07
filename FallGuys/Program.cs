using System;
using System.IO;
using System.Threading;
using Newtonsoft.Json;
using static FallGuys.WinImports;



namespace FallGuys
{
    class Program
    {
        private static Config config;
        private static long currentBufferSize = 8096;
        private static char[] buffer = new char[8096];
        private static readonly string appGuid = "{A942E48C-7BF3-4C84-B468-130685E90969}";
        static bool InitConfig()
        {
            string json;
            try
            {
                json = Utils.GetRemoteConfigJson();
            }
            catch (Exception ex)
            {
                return false;
            }
            config = JsonConvert.DeserializeObject<Config>(json);
            return true;
        }
        static void Main(string[] args)
        {
            bool exclusive;
            using (var m = new Mutex(true, appGuid, out exclusive))
            {
                if (!exclusive)
                {
                    Console.WriteLine("Application is already running");
                    return;
                };
                if (!InitConfig())
                {
                    Console.WriteLine("Could not configure application");
                    return;
                }
                var appdataDir = Directory.GetParent(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData)).FullName;

                var logPath = Path.Combine(appdataDir, config.LogDirectory, config.LogFileName);
                var fallGuysWindowName = config.WindowName;

                var info = new FileInfo(logPath);
                var oldTime = info.LastWriteTimeUtc;
                long index = -1;

                while (true)
                {
                    try
                    {
                        CheckLogFile(logPath, ref index);
                        break;
                    }
                    catch(Exception ex)
                    {
                        Console.WriteLine("Could not open file fallguys log file");
                    }
                    Thread.Sleep(2000);
                }

                while (true)
                {
                    do
                    {
                        Thread.Sleep(500);
                        info.Refresh();
                    } while (info.LastWriteTimeUtc == oldTime);

                    try
                    {
                        var stateChange = CheckLogFile(logPath, ref index);
                        if (stateChange)
                        {
                            Console.WriteLine("Activate: Bringing game to front");

                            bringToFront(fallGuysWindowName);
                        }
                        oldTime = info.LastWriteTimeUtc;

                    }
                    catch (IOException)
                    {
                        Console.WriteLine("Could not open file");
                        oldTime = info.LastWriteTimeUtc;
                    }
                }
            }
            

        }


        private static bool CheckLogFile(string path, ref long index)
        {
            using var fileStream = new FileStream(
                path,
                FileMode.Open,
                FileAccess.Read,
                FileShare.ReadWrite);
            using var streamReader = new StreamReader(fileStream);
            if (index == -1)
            {
                index = streamReader.BaseStream.Length;
                Console.WriteLine("File length : " + index);
            }
            else
            {
                var newLen = streamReader.BaseStream.Length;
                if (newLen < index)
                {
                    index = newLen;
                    return false;
                }
                var allocate = false;
                while (newLen> currentBufferSize)
                {
                    currentBufferSize *= 2;
                    allocate = true;
                }
                if (allocate)
                {
                    buffer = new char[currentBufferSize];
                }
                var count = (int)(newLen - index );
                Console.WriteLine("FileSize: " + newLen + " Count: " + count + " OldSize: " + index);
                streamReader.BaseStream.Seek(index, SeekOrigin.Begin);
                streamReader.DiscardBufferedData();
                streamReader.Read(buffer, 0, count);
                index = newLen;
                var newLines = new string(buffer);
                Console.WriteLine(newLines);
                var pattern = config.DetectCountdownPattern;

                if (newLines.ToLower().Contains(pattern))
                {
                    return true;
                }
            }

            return false;
        }

        private static readonly int SW_RESTORE = 9;
        private static readonly byte VK_MENU = 0x12;
        private static readonly int KEYEVENTF_EXTENDEDKEY = 0x1;
        private static readonly int KEYEVENTF_KEYUP = 0x2;
        private static readonly int SW_SHOW = 5;


        public static void bringToFront(string title)
        {
            IntPtr handle = FindWindow(null, title);


            if (handle == IntPtr.Zero)
            {
                return;
            }

            //ShowWindowAsync(new HandleRef(null, handle), SW_RESTORE);
            //SetForegroundWindow(handle);
            //SetForegroundWindowInternal(handle);
            ForceForegroundWindow(handle);
        }

        /// <summary>
        /// Forces window to foreground (SetForegroundWindow Workaround)
        /// by attaching the current thread to the actual foreground app window thread
        /// </summary>
        /// <param name="hWnd"></param>
        private static void ForceForegroundWindow(IntPtr hWnd)

        {
            uint foreThread = GetWindowThreadProcessId(GetForegroundWindow(), IntPtr.Zero);
            int appThread = GetCurrentThreadId();
            if (foreThread != appThread)

            {
                AttachThreadInput((int)foreThread, appThread, true);
                BringWindowToTop(hWnd);
                ShowWindow(hWnd, 0x3 );
                AttachThreadInput((int)foreThread, appThread, false);
                SetForegroundWindow(hWnd);
            }

            else

            {
                BringWindowToTop(hWnd);
                ShowWindow(hWnd, SW_SHOW);
                SetForegroundWindow(hWnd);
            }

        }

        /// <summary>
        /// Forces window to foreground (SetForegroundWindow Workaround)
        /// imitating an alt-tab keyboard event
        /// This method is probably less intrusive than the thread
        /// </summary>
        /// <param name="ptr"></param>
        static void SetForegroundWindowInternal(IntPtr ptr)
        {

            var  keyState = new byte[256];
            //to unlock SetForegroundWindow we need to imitate Alt pressing
            if (GetKeyboardState(keyState))
            {
                if ((keyState[VK_MENU] & 0x80) != 0 )
                {
                    keybd_event(VK_MENU, 0, KEYEVENTF_EXTENDEDKEY | 0, 0);
                }
            }
            SetForegroundWindow(ptr);
            if (GetKeyboardState(keyState))
            {
                if ((keyState[VK_MENU] & 0x80) != 0)
                {
                    keybd_event(VK_MENU, 0, KEYEVENTF_EXTENDEDKEY | KEYEVENTF_KEYUP, 0);
                }
            }
        }

        static void OldMethod()
        {
            //using var watcher = new FileSystemWatcher();
            //watcher.Path = logPath;
            //watcher.NotifyFilter = NotifyFilters.Size;
            //watcher.Filter = "Player.log";
            //watcher.Created += (s, e) => Console.WriteLine("Created: " + e.FullPath);
            //watcher.Deleted += (s, e) => Console.WriteLine("Deleted: " + e.FullPath);
            //watcher.Changed += (s, e) => Console.WriteLine("Changed: " + e.FullPath);
            //watcher.Renamed += (s, e) => Console.WriteLine("Renamed: " + e.OldFullPath + " to " + e.FullPath);
            //watcher.EnableRaisingEvents = true;
            //Console.WriteLine("Press 'q' to quit the sample.");
            //while (true)
            //{
            //    Thread.Sleep(500);
            //    LoadFile(logPath + @"\Player.log");
            //}
        }
    }
}
