# Debug script - Foglalás státusz ellenőrzés

$baseUrl = "http://localhost:5265/api"

Write-Host "`n=== 1. Foglalás létrehozása ===" -ForegroundColor Yellow
$data = @{
    eszkozID = 1
    nev = "Debug Test"
    email = "debug@test.com"
    telefonszam = "+36301111111"
    cim = "Debug cím"
    foglalasKezdete = (Get-Date).AddHours(1).ToString("yyyy-MM-ddTHH:mm:ss")
    foglalasVege = (Get-Date).AddHours(9).ToString("yyyy-MM-ddTHH:mm:ss")
    oraSzam = 8
} | ConvertTo-Json

$foglalas = Invoke-RestMethod -Uri "$baseUrl/foglalasok" -Method Post -Body $data -ContentType "application/json; charset=utf-8"
Write-Host "Foglalás ID: $($foglalas.foglalasID)" -ForegroundColor Green
Write-Host "Státusz: $($foglalas.status)" -ForegroundColor Green

Start-Sleep -Seconds 2

Write-Host "`n=== 2. Foglalás kiadása ===" -ForegroundColor Yellow
$kiadva = Invoke-RestMethod -Uri "$baseUrl/foglalasok/$($foglalas.foglalasID)/kiad" -Method Post
Write-Host "Kiadva!" -ForegroundColor Green

Start-Sleep -Seconds 2

Write-Host "`n=== 3. Eszköz visszahozása ===" -ForegroundColor Yellow
$vissza = Invoke-RestMethod -Uri "$baseUrl/eszkozok/1/visszahoz" -Method Post
Write-Host "Visszahozva!" -ForegroundColor Green
Write-Host "Response: $($vissza | ConvertTo-Json)" -ForegroundColor Cyan

Start-Sleep -Seconds 3

Write-Host "`n=== 4. Foglalás státusz ellenőrzése ===" -ForegroundColor Yellow
$foglalasUtana = Invoke-RestMethod -Uri "$baseUrl/foglalasok/$($foglalas.foglalasID)"
Write-Host "Foglalás ID: $($foglalasUtana.foglalasID)" -ForegroundColor Cyan
Write-Host "Státusz: $($foglalasUtana.status)" -ForegroundColor $(if ($foglalasUtana.status -eq "Lezart") { "Green" } else { "Red" })
Write-Host "Eszköz ID: $($foglalasUtana.eszkozID)" -ForegroundColor Cyan

Write-Host "`n=== 5. ÖSSZES KIADVA FOGLALÁS (debug) ===" -ForegroundColor Yellow
$osszesKiadva = Invoke-RestMethod -Uri "$baseUrl/foglalasok?eszkozId=1"
$kiadvaFoglalasok = $osszesKiadva | Where-Object { $_.status -eq "Kiadva" }
Write-Host "Kiadva státuszú foglalások száma: $($kiadvaFoglalasok.Count)" -ForegroundColor Cyan
$kiadvaFoglalasok | ForEach-Object {
    Write-Host "  - ID: $($_.foglalasID), Eszköz: $($_.eszkozID), Státusz: $($_.status)" -ForegroundColor Cyan
}
