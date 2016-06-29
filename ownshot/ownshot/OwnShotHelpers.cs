using System;
using System.Drawing;
using System.IO;
using System.Net;
using System.Windows.Forms;

namespace ownshot
{
    class OwnShotHelpers
    {
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

        public static string UploadImage(string ImagePath)
        {
            try
            {
                var request = (FtpWebRequest)WebRequest.Create(File.ReadAllText("ftpdir.txt").Replace("imagename", ImagePath));
                request.Method = WebRequestMethods.Ftp.UploadFile;

                request.Credentials = new NetworkCredential(File.ReadAllText("ftpuser.txt"), File.ReadAllText("ftppass.txt"));

                var fileContents = File.ReadAllBytes(ImagePath);
                request.ContentLength = fileContents.Length;

                var requestStream = request.GetRequestStream();
                requestStream.Write(fileContents, 0, fileContents.Length);
                requestStream.Close();

                var link = File.ReadAllText("serverlink.txt").Replace("imagename", ImagePath);

                var response = (FtpWebResponse)request.GetResponse();
                System.Windows.Clipboard.SetDataObject(link);

                requestStream.Dispose();
                response.Close();

                return link;
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
                return "ERROR: " + ex.ToString();
            }
        }

        public static bool saveToClipboard = false;

        public static void CaptureImage(bool showCursor, string FilePath)
        {
            var SelectionRectangle = Screen.GetBounds(Point.Empty);

            using (Bitmap bitmap = new Bitmap(SelectionRectangle.Width, SelectionRectangle.Height))
            {
                using (Graphics g = Graphics.FromImage(bitmap))
                {
                    g.CopyFromScreen(Point.Empty, Point.Empty, SelectionRectangle.Size);
                    if (showCursor)
                    {
                        var cursorBounds = new Rectangle(Cursor.Position, Cursor.Current.Size);
                        Cursors.Default.Draw(g, cursorBounds);
                    }
                }
                bitmap.Save(FilePath, System.Drawing.Imaging.ImageFormat.Png);
            }
        }
    }
}
