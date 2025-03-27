echo copying build...
mkdir -p linux-dir/usr/bin
cp -r ../matritsa/matritsa.Desktop/bin/Release-linux/net6.0/publish/linux-x64/* linux-dir/usr/bin/

echo downloading appimagetool...
wget "https://github.com/AppImage/AppImageKit/releases/download/continuous/appimagetool-x86_64.AppImage"
chmod a+x appimagetool-x86_64.AppImage

echo packaging matritsa...
sudo apt install -y libfuse2
ARCH=x86_64 ./appimagetool-x86_64.AppImage ./linux-dir