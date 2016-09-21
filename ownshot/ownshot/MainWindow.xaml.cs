using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media.Imaging;
using System.Windows.Forms;
using System.Drawing;
using System.IO;

namespace ownshot
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        NotifyIcon notifyIcon1;
        public static int ssnamelength = 4;
        string ScreenPath;
        public MainWindow()
        {
            InitializeComponent();
            this.AllowsTransparency = true;
            SetWindowStyle(true);
            new Hotkey(Key.D4, KeyModifier.Shift | KeyModifier.Ctrl, hkhandler);
            new Hotkey(Key.D5, KeyModifier.Shift | KeyModifier.Ctrl, hkhandler);
            new Hotkey(Key.D6, KeyModifier.Shift | KeyModifier.Ctrl, hkhandler);
            notifyIcon1 = new NotifyIcon
            {
                Icon = new Icon(Path.GetFileName(@"image.ico")),
                Text = "OwnShot",
                Visible = true,
                BalloonTipText = "If you see this text, then I forgot to implement some code, sorry.",
                BalloonTipTitle = "Screenshot get!"
            };
            notifyIcon1.BalloonTipClicked += NotifyIcon1_BalloonTipClicked;
        }

        void SetWindowStyle(bool hidden)
        {
            this.Topmost = !hidden;
            this.ResizeMode = ResizeMode.NoResize;
            this.WindowStyle = WindowStyle.None;
            this.ShowInTaskbar = !hidden;
            this.Visibility = (hidden) ? Visibility.Hidden : Visibility.Visible;
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
            else if (hotKey.Key == Key.D5)
            {
                WindowCapture();
            }
            else if (hotKey.Key == Key.D6)
            {
                PartOfScreen();
            }
        }

        public void WindowCapture()
        {
            ScreenPath = OwnShotHelpers.RandomName(ssnamelength) + ".png";

            SetWindowStyle(true);
            System.Threading.Thread.Sleep(25);

            OwnShotHelpers.WindowCapture(ScreenPath);

            ShowBalloon(OwnShotHelpers.UploadImage(ScreenPath));
        }

        public void screenCapture(bool showCursor, bool Partial = false)
        {
            ScreenPath = OwnShotHelpers.RandomName(ssnamelength) + (Partial ? "_pre" : "") + ".png";

            SetWindowStyle(true);
            System.Threading.Thread.Sleep(25);

            OwnShotHelpers.CaptureImage(showCursor, ScreenPath);

            if (!Partial) ShowBalloon(OwnShotHelpers.UploadImage(ScreenPath));
            
            SetWindowStyle(!Partial);
        }

        void PartOfScreen()
        {
            screenCapture(false, true);
            image.Source = new BitmapImage(new Uri(Path.Combine(Environment.CurrentDirectory, ScreenPath)));

            //+ugh fml
            SetWindowStyle(false);
            this.Left = SystemParameters.VirtualScreenLeft;
            this.Top = SystemParameters.VirtualScreenTop;

            var buttonPos = new System.Windows.Media.TranslateTransform();
            buttonPos.X = (0 - SystemParameters.VirtualScreenLeft) + 15;
            buttonPos.Y = (0 - SystemParameters.VirtualScreenTop) + 15;

            button.RenderTransform = buttonPos;

            BackPanel.Width = image.Width = this.Width = SystemParameters.VirtualScreenWidth;
            BackPanel.Height = image.Height = this.Height = SystemParameters.VirtualScreenHeight;

            selectionRectangle.MouseDown += image1_MouseLeftButtonDown;
            BackPanel.MouseDown += image1_MouseLeftButtonDown;
            image.MouseDown += image1_MouseLeftButtonDown;
            //-ugh fml
        }

        #region "Mouse events"
        private System.Windows.Point anchorPoint;

        private void image1_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            anchorPoint.X = e.GetPosition(BackPanel).X;
            anchorPoint.Y = e.GetPosition(BackPanel).Y;

            image.MouseUp += image1_MouseLeftButtonUp;
            BackPanel.MouseUp += image1_MouseLeftButtonUp;
            selectionRectangle.MouseUp += image1_MouseLeftButtonUp;
            image.MouseMove += image1_MouseMove;
            BackPanel.MouseMove += image1_MouseMove;
            selectionRectangle.MouseMove += image1_MouseMove;
            BackPanel.MouseDown -= image1_MouseLeftButtonDown;
            image.MouseDown -= image1_MouseLeftButtonDown;
            selectionRectangle.MouseDown -= image1_MouseLeftButtonDown;
        }

        private void image1_MouseMove(object sender, System.Windows.Input.MouseEventArgs e)
        {
            double x = e.GetPosition(BackPanel).X;
            double y = e.GetPosition(BackPanel).Y;
            selectionRectangle.SetValue(Canvas.LeftProperty, Math.Min(x, anchorPoint.X));
            selectionRectangle.SetValue(Canvas.TopProperty, Math.Min(y, anchorPoint.Y));
            selectionRectangle.Width = Math.Abs(x - anchorPoint.X);
            selectionRectangle.Height = Math.Abs(y - anchorPoint.Y);

            selectionRectangle.Visibility = Visibility.Visible;
        }

        private void image1_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            selectionRectangle.MouseUp -= image1_MouseLeftButtonUp;
            selectionRectangle.MouseMove -= image1_MouseMove;
            BackPanel.MouseUp -= image1_MouseLeftButtonUp;
            BackPanel.MouseMove -= image1_MouseMove;
            image.MouseUp -= image1_MouseLeftButtonUp;
            image.MouseMove -= image1_MouseMove;

            if (selectionRectangle.Width > 0)
            {
                this.Hide();
                Rect rect1 = new Rect(Canvas.GetLeft(selectionRectangle), Canvas.GetTop(selectionRectangle), selectionRectangle.Width, selectionRectangle.Height);
                Int32Rect rcFrom = new Int32Rect();
                rcFrom.X = (int)((rect1.X) * (image.Source.Width) / (image.Width));
                rcFrom.Y = (int)((rect1.Y) * (image.Source.Height) / (image.Height));
                rcFrom.Width = (int)((rect1.Width) * (image.Source.Width) / (image.Width));
                rcFrom.Height = (int)((rect1.Height) * (image.Source.Height) / (image.Height));
                BitmapSource bs = new CroppedBitmap(image.Source as BitmapSource, rcFrom);

                using (MemoryStream outStream = new MemoryStream())
                {
                    BitmapEncoder enc = new BmpBitmapEncoder();
                    enc.Frames.Add(BitmapFrame.Create(bs));
                    enc.Save(outStream);
                    ScreenPath = ScreenPath.Replace("_pre", "");
                    new Bitmap(outStream).Save(ScreenPath, System.Drawing.Imaging.ImageFormat.Png);
                }

                ShowBalloon(OwnShotHelpers.UploadImage(ScreenPath));
                SetWindowStyle(true);
                selectionRectangle.Width = 0; //workaround
            }
            selectionRectangle.Visibility = Visibility.Visible;
        }


        #endregion
        private void button_Click(object sender, RoutedEventArgs e)
        {
            SetWindowStyle(true);
            selectionRectangle.MouseUp -= image1_MouseLeftButtonUp;
            selectionRectangle.MouseMove -= image1_MouseMove;
            BackPanel.MouseUp -= image1_MouseLeftButtonUp;
            BackPanel.MouseMove -= image1_MouseMove;
            image.MouseUp -= image1_MouseLeftButtonUp;
            image.MouseMove -= image1_MouseMove;
            selectionRectangle.Width = 0; //workaround
        }
    }
}