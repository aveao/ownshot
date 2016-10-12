using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Net;
using System.Runtime.InteropServices;
using System.Windows.Forms;

namespace ownshot
{
    class OwnShotHelpers
    {

        [DllImport("user32.dll")]
        private static extern IntPtr GetForegroundWindow();

        [DllImport("user32.dll")]
        private static extern IntPtr GetWindowRect(IntPtr hWnd, ref Rect rect);

        [StructLayout(LayoutKind.Sequential)]
        private struct Rect
        {
            public int Left;
            public int Top;
            public int Right;
            public int Bottom;
        }

        public static void WindowCapture(string FilePath)
        {
            var foregroundWindowsHandle = GetForegroundWindow();
            var rect = new Rect();
            GetWindowRect(foregroundWindowsHandle, ref rect);
            var bounds = new Rectangle(rect.Left, rect.Top, rect.Right - rect.Left, rect.Bottom - rect.Top);

            var result = new Bitmap(bounds.Width, bounds.Height);

            using (var g = Graphics.FromImage(result))
            {
                g.CopyFromScreen(new Point(bounds.Left, bounds.Top), Point.Empty, bounds.Size);
            }
            result.Save(FilePath, ImageFormat.Png);
        }

        public static string RandomName(int length)
        {
            var chars = "abcdefghijklmnopqrstuvwxyz0123456789"; //ABCDEFGHIJKLMNOPQRSTUVWXYZ
            var stringChars = new char[length];
            var random = new Random();

            for (int i = 0; i < stringChars.Length; i++)
            {
                stringChars[i] = chars[random.Next(chars.Length)];
            }
            return new string(stringChars);
        }

        public static string GetConfig(string key, string defaultvalue = "NOTFOUND")
        {
            if (!File.Exists("ownshot.ini"))
            {
                File.WriteAllText("ownshot.ini", string.Format("UploadMethod=FTP{0}FTPDirectory=ftp://iphere/imagename.png{0}ServerLink=https://serverlinkhere.com/imagename.png{0}FTPUser=FTPUsernameHere{0}FTPPassword=P455Here{0}DeleteMode=Yes", Environment.NewLine));
            }
            var config = File.ReadAllLines("ownshot.ini");
            var SearchingFor = key + "=";
            foreach (var line in config)
            {
                if (line.StartsWith(SearchingFor))
                {
                    return line.Replace(SearchingFor, "");
                }
            }

            return defaultvalue;
        }

        public static string UploadImage(string ImagePath)
        {
            var UploadMethod = GetConfig("UploadMethod", "FTP").Trim();
            if (UploadMethod.ToUpper() == "FTP")
            {
                try
                {
                    var request = (FtpWebRequest)WebRequest.Create(GetConfig("FTPDirectory").Replace(".png", "").Replace("imagename", ImagePath));
                    request.Method = WebRequestMethods.Ftp.UploadFile;

                    request.Credentials = new NetworkCredential(GetConfig("FTPUser"), GetConfig("FTPPassword"));

                    var fileContents = File.ReadAllBytes(ImagePath);
                    request.ContentLength = fileContents.Length;

                    var requestStream = request.GetRequestStream();
                    requestStream.Write(fileContents, 0, fileContents.Length);
                    requestStream.Close();
                    requestStream.Dispose();

                    var response = (FtpWebResponse)request.GetResponse();
                    response.Close();

                    var link = GetConfig("ServerLink").Replace(".png", "").Replace("imagename", ImagePath);

                    System.Windows.Clipboard.SetDataObject(link);

                    return link;
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex);
                    return "ERROR: " + ex.ToString();
                }
            }
            else
            {
                var proc = new System.Diagnostics.Process
                {
                    StartInfo = new System.Diagnostics.ProcessStartInfo
                    {
                        FileName = UploadMethod,
                        Arguments = ImagePath,
                        UseShellExecute = false,
                        RedirectStandardOutput = true,
                        CreateNoWindow = true
                    }
                };
                proc.Start();
                while (!proc.StandardOutput.EndOfStream)
                {
                    var link = proc.StandardOutput.ReadLine();
                    System.Windows.Clipboard.SetDataObject(link);
                    return link;
                }
                return "ERROR: the external uploader timed out";
            }
        }

        public static void CaptureImage(bool showCursor, string FilePath)
        {
            int screenWidth = Convert.ToInt32(System.Windows.SystemParameters.VirtualScreenWidth);
            int screenHeight = Convert.ToInt32(System.Windows.SystemParameters.VirtualScreenHeight);
            int screenLeft = Convert.ToInt32(System.Windows.SystemParameters.VirtualScreenLeft);
            int screenTop = Convert.ToInt32(System.Windows.SystemParameters.VirtualScreenTop);
            using (Bitmap bitmap = new Bitmap(screenWidth, screenHeight, PixelFormat.Format32bppArgb))
            {
                using (Graphics gr = Graphics.FromImage(bitmap))
                {
                    gr.CopyFromScreen(screenLeft, screenTop, 0, 0, bitmap.Size);
                    if (showCursor)
                    {
                        var cursorBounds = new Rectangle(Cursor.Position, Cursor.Current.Size);
                        Cursors.Default.Draw(gr, cursorBounds);
                    }
                    bitmap.Save(FilePath);
                }
            }

            if (OwnShotHelpers.GetConfig("OptimizeImage", "Yes") == "Yes" && File.Exists("optipng.exe"))
            {
                var proc = new System.Diagnostics.Process
                {
                    StartInfo = new System.Diagnostics.ProcessStartInfo
                    {
                        FileName = "optipng.exe",
                        Arguments = FilePath,
                        UseShellExecute = false,
                        RedirectStandardOutput = false,
                        CreateNoWindow = true
                    }
                };
                proc.Start();
                proc.WaitForExit();
            }
        }
    }
}
