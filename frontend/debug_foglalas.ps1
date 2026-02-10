# Teszt - részletes hibakiírással

$baseUrl = "http://localhost:5265/api"

Write-Host "=== TESZT 1: Foglalás létrehozása ===" -ForegroundColor Yellow

$data = @{
    eszkozID = 1
    nev = "Teszt Peter"
    email = "teszt.peter@example.com"
    telefonszam = "+36301234567"
    cim = "Budapest, Teszt utca 1."
    foglalasKezdete = (Get-Date).AddHours(1).ToString("yyyy-MM-ddTHH:mm:ss")
    foglalasVege = (Get-Date).AddHours(9).ToString("yyyy-MM-ddTHH:mm:ss")
    oraSzam = 8
} | ConvertTo-Json -Depth 5

Write-Host "Request body:" -ForegroundColor Cyan
Write-Host $data

try {
    $response = Invoke-RestMethod -Uri "$baseUrl/foglalasok" `
        -Method Post `
        -Body $data `
        -ContentType "application/json; charset=utf-8"
    
    Write-Host "SIKER!" -ForegroundColor Green
    Write-Host ($response | ConvertTo-Json)
}
catch {
    Write-Host "HIBA:" -ForegroundColor Red
    Write-Host "StatusCode: $($_.Exception.Response.StatusCode.value__)"
    Write-Host "Error: $($_.Exception.Message)"
    
    # Részletes hibaüzenet
    if ($_.ErrorDetails) {
        Write-Host "Details: $($_.ErrorDetails.Message)"
    }
}
