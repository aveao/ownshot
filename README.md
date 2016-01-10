# ownshot
Wanna setup your own LightShot, puush or gyazo? Here you go!

###How can I use this?
First of all, create a FTP user in your server.

Compile the code.

On the running directory, create four files called `serverlink.txt, ftpuser.txt, ftppass.txt and ftpdir.txt`.

On `serverlink.txt` file, write the link it'll copy to copyboard. Replace image name with `imagename`. Ex. `http://ardao.me/files/imagename.png`

On `ftpuser.txt` file, write the FTP username. Ex. `screenshotftpuser`.

On `ftppass.txt` file, write the FTP password. Ex. `correcthorsebatterystaple`.

On `ftpdir.txt` file, write the FTP link and directory. `%2F` returns you to / directory. Replace image name with `imagename`. Ex. `ftp://ardao.me/%2F/var/www/ardaome/public_html/files/imagename.png`

Run the `ownshot.exe` file.

If you want to run it at startup, open the startup folder (Run (`Win+R`): `shell:startup`) and copy a **shortcut** of the `ownshot.exe` file to the startup directory. Be sure to use a shortcut as ownshot keeps a copy of your screenshots at the running directory, if you fail to do it, you might be welcomed by hundreds of pictures at your next restart.

###How can I change the length of the screenshot name?

On the `MainWindow.xaml.cs` file, there is a line like this:

`public static int ssnamelength = 4;`

Eeplace 4 with the length you want it to be.

###How can I use this? 

Ctrl+Shift+4 to get a fullscreen screenshot, Ctrl+Shift+6 to crop a part of screen.
