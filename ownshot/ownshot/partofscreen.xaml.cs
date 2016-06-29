using System;
using System.Drawing.Imaging;
using System.IO;
using System.Net;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Forms;
using System.Windows.Input;
using System.Windows.Media.Imaging;

namespace ownshot
{
    /// <summary>
    /// Interaction logic for partofscreen.xaml
    /// </summary>
    public partial class partofscreen : Window
    {
        string ScreenPath;
        MainWindow referring = null;
        string curname = "";

        public partofscreen(MainWindow referrer)
        {
            InitializeComponent();
            referring = referrer;
            screenCapture(false);
            image.Source = new BitmapImage(new Uri(Path.Combine(Environment.CurrentDirectory, ScreenPath)));

            //+ugh fml
            this.Topmost = true;
            this.Left = 0;
            this.Top = 0;

            BackPanel.Width = image.Width = this.Width = SystemParameters.PrimaryScreenWidth;
            BackPanel.Height = image.Height = this.Height = SystemParameters.PrimaryScreenHeight;

            //this.Width = SystemParameters.PrimaryScreenWidth;
            //this.Height = SystemParameters.PrimaryScreenHeight;
            //image.Width = this.Width;
            //image.Height = this.Height;
            //BackPanel.Width = this.Width;
            //BackPanel.Height = this.Height;

            selectionRectangle.MouseDown += image1_MouseLeftButtonDown;
            BackPanel.MouseDown += image1_MouseLeftButtonDown;
            image.MouseDown += image1_MouseLeftButtonDown;
            //-ugh fml
        }

        #region "Mouse events"
        private Point anchorPoint;

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
        }

        private void image1_MouseMove(object sender, System.Windows.Input.MouseEventArgs e)
        {
            double x = e.GetPosition(BackPanel).X;
            double y = e.GetPosition(BackPanel).Y;
            selectionRectangle.SetValue(Canvas.LeftProperty, Math.Min(x, anchorPoint.X));
            selectionRectangle.SetValue(Canvas.TopProperty, Math.Min(y, anchorPoint.Y));
            selectionRectangle.Width = Math.Abs(x - anchorPoint.X);
            selectionRectangle.Height = Math.Abs(y - anchorPoint.Y);

            if (selectionRectangle.Visibility != Visibility.Visible)
                selectionRectangle.Visibility = Visibility.Visible;
        }

        private void image1_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            selectionRectangle.MouseUp -= image1_MouseLeftButtonUp;
            selectionRectangle.MouseDown -= image1_MouseLeftButtonDown;
            selectionRectangle.MouseMove -= image1_MouseMove;
            BackPanel.MouseUp -= image1_MouseLeftButtonUp;
            BackPanel.MouseDown -= image1_MouseLeftButtonDown;
            BackPanel.MouseMove -= image1_MouseMove;
            image.MouseUp -= image1_MouseLeftButtonUp;
            image.MouseDown -= image1_MouseLeftButtonDown;
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

                System.Drawing.Bitmap bitmap;
                using (MemoryStream outStream = new MemoryStream())
                {
                    BitmapEncoder enc = new BmpBitmapEncoder();
                    enc.Frames.Add(BitmapFrame.Create(bs));
                    enc.Save(outStream);
                    bitmap = new System.Drawing.Bitmap(outStream);
                    ScreenPath = ScreenPath.Replace("_pre", "");
                    bitmap.Save(ScreenPath, ImageFormat.Png);
                }

                referring.ShowBalloon(OwnShotHelpers.UploadImage(ScreenPath));
                this.Close();
                selectionRectangle.Width = 0; //workaround
            }
            selectionRectangle.Visibility = Visibility.Visible;
        }


        #endregion

        public void screenCapture(bool showCursor)
        {
            var curPos = new System.Drawing.Point(System.Windows.Forms.Cursor.Position.X, System.Windows.Forms.Cursor.Position.Y);
            var curSize = new System.Drawing.Size
            {
                Height = System.Windows.Forms.Cursor.Current.Size.Height,
                Width = System.Windows.Forms.Cursor.Current.Size.Width
            };

            var finalString = OwnShotHelpers.RandomName(MainWindow.ssnamelength) + "_pre";

            ScreenPath = finalString + ".png";
            curname = finalString;

            this.WindowState = WindowState.Minimized;

            System.Threading.Thread.Sleep(250);
            
            OwnShotHelpers.CaptureImage(showCursor, curSize, curPos, ScreenPath);

            this.WindowState = WindowState.Normal;
        }

        private void button_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }
    }
}
