wget https://packages.microsoft.com/config/ubuntu/19.10/packages-microsoft-prod.deb -O packages-microsoft-prod.deb
sudo dpkg -i packages-microsoft-prod.deb
sudo apt-get update
sudo apt-get install apt-transport-https
sudo apt-get install psmisc
sudo apt-get update
sudo apt-get install dotnet-sdk-3.1 -y
cd FastBite.Tests
rm -rf bin
dotnet build
cd