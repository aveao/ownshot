using System;
using System.Drawing;
using System.Windows.Forms;
using System.Net;
using System.IO;

namespace ownshotLin
{
	class MainClass
	{
		static int ssnamelength = 4;
		public static void Main(string[] args)
		{
			var configpath = Path.GetDirectoryName(Application.ExecutablePath) + "/";
			var chars = "abcdefghijklmnopqrstuvwxyz0123456789"; //ABCDEFGHIJKLMNOPQRSTUVWXYZ
			var stringChars = new char[ssnamelength];
			var random = new Random();
			for (int i = 0; i < stringChars.Length; i++)
			{
				stringChars[i] = chars[random.Next(chars.Length)];
			}
			var finalString = new string(stringChars) + ".png";
			TakeScreenshot(configpath + finalString);
			Console.WriteLine(finalString);

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

				requestStream.Dispose();
				response.Close();

			}
			catch
			{
				var proc = new System.Diagnostics.Process();
				proc.StartInfo.FileName = "/usr/bin/notify-send";
				proc.StartInfo.Arguments = ("'Error while uploading!' 'Please try again!' --icon=dialog-error");
				proc.StartInfo.UseShellExecute = false; 
				proc.StartInfo.RedirectStandardOutput = false;
				proc.Start();
			}
		}

		public static void TakeScreenshot(string filePath)
		{
			using (Bitmap bmpScreenCapture = new Bitmap(Screen.PrimaryScreen.Bounds.Width,
				                                 Screen.PrimaryScreen.Bounds.Height)) {
				using (Graphics g = Graphics.FromImage(bmpScreenCapture)) {
					g.CopyFromScreen(Screen.PrimaryScreen.Bounds.X,
						Screen.PrimaryScreen.Bounds.Y,
						0, 0,
						bmpScreenCapture.Size,
						CopyPixelOperation.SourceCopy);
				}
				bmpScreenCapture.Save(filePath, System.Drawing.Imaging.ImageFormat.Png);
			}
		}
	}
}
