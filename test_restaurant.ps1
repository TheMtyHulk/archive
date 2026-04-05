$ErrorActionPreference = "Stop"

# Login as restaurant owner
$s = New-Object Microsoft.PowerShell.Commands.WebRequestSession
$lp = Invoke-WebRequest -Uri "http://localhost:8000/Identity/Account/Login" -UseBasicParsing -WebSession $s -TimeoutSec 10
$lt = [regex]::Match($lp.Content, '__RequestVerificationToken.*?value="([^"]*)"').Groups[1].Value
$lb = "Input.Email=restaurant1%40fastbite.com&Input.Password=Restaurant%401&Input.RememberMe=false&__RequestVerificationToken=" + [uri]::EscapeDataString($lt)
$lr = Invoke-WebRequest -Uri "http://localhost:8000/Identity/Account/Login" -Method POST -Body $lb -ContentType "application/x-www-form-urlencoded" -UseBasicParsing -WebSession $s -TimeoutSec 10
if ($lr.Content -match "restaurant1@fastbite.com") { Write-Host "1. Login OK" } else { Write-Host "1. Login FAIL" }

# Get Restaurant Create page
$cp = Invoke-WebRequest -Uri "http://localhost:8000/Admin/Restaurant/Create" -UseBasicParsing -WebSession $s -TimeoutSec 10
Write-Host "2. Create page URL: $($cp.BaseResponse.ResponseUri)"
$formHtml = [regex]::Match($cp.Content, '(?s)<form.*?</form>').Value
Write-Host "2. Form HTML:"
Write-Host $formHtml

# Get anti-forgery token
$ct = [regex]::Match($cp.Content, '__RequestVerificationToken.*?value="([^"]*)"').Groups[1].Value
Write-Host "2. Has token: $([bool]([string]::IsNullOrEmpty($ct) -eq $false))"

# Submit restaurant create form
$postBody = "RestaurantName=Restaurant1&Address=Wallstreet"
if ($ct) { $postBody += "&__RequestVerificationToken=" + [uri]::EscapeDataString($ct) }
Write-Host "3. Posting: $postBody"

try {
    $cr = Invoke-WebRequest -Uri "http://localhost:8000/Admin/Restaurant/Create" -Method POST -Body $postBody -ContentType "application/x-www-form-urlencoded" -UseBasicParsing -WebSession $s -TimeoutSec 10 -MaximumRedirection 0
    Write-Host "3. Status: $($cr.StatusCode)"
} catch {
    $ex = $_.Exception
    if ($ex.Response) {
        $statusCode = [int]$ex.Response.StatusCode
        $location = $ex.Response.Headers["Location"]
        Write-Host "3. Status: $statusCode, Location: $location"
    } else {
        Write-Host "3. Error: $ex"
    }
}

# Follow redirect or check result  
$cr2 = Invoke-WebRequest -Uri "http://localhost:8000/Admin/Restaurant" -UseBasicParsing -WebSession $s -TimeoutSec 10
$tm = [regex]::Match($cr2.Content, '(?s)<tbody>.*?</tbody>').Value
Write-Host "4. Restaurant Index tbody: $tm"
