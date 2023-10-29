for /r %%i in (*.png *.jpg) do magick convert "%%i" -define jpeg:extent=1024KB "%%~dpni.jpeg"
