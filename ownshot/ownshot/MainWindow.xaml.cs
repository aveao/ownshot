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
        NotifyIcon notifyIcon1;
        public static int ssnamelength = 4; 
        public MainWindow()
        {
            InitializeComponent();
            this.WindowStyle = WindowStyle.None;

            var _hotKey = new Hotkey(Key.D4, KeyModifier.Shift | KeyModifier.Ctrl, hkhandler);
            var _hotKey2 = new Hotkey(Key.D6, KeyModifier.Shift | KeyModifier.Ctrl, hkhandler);
            notifyIcon1 = new NotifyIcon
            {
                Icon = new Icon(System.IO.Path.GetFileName(@"image.ico")),
                Text = "OwnShot",
                Visible = true,
                BalloonTipText = "If you see this text, then I forgot to implement some code, sorry.",
                BalloonTipTitle = "Screenshot get!"
            };
            notifyIcon1.BalloonTipClicked += NotifyIcon1_BalloonTipClicked;
        }

        public void ShowBalloon(string text)
        {
            notifyIcon1.BalloonTipText = text;
            notifyIcon1.ShowBalloonTip(1000);
        }

        public void NotifyIcon1_BalloonTipClicked(object sender, EventArgs e)
        {
            if (!notifyIcon1.BalloonTipText.StartsWith("ERROR:"))
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

            var ScreenPath = OwnShotHelpers.RandomName(ssnamelength) + ".png";
            this.WindowState = WindowState.Minimized;
            System.Threading.Thread.Sleep(250);

            OwnShotHelpers.CaptureImage(showCursor, curSize, curPos, ScreenPath);

            ShowBalloon(OwnShotHelpers.UploadImage(ScreenPath));
            
            this.WindowState = WindowState.Normal;
        }
    }
}
