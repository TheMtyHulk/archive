$ErrorActionPreference = "Stop"

# Reset DB
Invoke-WebRequest -Uri "http://localhost:8000/Admin/Database/deleteContext" -UseBasicParsing -TimeoutSec 10 | Out-Null
Write-Host "1. DB reset"

# Register admin  
$sess = New-Object Microsoft.PowerShell.Commands.WebRequestSession
$p = Invoke-WebRequest -Uri "http://localhost:8000/Identity/Account/Register" -UseBasicParsing -WebSession $sess -TimeoutSec 10
$t = [regex]::Match($p.Content, '__RequestVerificationToken.*?value="([^"]*)"').Groups[1].Value
$b = "Input.Name=Test+Employee&Input.Email=admin1%40fastbite.com&Input.PhoneNumber=77777777777&Input.Address=Wallstreet&Input.Password=Admin%40123&Input.ConfirmPassword=Admin%40123&Input.role=ADMIN&__RequestVerificationToken=" + [uri]::EscapeDataString($t)
$r = Invoke-WebRequest -Uri "http://localhost:8000/Identity/Account/Register" -Method POST -Body $b -ContentType "application/x-www-form-urlencoded" -UseBasicParsing -WebSession $sess -TimeoutSec 10
if ($r.Content -match "admin1@fastbite.com") { Write-Host "2. Admin registered" } else { Write-Host "2. Admin FAILED" }

# New session - login as admin
$s2 = New-Object Microsoft.PowerShell.Commands.WebRequestSession
$lp = Invoke-WebRequest -Uri "http://localhost:8000/Identity/Account/Login" -UseBasicParsing -WebSession $s2 -TimeoutSec 10
$lt = [regex]::Match($lp.Content, '__RequestVerificationToken.*?value="([^"]*)"').Groups[1].Value
$lb = "Input.Email=admin1%40fastbite.com&Input.Password=Admin%40123&Input.RememberMe=false&__RequestVerificationToken=" + [uri]::EscapeDataString($lt)
$lr = Invoke-WebRequest -Uri "http://localhost:8000/Identity/Account/Login" -Method POST -Body $lb -ContentType "application/x-www-form-urlencoded" -UseBasicParsing -WebSession $s2 -TimeoutSec 10
if ($lr.Content -match "admin1@fastbite.com") { Write-Host "3. Login OK" } else { Write-Host "3. Login FAIL" }

# Navigate to Category Create
$cp = Invoke-WebRequest -Uri "http://localhost:8000/Admin/Category/Create" -UseBasicParsing -WebSession $s2 -TimeoutSec 10
$ct = [regex]::Match($cp.Content, '__RequestVerificationToken.*?value="([^"]*)"').Groups[1].Value
Write-Host "4. Create page loaded, token: $([bool]([string]::IsNullOrEmpty($ct) -eq $false))"

# Submit category
$cb = "Name=Apetizer&__RequestVerificationToken=" + [uri]::EscapeDataString($ct)
$cr = Invoke-WebRequest -Uri "http://localhost:8000/Admin/Category/Create" -Method POST -Body $cb -ContentType "application/x-www-form-urlencoded" -UseBasicParsing -WebSession $s2 -TimeoutSec 10
Write-Host "5. Create result status: $($cr.StatusCode)"
$tm = [regex]::Match($cr.Content, '(?s)<tbody>.*?</tbody>')
Write-Host "5. tbody: $($tm.Value)"
if ($cr.Content -match "Apetizer") { Write-Host "5. Category FOUND" } else { Write-Host "5. Category NOT FOUND" }
