# ownshot
Wanna setup your own LightShot, puush or gyazo? Here you go!

###How can I setup this?
First of all, create a FTP user in your server.
Then in [this](https://github.com/ardaozkal/ownshot/blob/master/ownshot/ownshot/MainWindow.xaml.cs) and [this](https://github.com/ardaozkal/ownshot/blob/master/ownshot/ownshot/partofscreen.xaml.cs) files:

On the "var request = (FtpWebRequest)WebRequest.Create("ftp://ardao.me/%2F/var/www/ardaome/public_html/files/" + new string(stringChars) + ".png");" line, change the link to your server. %2F changes directory to /, change the rest of the link to where you want it to go (my publichtml folder is at /var/www/ardaome/publichtml, and I put the screenshots to the files folder in it.)

On the "request.Credentials = new NetworkCredential("ardaoftp", File.ReadAllText("C:\\ftppass.txt"));" line, change ardaoftp with your FTP username and create a file in C, with the name "ftppass.txt", and put your password in it.

On the "var link = "http://ardao.me/files/" + new string(stringChars) + ".png";" line, change the http://ardao.me/files/ part to your own server's link and folder.

###How can I use this? 

Ctrl+Shift+4 to get a fullscreen screenshot, Ctrl+Shift+6 to crop a part of screen.
