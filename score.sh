#!/bin/sh
count=1
fuser -k 8000/tcp
cd FastBite
dotnet run > server.txt 2>&1 &
pid=$!
while [ "$(cat server.txt | grep 'Application started. Press Ctrl+C to shut down.')" == "" ] && [ $count -lt 40 ]
do
sleep 1
count=$((count + 1))
done
cd ..
cd FastBite.Tests 
rm -rf reports 
dotnet build
dotnet test --logger xunit --results-directory ./reports/
#JAVA_HOME=/usr/lib/jvm/java-1.8.0-openjdk-amd64/jre/
