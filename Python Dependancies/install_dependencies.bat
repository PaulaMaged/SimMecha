@echo off
echo Changing directory to the script location...
cd /d "%~dp0"

echo Installing dependencies...
pip install -r requirements.txt
pause
