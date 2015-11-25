using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Interop;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Windows.Forms;
using System.Drawing;
using System.IO;
using System.Net;
using System.Windows.Threading;

namespace ownshot
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        string ScreenPath;
        NotifyIcon notifyIcon1 = new NotifyIcon();
        public MainWindow()
        {
            InitializeComponent();
            this.WindowStyle = WindowStyle.None;
            var titlecheck = new DispatcherTimer();
            titlecheck.Interval = new TimeSpan(0,0,0,0,100);
            titlecheck.Tick += Titlecheck_Tick;
            titlecheck.IsEnabled = true;

            var _hotKey = new Hotkey(Key.D4, KeyModifier.Shift | KeyModifier.Ctrl, hkhandler);
            var _hotKey2 = new Hotkey(Key.D6, KeyModifier.Shift | KeyModifier.Ctrl, hkhandler);
            
            notifyIcon1.Icon = new Icon(System.IO.Path.GetFileName(@"image.ico"));
            notifyIcon1.Text = "OwnShot";
            notifyIcon1.Visible = true;
            notifyIcon1.BalloonTipText = "If you see this, then I forgot to implement a code, sorry.";
            notifyIcon1.BalloonTipTitle = "Screenshot get!";
            notifyIcon1.BalloonTipClicked += NotifyIcon1_BalloonTipClicked;
        }

        private void Titlecheck_Tick(object sender, EventArgs e)
        {
            if (this.Title != "OwnShot")
            {
                notifyIcon1.BalloonTipText = this.Title;
                notifyIcon1.ShowBalloonTip(1000);
                this.Title = "OwnShot";
            }
        }

        public void NotifyIcon1_BalloonTipClicked(object sender, EventArgs e)
        {
            System.Diagnostics.Process.Start(notifyIcon1.BalloonTipText);
        }

        private void hkhandler(Hotkey hotKey)
        {
            if (hotKey.Key == Key.D4)
            {
                screenCapture(true);
            }
            else if (hotKey.Key == Key.D6)
            {
                var pos = new partofscreen(this);
                pos.Show();
                pos.Activate();
            }
        }

        public void screenCapture(bool showCursor)
        {
            var curPos = new System.Drawing.Point(System.Windows.Forms.Cursor.Position.X, System.Windows.Forms.Cursor.Position.Y);
            var curSize = new System.Drawing.Size
            {
                Height = System.Windows.Forms.Cursor.Current.Size.Height,
                Width = System.Windows.Forms.Cursor.Current.Size.Width
            };
            var chars = "abcdefghijklmnopqrstuvwxyz0123456789"; //ABCDEFGHIJKLMNOPQRSTUVWXYZ
            var stringChars = new char[3];
            var random = new Random();
            for (int i = 0; i < stringChars.Length; i++)
            {
                stringChars[i] = chars[random.Next(chars.Length)];
            }
            var finalString = new String(stringChars);
            ScreenPath = Directory.GetCurrentDirectory() + "\\" + new string(stringChars) + ".png";
            this.WindowState = WindowState.Minimized;
            System.Threading.Thread.Sleep(250);

            var bounds = Screen.GetBounds(Screen.GetBounds(System.Drawing.Point.Empty));
            var fi = "";

            if (ScreenPath != "")
            {
                fi = new FileInfo(ScreenPath).Extension;
            }
            ScreenShot.CaptureImage(showCursor, curSize, curPos, System.Drawing.Point.Empty, System.Drawing.Point.Empty, bounds, ScreenPath, fi);
            try
            {
                var request = (FtpWebRequest)WebRequest.Create("ftp://ardao.me/%2F/var/www/ardaome/public_html/files/" + new string(stringChars) + ".png");
                request.Method = WebRequestMethods.Ftp.UploadFile;

                request.Credentials = new NetworkCredential("ardaoftp", File.ReadAllText("C:\\ftppass.txt"));

                var fileContents = File.ReadAllBytes(ScreenPath);
                request.ContentLength = fileContents.Length;

                var requestStream = request.GetRequestStream();
                requestStream.Write(fileContents, 0, fileContents.Length);
                requestStream.Close();

                var link = "http://ardao.me/files/" + new string(stringChars) + ".png";

                var response = (FtpWebResponse)request.GetResponse();
                notifyIcon1.BalloonTipText = link;
                notifyIcon1.ShowBalloonTip(1000);

                Console.WriteLine("Upload File Complete, status {0}", response.StatusDescription);
                System.Windows.Clipboard.SetText(link);

                requestStream.Dispose();
                response.Close();

            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
                notifyIcon1.BalloonTipText = "Upload failed. " + ex;
                notifyIcon1.ShowBalloonTip(1000);
            }

            this.WindowState = WindowState.Normal;
        }
    }
}
