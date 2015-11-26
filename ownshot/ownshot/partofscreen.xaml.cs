using System;
using System.Collections.Generic;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Net;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Forms;
using System.Windows.Input;
using System.Windows.Interop;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

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
            this.Topmost = true;
            referring = referrer;
            this.Left = 0;
            this.Top = 0;
            this.Width = SystemParameters.PrimaryScreenWidth;
            this.Height = SystemParameters.PrimaryScreenHeight;
            image.Width = this.Width;
            image.Height = this.Height;
            BackPanel.Width = this.Width;
            BackPanel.Height = this.Height;
            screenCapture(false);
            image.Source = new BitmapImage(new Uri(ScreenPath));

            selectionRectangle.MouseUp += image1_MouseLeftButtonUp;
            selectionRectangle.MouseDown += image1_MouseLeftButtonDown;
            selectionRectangle.MouseMove += image1_MouseMove;
            BackPanel.MouseUp += image1_MouseLeftButtonUp;
            BackPanel.MouseDown += image1_MouseLeftButtonDown;
            BackPanel.MouseMove += image1_MouseMove;
            image.MouseUp += image1_MouseLeftButtonUp;
            image.MouseDown += image1_MouseLeftButtonDown;
            image.MouseMove += image1_MouseMove;
        }

        #region "Mouse events"
        bool isDragging = false;
        private Point anchorPoint;

        private void image1_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (isDragging == false)
            {
                anchorPoint.X = e.GetPosition(BackPanel).X;
                anchorPoint.Y = e.GetPosition(BackPanel).Y;
                isDragging = true;
            }

        }

        private void image1_MouseMove(object sender, System.Windows.Input.MouseEventArgs e)
        {
            if (isDragging)
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
        }

        private void image1_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            if (isDragging)
            {
                isDragging = false;
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

                    uppic(curname.Replace("_pre", ""));
                    this.Close();
                }
                if (selectionRectangle.Visibility != Visibility.Visible)
                    selectionRectangle.Visibility = Visibility.Visible;
            }
        }

        private void RestRect()
        {
            selectionRectangle.Visibility = Visibility.Collapsed;
            isDragging = false;
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

            var chars = "abcdefghijklmnopqrstuvwxyz0123456789"; //ABCDEFGHIJKLMNOPQRSTUVWXYZ
            var stringChars = new char[3];
            var random = new Random();

            for (int i = 0; i < stringChars.Length; i++)
            {
                stringChars[i] = chars[random.Next(chars.Length)];
            }

            var finalString = new String(stringChars) + "_pre";

            ScreenPath = Directory.GetCurrentDirectory() + "\\" + finalString + ".png";
            curname = finalString;

            this.WindowState = WindowState.Minimized;

            System.Threading.Thread.Sleep(250);

            var bounds = Screen.GetBounds(Screen.GetBounds(System.Drawing.Point.Empty));
            var fi = "";

            if (ScreenPath != "")
            {
                fi = new FileInfo(ScreenPath).Extension;
            }
            ScreenShot.CaptureImage(showCursor, curSize, curPos, System.Drawing.Point.Empty, System.Drawing.Point.Empty, bounds, ScreenPath, fi);
            
            this.WindowState = WindowState.Normal;
        }

        void uppic(string imgname)
        {
            try
            {
                var request = (FtpWebRequest)WebRequest.Create("ftp://ardao.me/%2F/var/www/ardaome/public_html/files/" + imgname + ".png");
                request.Method = WebRequestMethods.Ftp.UploadFile;

                request.Credentials = new NetworkCredential("ardaoftp", File.ReadAllText("C:\\ftppass.txt"));

                var fileContents = File.ReadAllBytes(ScreenPath);
                request.ContentLength = fileContents.Length;

                var requestStream = request.GetRequestStream();
                requestStream.Write(fileContents, 0, fileContents.Length);
                requestStream.Close();

                var link = "http://ardao.me/files/" + imgname + ".png";

                var response = (FtpWebResponse)request.GetResponse();
                referring.Title = link;

                Console.WriteLine("Upload File Complete, status {0}", response.StatusDescription);
                System.Windows.Clipboard.SetText(link);

                requestStream.Dispose();
                response.Close();

            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
                referring.Title = ex.ToString();
            }
        }
        
        private void button_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }
    }
}
