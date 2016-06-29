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
            SetWindowStyle(false);
            new Hotkey(Key.D4, KeyModifier.Shift | KeyModifier.Ctrl, hkhandler);
            new Hotkey(Key.D6, KeyModifier.Shift | KeyModifier.Ctrl, hkhandler);
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

        void SetWindowStyle(bool hidden)
        {
            this.ResizeMode = ResizeMode.NoResize;
            this.WindowStyle = WindowStyle.None;
            this.ShowInTaskbar = !hidden;
            this.Visibility = (hidden) ? Visibility.Hidden : Visibility.Visible;
            this.AllowsTransparency = hidden;
            this.Background = (hidden) ? System.Windows.Media.Brushes.Transparent : System.Windows.Media.Brushes.LightGray;
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
            var ScreenPath = OwnShotHelpers.RandomName(ssnamelength) + ".png";
            this.WindowState = WindowState.Minimized;
            System.Threading.Thread.Sleep(250);

            OwnShotHelpers.CaptureImage(showCursor, ScreenPath);

            ShowBalloon(OwnShotHelpers.UploadImage(ScreenPath));
            
            this.WindowState = WindowState.Normal;
        }
    }
}
