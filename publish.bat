@echo off
setlocal enabledelayedexpansion

for %%f in (*.version) do set VERSION=%%~nf
set PROJECT=CashFlowBot\CashFlowBot.csproj
set CONFIG=Release
set PUBLISH=publish
set RIDS=win-x86 win-x64 linux-x64 osx-x64

if exist %PUBLISH% rmdir /s /q %PUBLISH%
mkdir %PUBLISH%

for %%R in (%RIDS%) do (
	echo ============================
	echo Building for %%R
	echo ============================

	set PUBLISH_DIR=%PUBLISH%\%%R

	dotnet publish %PROJECT% -c %CONFIG% -r %%R --self-contained true ^
		-p:PublishSingleFile=true ^
		-p:IncludeNativeLibrariesForSelfExtract=true ^
		-o !PUBLISH_DIR!

	if exist !PUBLISH_DIR!\BotID.txt (
		echo. > !PUBLISH_DIR!\BotID.txt
	)

	set ZIP_NAME=CashFlow%VERSION%-%%R.zip

	powershell -Command "Compress-Archive -Path '!PUBLISH_DIR!\*' -DestinationPath '%CD%\!ZIP_NAME!' -Force"

	echo Created !ZIP_NAME!
)

rmdir /s /q %PUBLISH%
rmdir /s /q CashFlow\%PUBLISH%
rmdir /s /q CashFlowBot\%PUBLISH%

echo ============================
echo DONE
echo ============================

pause
