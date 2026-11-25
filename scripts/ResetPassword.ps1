param([string]$SqlServer='.\SQLEXPRESS', [string]$Database='IskurVize')
$ErrorActionPreference='Stop'
$root=Split-Path $PSScriptRoot -Parent
$env:ASPNETCORE_ENVIRONMENT='Development'
if(-not $env:ConnectionStrings__Default){$env:ConnectionStrings__Default="Server=$SqlServer;Database=$Database;Trusted_Connection=True;TrustServerCertificate=True"}
$secret=Read-Host 'Yeni demo parolasi (10+ karakter, buyuk/kucuk harf, sayi, ozel karakter)' -AsSecureString
$env:ISKUR_DEMO_PASSWORD=[System.Net.NetworkCredential]::new('', $secret).Password
try {
 dotnet run --project "$root/backend/Iskur.Api" -- --reset-demo-password
 if($LASTEXITCODE -ne 0){throw 'Parola degistirilemedi. Yukaridaki hatayi kontrol edin.'}
}finally {Remove-Item Env:ISKUR_DEMO_PASSWORD -ErrorAction SilentlyContinue}
