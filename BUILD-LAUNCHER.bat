@echo off
title MNG Launcher - Build
cd /d "%~dp0"

echo ========================================
echo       MNG LAUNCHER - COMPILATION
echo ========================================
echo.

where dotnet >nul 2>&1
if %errorlevel% neq 0 (
    echo ERREUR : .NET SDK n'est pas installe.
    echo Installez .NET 8 SDK puis relancez ce fichier.
    pause
    exit /b 1
)

echo [1/3] Restauration des dependances...
dotnet restore
if %errorlevel% neq 0 goto ERROR

echo.
echo [2/3] Compilation du MNG Launcher...
dotnet publish MNGLauncher.csproj -c Release -r win-x64 --self-contained true -p:PublishSingleFile=true

if %errorlevel% neq 0 goto ERROR

echo.
echo [3/3] Termine !
echo.
echo ========================================
echo          COMPILATION REUSSIE
echo ========================================
echo.
echo L'EXE se trouve dans :
echo bin\Release\net8.0-windows\win-x64\publish\
echo.
explorer "bin\Release\net8.0-windows\win-x64\publish"
pause
exit /b 0

:ERROR
echo.
echo ========================================
echo          ERREUR DE COMPILATION
echo ========================================
echo.
pause
exit /b 1