@echo off
setlocal
chcp 65001 >nul
title ISKUR - Backend
pushd "%~dp0"
if errorlevel 1 goto :folder_error
where dotnet >nul 2>&1
if errorlevel 1 (
 echo HATA: .NET 10 SDK bulunamadi. Once .NET 10 SDK kurun.
 goto :error
)
if not exist "backend\Iskur.Api\Iskur.Api.csproj" (
 echo HATA: Bu dosya IskurVize ana klasorunde olmali.
 goto :error
)
if not defined ConnectionStrings__Default set "ConnectionStrings__Default=Server=.\SQLEXPRESS;Database=IskurVize;Trusted_Connection=True;TrustServerCertificate=True"
set "ASPNETCORE_ENVIRONMENT=Development"
if exist ".iskur-kurulum-tamam" goto :run

echo Ilk kullanimda veritabani ve demo hesaplar olusturulmalidir.
echo SQL Server Express ornegi: .\SQLEXPRESS
echo Daha once Setup.ps1 calistirdiysaniz H secin.
choice /c EH /n /m "Ilk kurulumu simdi yapalim mi? [E/H]: "
if errorlevel 2 goto :run
powershell.exe -NoProfile -ExecutionPolicy Bypass -File "%~dp0scripts\Setup.ps1" -ConnectionString "%ConnectionStrings__Default%"
if errorlevel 1 goto :error
>".iskur-kurulum-tamam" echo Kurulum tamamlandi.

:run
dotnet run --project "backend\Iskur.Api" -- --upgrade-db
if errorlevel 1 goto :error
echo.
echo Backend baslatiliyor: http://127.0.0.1:5225
echo Bu pencere acik kalsin. Durdurmak icin Ctrl+C kullanin.
echo Ardindan Frontend-Baslat.bat dosyasini acin.
dotnet run --project "backend\Iskur.Api" --urls http://127.0.0.1:5225
if errorlevel 1 goto :error
popd
exit /b 0

:error
echo.
echo Islem tamamlanamadi. Yukaridaki hata mesajini kontrol edin.
echo SQL Server calisiyor olmali. Ayrintilar README.md dosyasinda.
pause
popd
exit /b 1

:folder_error
echo HATA: Proje klasoru acilamadi.
pause
exit /b 1
