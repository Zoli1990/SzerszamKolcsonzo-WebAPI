# Foglalások státusz ellenőrzése
$token = "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJzdWIiOiIxIiwiZW1haWwiOiJhZG1pbkBzemVyc3phbS5odSIsImh0dHA6Ly9zY2hlbWFzLm1pY3Jvc29mdC5jb20vd3MvMjAwOC8wNi9pZGVudGl0eS9jbGFpbXMvcm9sZSI6IkFkbWluIiwianRpIjoiYTlhNjY5MDMtMDdkMi00NTVmLWIzMTctOGY2OWNmNWVhZDcyIiwiZXhwIjoxNzY4NjQ0ODQwLCJpc3MiOiJzemVyc3phbWtvbGNzb256by1hcGkiLCJhdWQiOiJzemVyc3phbWtvbGNzb256by11c2VycyJ9.nv6l56Rp6_FWj7miV5JfAKke8VwllzcQngaU-jiWw9I"

$headers = @{ "Authorization" = "Bearer $token" }

Write-Host "═══════════════════════════════════════════════" -ForegroundColor Cyan
Write-Host "  FOGLALÁSOK STÁTUSZA" -ForegroundColor Cyan
Write-Host "═══════════════════════════════════════════════" -ForegroundColor Cyan

$foglalasok = Invoke-RestMethod -Uri "https://szerszamkolcsonzo.runasp.net/api/Foglalasok" -Headers $headers

Write-Host "`nÖsszesen: $($foglalasok.Count) foglalás`n"

$fuggobe = $foglalasok | Where-Object { $_.status -eq "Függőben" -or $_.status -eq 1 }
$kiadva = $foglalasok | Where-Object { $_.status -eq "Kiadva" -or $_.status -eq 2 }
$lezarva = $foglalasok | Where-Object { $_.status -eq "Lezárva" -or $_.status -eq 3 }

Write-Host "📊 Státusz összesítő:" -ForegroundColor Yellow
Write-Host "─────────────────────────────────────────────────"
Write-Host "  🟡 Függőben: $($fuggobe.Count) db" -ForegroundColor Yellow
Write-Host "  🔵 Kiadva:   $($kiadva.Count) db" -ForegroundColor Cyan
Write-Host "  🟢 Lezárva:  $($lezarva.Count) db" -ForegroundColor Green

if ($fuggobe.Count -gt 0) {
    Write-Host "`n🔔 FÜGGŐBEN LÉVŐ FOGLALÁSOK:" -ForegroundColor Yellow
    Write-Host "─────────────────────────────────────────────────"
    $fuggobe | ForEach-Object {
        Write-Host "  #$($_.id) - $($_.eszkoz.nev) - $($_.felhasznalo.nev)"
        Write-Host "       Létrehozva: $($_.createdAt)"
        Write-Host "       Kezdés: $($_.kezdetDatum)"
        Write-Host ""
    }
} else {
    Write-Host "`n⚠️  NINCS FÜGGŐBEN LÉVŐ FOGLALÁS" -ForegroundColor Red
    Write-Host "   → Ezért nem jelenik meg az értesítés a WPF-ben!`n" -ForegroundColor Red
}

if ($kiadva.Count -gt 0) {
    Write-Host "`n📦 KIADOTT FOGLALÁSOK:" -ForegroundColor Cyan
    Write-Host "─────────────────────────────────────────────────"
    $kiadva | ForEach-Object {
        Write-Host "  #$($_.id) - $($_.eszkoz.nev) - $($_.felhasznalo.nev)"
        Write-Host "       Kiadva: $($_.kiadasDatum)"
        Write-Host ""
    }
}

Write-Host "`n💡 TIPP:" -ForegroundColor Green
Write-Host "─────────────────────────────────────────────────"
Write-Host "A WPF értesítés panel csak akkor jelenik meg, ha van"
Write-Host "'Függőben' státuszú foglalás!"
Write-Host "`nKészíts új foglalást a frontend-en és NE add ki még!"
Write-Host "5 másodpercen belül megjelenik a WPF-ben! 🔔"

Write-Host "`n`nNyomj ENTER-t..."
Read-Host
