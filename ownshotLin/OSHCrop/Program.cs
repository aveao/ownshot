using System;
using Gtk;
using Gdk;
using System.Drawing;
using System.IO;
using System.Net;

namespace OSHCrop
{
	class MainClass
	{
		static MainWindow win;
		static double downx = 0;
		static double downy = 0;
		static double upx = 0;
		static double upy = 0;
		static int ssnamelength = 4;
		static string finalString = "";
		static Gtk.Image img;
		static string configpath;

		public static void Main(string[] args)
		{
			configpath = System.IO.Path.GetDirectoryName(System.Windows.Forms.Application.ExecutablePath) + "/";
			Application.Init();
			win = new MainWindow();
			var eventbox = new EventBox();

			//win.ExposeEvent += OnExposed;

			var chars = "abcdefghijklmnopqrstuvwxyz0123456789"; //ABCDEFGHIJKLMNOPQRSTUVWXYZ
			var stringChars = new char[ssnamelength];
			var random = new Random();
			for (int i = 0; i < stringChars.Length; i++)
			{
				stringChars[i] = chars[random.Next(chars.Length)];
			}
			finalString = new string(stringChars) + ".png";

			win.Hide();
			TakeScreenshot(configpath + finalString.Replace(".png", "_pre.png"));
			win.Show();
			win.Decorated = false;

			img = new Gtk.Image(configpath + finalString.Replace(".png", "_pre.png"));
			eventbox.ButtonPressEvent += (ButtonPressHandler);
			eventbox.ButtonReleaseEvent += (ButtonReleaseHandler);

			img.Xalign = 0.5f;
			img.Yalign = 0.5f;

			eventbox.Add(img);
			eventbox.ShowAll();
			win.Add(eventbox);
			win.ShowAll();

			win.Move(0, 0);

			Application.Run();
		}

		public static void ButtonPressHandler (object o, ButtonPressEventArgs args)
		{
			downx = args.Event.X; 
			downy = args.Event.Y;
			args.RetVal = true;
		}

		public static void ButtonReleaseHandler (object o, ButtonReleaseEventArgs args)
		{
			upx = args.Event.X; 
			upy = args.Event.Y; 
			completecrop(configpath + finalString); 
			args.RetVal = true;
		}

		static void completecrop(string finalString)
		{
			int startx = Convert.ToInt32((upx < downx) ? upx : downx);
			int starty = Convert.ToInt32((upy < downy) ? upy : downy);
			int sizex = Convert.ToInt32((upx > downx) ? (upx - downx) : (downx - upx));
			int sizey = Convert.ToInt32((upy > downy) ? (upy - downy) : (downy - upy));
			cropAtRect(System.Drawing.Image.FromFile(finalString.Replace(".png", "_pre.png")), new System.Drawing.Rectangle(new System.Drawing.Point(startx, starty), new System.Drawing.Size(sizex, sizey))).Save(configpath + finalString);

			win.HideAll();

			try
			{
				var request = (FtpWebRequest)WebRequest.Create(File.ReadAllText(configpath + "ftpdir.txt").Replace("imagename", finalString));
				request.Method = WebRequestMethods.Ftp.UploadFile;

				request.Credentials = new NetworkCredential(File.ReadAllText(configpath + "ftpuser.txt"), File.ReadAllText(configpath + "ftppass.txt"));

				var fileContents = File.ReadAllBytes(configpath + finalString);
				request.ContentLength = fileContents.Length;

				var requestStream = request.GetRequestStream();
				requestStream.Write(fileContents, 0, fileContents.Length);
				requestStream.Close();

				var link = File.ReadAllText(configpath + "serverlink.txt").Replace("imagename", finalString);
				var response = (FtpWebResponse)request.GetResponse();

				var proc = new System.Diagnostics.Process();
				proc.StartInfo.FileName = "/usr/bin/notify-send";
				proc.StartInfo.Arguments = ("'Screenshot Get!' '" + link + "' --icon=dialog-information");
				proc.StartInfo.UseShellExecute = false; 
				proc.StartInfo.RedirectStandardOutput = false;
				proc.Start();

				Gtk.Clipboard clipboard = Gtk.Clipboard.Get(Gdk.Atom.Intern("CLIPBOARD", false));

				clipboard.Text = link;

				requestStream.Dispose();
				response.Close();

			}
			catch (Exception ex)
			{
				File.WriteAllText(configpath + "log.log", ex.Message);
				var proc = new System.Diagnostics.Process();
				proc.StartInfo.FileName = "/usr/bin/notify-send";
				proc.StartInfo.Arguments = ("'Error while uploading!' 'Please try again!' --icon=dialog-error");
				proc.StartInfo.UseShellExecute = false; 
				proc.StartInfo.RedirectStandardOutput = false;
				proc.Start();
			}
			Application.Quit();
		}

		public static System.Drawing.Image cropAtRect(System.Drawing.Image b, System.Drawing.Rectangle r)
		{
			var nb = new Bitmap(r.Width, r.Height);
			var g = Graphics.FromImage(nb);
			g.DrawImage(b, -r.X, -r.Y);
			return nb;
		}

		public static void TakeScreenshot(string filePath)
		{
			using (Bitmap bmpScreenCapture = new Bitmap(System.Windows.Forms.Screen.PrimaryScreen.Bounds.Width,
				System.Windows.Forms.Screen.PrimaryScreen.Bounds.Height)) {
				using (Graphics g = Graphics.FromImage(bmpScreenCapture)) {
					g.CopyFromScreen(System.Windows.Forms.Screen.PrimaryScreen.Bounds.X,
						System.Windows.Forms.Screen.PrimaryScreen.Bounds.Y,
						0, 0,
						bmpScreenCapture.Size,
						CopyPixelOperation.SourceCopy);
				}
				bmpScreenCapture.Save(filePath, System.Drawing.Imaging.ImageFormat.Png);
			}
		}
	}
}
