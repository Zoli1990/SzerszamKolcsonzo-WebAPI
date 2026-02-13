#########################################################
# UTF-8 KODOLAS BEALLITASA (FONTOS!)
#########################################################

chcp 65001 | Out-Null
[Console]::InputEncoding  = [System.Text.Encoding]::UTF8
[Console]::OutputEncoding = [System.Text.Encoding]::UTF8
$PSDefaultParameterValues['*:Encoding'] = 'utf8'

#########################################################
# FRONTEND TELJES TESZT SCRIPT
# Vegigmegy minden forgatokonyven
#########################################################

$baseUrl = "http://localhost:5265/api"

Write-Host "`n" -NoNewline
Write-Host "===============================================================" -ForegroundColor Cyan
Write-Host "   FRONTEND TELJES TESZT SCRIPT" -ForegroundColor Cyan
Write-Host "===============================================================" -ForegroundColor Cyan
Write-Host "`n"

# Globalis valtozok
$testResults = @{
    Passed = 0
    Failed = 0
    Total = 0
}

function Test-Step {
    param(
        [string]$Name,
        [scriptblock]$Action,
        [scriptblock]$Validation
    )
    
    $testResults.Total++
    Write-Host "`n>>> TESZT: $Name" -ForegroundColor Yellow
    
    try {
        $result = & $Action
        $isValid = & $Validation $result
        
        if ($isValid) {
            $testResults.Passed++
            Write-Host "    [OK] SIKERES" -ForegroundColor Green
            return $result
        } else {
            $testResults.Failed++
            Write-Host "    [HIBA] SIKERTELEN - Validacio hiba" -ForegroundColor Red
            return $null
        }
    }
    catch {
        $testResults.Failed++
        Write-Host "    [HIBA] SIKERTELEN - $($_.Exception.Message)" -ForegroundColor Red
        return $null
    }
}

#########################################################
# SZCENARIO 1: SIKERES FOGLALAS ELETCIKLUS
#########################################################

Write-Host "`n" -NoNewline
Write-Host "-------------------------------------------------------------" -ForegroundColor White
Write-Host "SZCENARIO 1: Sikeres foglalas -> kiadas -> visszahozas" -ForegroundColor White
Write-Host "-------------------------------------------------------------" -ForegroundColor White

# 1.1 Foglalas letrehozasa
$foglalas = Test-Step -Name "1.1 - Foglalas letrehozasa" -Action {
    $data = @{
        eszkozID = 1
        nev = "Teszt Peter"
        email = "teszt.peter@example.com"
        telefonszam = "+36301234567"
        cim = "Budapest, Teszt utca 1."
        foglalasKezdete = (Get-Date).AddHours(1).ToString("yyyy-MM-ddTHH:mm:ss")
        # ❌ foglalasVege - TÖRÖLVE
        # ❌ oraSzam - TÖRÖLVE
    } | ConvertTo-Json -Depth 5
    
    Invoke-RestMethod -Uri "$baseUrl/foglalasok" `
        -Method Post `
        -Body $data `
        -ContentType "application/json; charset=utf-8"
} -Validation {
    param($result)
    ($result -ne $null) -and ($result.status -eq "Aktiv")
}

if ($foglalas) {
    Write-Host "    Foglalas ID: $($foglalas.foglalasID)" -ForegroundColor Cyan
    Write-Host "    Statusz: $($foglalas.status)" -ForegroundColor Cyan
    Write-Host "    Eszkoz: $($foglalas.eszkozNev)" -ForegroundColor Cyan
}

Start-Sleep -Seconds 2

# 1.2 Foglalas kiadasa
$kiadva = Test-Step -Name "1.2 - Foglalas kiadasa (KIADVA gomb)" -Action {
    Invoke-RestMethod -Uri "$baseUrl/foglalasok/$($foglalas.foglalasID)/kiad" `
        -Method Post
} -Validation {
    param($result)
    ($result -ne $null) -and ($result.message -like "*sikeresen kiadva*")
}

if ($kiadva) {
    Write-Host "    Kiadas idopontja: $($kiadva.kiadasIdopontja)" -ForegroundColor Cyan
}

Start-Sleep -Seconds 2

# 1.3 Eszkoz statusz ellenorzese (kiadas utan)
$eszkozKiadasUtan = Test-Step -Name "1.3 - Eszkoz statusz ellenorzese (Kiadva)" -Action {
    Invoke-RestMethod -Uri "$baseUrl/eszkozok/1" -Method Get
} -Validation {
    param($result)
    $result.status -eq "Kiadva"
}

Start-Sleep -Seconds 2

# 1.4 Eszkoz visszahozasa
$visszahozva = Test-Step -Name "1.4 - Eszkoz visszahozasa (VISSZAHOZVA gomb)" -Action {
    Invoke-RestMethod -Uri "$baseUrl/eszkozok/1/visszahoz" -Method Post
} -Validation {
    param($result)
    ($result -ne $null) -and ($result.message -like "*visszahozva*")
}

Start-Sleep -Seconds 2

# 1.5 Eszkoz statusz ellenorzese (visszahozas utan)
$eszkozVissza = Test-Step -Name "1.5 - Eszkoz statusz ellenorzese (Elerheto)" -Action {
    Invoke-RestMethod -Uri "$baseUrl/eszkozok/1" -Method Get
} -Validation {
    param($result)
    $result.status -eq "Elerheto"
}

Start-Sleep -Seconds 2

# 1.6 Foglalas statusz ellenorzese (visszahozas utan)
$foglalasLezart = Test-Step -Name "1.6 - Foglalas statusz ellenorzese (Lezart)" -Action {
    Invoke-RestMethod -Uri "$baseUrl/foglalasok/$($foglalas.foglalasID)" -Method Get
} -Validation {
    param($result)
    $result.status -eq "Lezart"
}

#########################################################
# SZCENARIO 2: ESZKOZ KIVONASA ES VISSZAALLITASA
#########################################################

Write-Host "`n" -NoNewline
Write-Host "-------------------------------------------------------------" -ForegroundColor White
Write-Host "SZCENARIO 2: Eszkoz kivonasa -> visszaallitasa" -ForegroundColor White
Write-Host "-------------------------------------------------------------" -ForegroundColor White

# 2.1 Eszkoz kivonasa megjegyzesssel
$kivonva = Test-Step -Name "2.1 - Eszkoz kivonasa megjegyzesssel (KIVONVA gomb)" -Action {
    $data = @{
        megjegyzes = "Szervizben van - teszteles alatt"
    } | ConvertTo-Json -Depth 5
    
    Invoke-RestMethod -Uri "$baseUrl/eszkozok/1/kivon" `
        -Method Post `
        -Body $data `
        -ContentType "application/json; charset=utf-8"
} -Validation {
    param($result)
    ($result -ne $null) -and ($result.ujStatus -eq "Kivonva") -and ($result.megjegyzes -ne $null)
}

if ($kivonva) {
    Write-Host "    Megjegyzes: $($kivonva.megjegyzes)" -ForegroundColor Cyan
}

Start-Sleep -Seconds 2

# 2.2 Eszkoz statusz ellenorzese (kivonas utan)
$eszkozKivonva = Test-Step -Name "2.2 - Eszkoz statusz ellenorzese (Kivonva)" -Action {
    Invoke-RestMethod -Uri "$baseUrl/eszkozok/1" -Method Get
} -Validation {
    param($result)
    ($result.status -eq "Kivonva") -and ($result.megjegyzes -eq "Szervizben van - teszteles alatt")
}

Start-Sleep -Seconds 2

# 2.3 Eszkoz visszaallitasa elerhetore
$elerhetove = Test-Step -Name "2.3 - Eszkoz visszaallitasa (ELERHETO gomb)" -Action {
    Invoke-RestMethod -Uri "$baseUrl/eszkozok/1/elerheto" -Method Post
} -Validation {
    param($result)
    ($result -ne $null) -and ($result.ujStatus -eq "Elerheto")
}

Start-Sleep -Seconds 2

# 2.4 Eszkoz statusz ellenorzese (visszaallitas utan)
$eszkozElerheto = Test-Step -Name "2.4 - Eszkoz statusz ellenorzese (Elerheto + megjegyzes torolve)" -Action {
    Invoke-RestMethod -Uri "$baseUrl/eszkozok/1" -Method Get
} -Validation {
    param($result)
    ($result.status -eq "Elerheto") -and ($result.megjegyzes -eq $null -or $result.megjegyzes -eq "")
}

#########################################################
# SZCENARIO 3: HIBAKEZELESEK TESZTELESE
#########################################################

Write-Host "`n" -NoNewline
Write-Host "-------------------------------------------------------------" -ForegroundColor White
Write-Host "SZCENARIO 3: Hibakezelesek tesztelese" -ForegroundColor White
Write-Host "-------------------------------------------------------------" -ForegroundColor White

# 3.1 Foglalas letrehozasa (hibakezeleshez)
$foglalasHiba = Test-Step -Name "3.1 - Foglalas letrehozasa" -Action {
    $data = @{
        eszkozID = 1
        nev = "Teszt Janos"
        email = "teszt.janos@example.com"
        telefonszam = "+36309876543"
        cim = "Budapest, Proba utca 2."
        foglalasKezdete = (Get-Date).AddHours(1).ToString("yyyy-MM-ddTHH:mm:ss")
        # ❌ foglalasVege - TÖRÖLVE
        # ❌ oraSzam - TÖRÖLVE
    } | ConvertTo-Json -Depth 5
    
    Invoke-RestMethod -Uri "$baseUrl/foglalasok" `
        -Method Post `
        -Body $data `
        -ContentType "application/json; charset=utf-8"
} -Validation {
    param($result)
    $result.status -eq "Aktiv"
}

Start-Sleep -Seconds 2

# 3.2 Foglalas kiadasa
Test-Step -Name "3.2 - Foglalas kiadasa (elso alkalommal)" -Action {
    Invoke-RestMethod -Uri "$baseUrl/foglalasok/$($foglalasHiba.foglalasID)/kiad" -Method Post
} -Validation {
    param($result)
    $result -ne $null
}

Start-Sleep -Seconds 2

# 3.3 Dupla kiadas probalkozas (HIBA ELVART)
$duplaKiadas = Test-Step -Name "3.3 - Dupla kiadas probalkozas (hiba elvart)" -Action {
    try {
        Invoke-RestMethod -Uri "$baseUrl/foglalasok/$($foglalasHiba.foglalasID)/kiad" -Method Post
        return $null
    }
    catch {
        return $_.Exception.Response.StatusCode.value__
    }
} -Validation {
    param($result)
    $result -eq 400
}

Start-Sleep -Seconds 2

# 3.4 Nem letezo eszkoz visszahozasa (HIBA ELVART)
$nemLetezik = Test-Step -Name "3.4 - Nem letezo eszkoz visszahozasa (hiba elvart)" -Action {
    try {
        Invoke-RestMethod -Uri "$baseUrl/eszkozok/9999/visszahoz" -Method Post
        return $null
    }
    catch {
        return $_.Exception.Response.StatusCode.value__
    }
} -Validation {
    param($result)
    $result -eq 404
}

Start-Sleep -Seconds 2

# 3.5 Ures megjegyzesssel kivonas (HIBA ELVART)
$uresMegjegyzes = Test-Step -Name "3.5 - Ures megjegyzesssel kivonas (hiba elvart)" -Action {
    try {
        $data = @{ megjegyzes = "" } | ConvertTo-Json
        Invoke-RestMethod -Uri "$baseUrl/eszkozok/1/kivon" `
            -Method Post `
            -Body $data `
            -ContentType "application/json"
        return $null
    }
    catch {
        return $_.Exception.Response.StatusCode.value__
    }
} -Validation {
    param($result)
    $result -eq 400
}

#########################################################
# OSSZESITES
#########################################################

Write-Host "`n" -NoNewline
Write-Host "===============================================================" -ForegroundColor Cyan
Write-Host "   TESZT EREDMENYEK" -ForegroundColor Cyan
Write-Host "===============================================================" -ForegroundColor Cyan
Write-Host "`n"

Write-Host "Osszes teszt:    $($testResults.Total)" -ForegroundColor White
Write-Host "Sikeres:         $($testResults.Passed)" -ForegroundColor Green
Write-Host "Sikertelen:      $($testResults.Failed)" -ForegroundColor Red

$percentage = [math]::Round(($testResults.Passed / $testResults.Total) * 100, 2)
$color = if ($percentage -eq 100) { "Green" } else { "Yellow" }
Write-Host "Sikeressegi arany: $percentage%" -ForegroundColor $color

Write-Host "`n"

if ($testResults.Failed -eq 0) {
    Write-Host "[SIKER] MINDEN TESZT SIKERES! A backend teljesen mukodokepes!" -ForegroundColor Green
} else {
    Write-Host "[FIGYELEM] Van nehany sikertelen teszt. Ellenorizd a hibakat!" -ForegroundColor Yellow
}

Write-Host "`n"
Write-Host "===============================================================" -ForegroundColor Cyan
Write-Host "`n"
