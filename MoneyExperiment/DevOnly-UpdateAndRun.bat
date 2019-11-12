@echo off
echo This is used only for the developing of the app.
git pull
dotnet build
cd bin
cd debug
cd netcoreapp3.1
MoneyExperiment.exe