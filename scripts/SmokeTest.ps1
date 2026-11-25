param([string]$BaseUrl='http://127.0.0.1:5225/api')
$ErrorActionPreference='Stop'
$secret=Read-Host 'Kurulumda belirlediğiniz demo parolası' -AsSecureString
$password=[System.Net.NetworkCredential]::new('', $secret).Password
function Login($email){$a=Invoke-RestMethod "$BaseUrl/auth/login" -Method Post -ContentType 'application/json' -Body (@{email=$email;password=$password}|ConvertTo-Json);return @{Authorization="Bearer $($a.accessToken)"}}
function Call($path,$headers,$method='Get',$body=$null){$args=@{Uri="$BaseUrl/$path";Headers=$headers;Method=$method};if($null -ne $body){$args.ContentType='application/json';$args.Body=ConvertTo-Json -Depth 10 -InputObject $body};Invoke-RestMethod @args}
$student=Login 'ogrenci1@demo.local'
$list=@(Call 'students' $student)
if($list.Count -ne 1){throw 'Öğrenci kapsamı hatalı.'}
$id=$list[0].id
$p=Call "students/$id" $student
$originalPhone=$p.phone
try {
 $p.phone='05001112233'
 Call "students/$id" $student 'Put' $p
 $again=Call "students/$id" $student
 if($again.phone -ne '05001112233'){throw 'Güncelleme kalıcı değil.'}
} finally {$p.phone=$originalPhone;Call "students/$id" $student 'Put' $p}
Write-Host 'PASS: Profil güncelleme ve tekrar okuma'
$management=Login 'yonetim@demo.local'
$all=@(Call 'students' $management)
$other=$all|Where-Object {$_.id -ne $id}|Select-Object -First 1
try {Call "students/$($other.id)" $student;throw 'Yetkisiz erişim kabul edildi.'}catch {if($_.Exception.Response.StatusCode.value__ -ne 403){throw}}
Write-Host 'PASS: Başka öğrencinin profiline erişim engellendi'
$filtered=@(Call 'students?year=2026&month=11' $management)
if($filtered.Count -lt 3){throw 'Ay/yıl filtreli örnek planlar bulunamadı.'}
Write-Host 'PASS: Ay/yıl filtresi'
$advisor=Login 'danisman@demo.local'
$own=@(Call 'students' $advisor)
if($own.Count -ne 2){throw 'Danışman erişim kapsamı hatalı.'}
Write-Host 'PASS: Danışman yalnız atanmış iki öğrenciyi görüyor'
Remove-Variable password
Write-Host 'Temel SQL Server smoke testleri tamamlandı.'
