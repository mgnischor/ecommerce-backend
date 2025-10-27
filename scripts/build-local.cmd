@echo off

cd ..

dotnet build -c Release
dotnet build -c Debug

dotnet publish -c Release
dotnet publish -c Debug
