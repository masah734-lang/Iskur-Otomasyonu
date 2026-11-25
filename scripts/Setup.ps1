param([string]$SqlServer='.\SQLEXPRESS', [string]$Database='IskurVize', [string]$ConnectionString)
$ErrorActionPreference='Stop'
$root=Split-Path $PSScriptRoot -Parent
if(-not (Get-Command dotnet -ErrorAction SilentlyContinue)){throw '.NET 10 SDK kurulmalı.'}
if(-not (Get-Command npm -ErrorAction SilentlyContinue)){throw 'Node.js 22.12 veya üzeri kurulmalı.'}
$env:ASPNETCORE_ENVIRONMENT='Development'
$env:ConnectionStrings__Default=if($ConnectionString){$ConnectionString}else{"Server=$SqlServer;Database=$Database;Trusted_Connection=True;TrustServerCertificate=True"}
$secret=Read-Host 'Demo hesaplar için en az 10 karakter, büyük/küçük harf, sayı ve özel karakter içeren parola' -AsSecureString
$env:ISKUR_DEMO_PASSWORD=[System.Net.NetworkCredential]::new('', $secret).Password
try {
 dotnet restore "$root/backend/Iskur.Api";if($LASTEXITCODE -ne 0){throw 'NuGet paketleri yüklenemedi.'}
 dotnet run --project "$root/backend/Iskur.Api" -- --init-db;if($LASTEXITCODE -ne 0){throw 'Veritabanı başlangıcı başarısız. Bağlantıyı ve boş veritabanını kontrol edin.'}
 Push-Location "$root/frontend"
 try {npm ci;if($LASTEXITCODE -ne 0){throw 'npm kurulumu başarısız.'}} finally {Pop-Location}
 Write-Host 'Kurulum tamamlandı. Sunucu ve arayüzü README adımlarıyla iki ayrı terminalde başlatın.'
}finally {Remove-Item Env:ISKUR_DEMO_PASSWORD -ErrorAction SilentlyContinue}
