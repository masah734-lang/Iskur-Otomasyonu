@echo off
setlocal
chcp 65001 >nul
title ISKUR - Frontend
pushd "%~dp0frontend"
if errorlevel 1 goto :folder_error
where node >nul 2>&1
if errorlevel 1 (
 echo HATA: Node.js bulunamadi. Node.js 22.12 veya uzerini kurun.
 goto :error
)
where npm >nul 2>&1
if errorlevel 1 (
 echo HATA: npm bulunamadi. Node.js kurulumunu kontrol edin.
 goto :error
)
if not exist "package.json" (
 echo HATA: frontend\package.json bulunamadi.
 goto :error
)
if exist "node_modules\vite\bin\vite.js" goto :run
echo Ilk kullanim: React bagimliliklari kuruluyor. Internet gereklidir.
call npm ci
if errorlevel 1 goto :error

:run
echo.
echo Arayuz baslatiliyor. Tarayici otomatik acilacak.
echo Once Backend-Baslat.bat dosyasi calisiyor olmali.
echo Bu pencere acik kalsin. Durdurmak icin Ctrl+C kullanin.
call npm run dev -- --open --strictPort
if errorlevel 1 goto :error
popd
exit /b 0

:error
echo.
echo Islem tamamlanamadi. Yukaridaki hata mesajini kontrol edin.
pause
popd
exit /b 1

:folder_error
echo HATA: frontend klasoru bulunamadi.
echo Bu dosya IskurVize ana klasorunde olmali.
pause
exit /b 1
