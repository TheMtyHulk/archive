$ErrorActionPreference = "Stop"

# Login as restaurant owner
$s = New-Object Microsoft.PowerShell.Commands.WebRequestSession
$lp = Invoke-WebRequest -Uri "http://localhost:8000/Identity/Account/Login" -UseBasicParsing -WebSession $s -TimeoutSec 10
$lt = [regex]::Match($lp.Content, '__RequestVerificationToken.*?value="([^"]*)"').Groups[1].Value
$lb = "Input.Email=restaurant1%40fastbite.com&Input.Password=Restaurant%401&Input.RememberMe=false&__RequestVerificationToken=" + [uri]::EscapeDataString($lt)
$lr = Invoke-WebRequest -Uri "http://localhost:8000/Identity/Account/Login" -Method POST -Body $lb -ContentType "application/x-www-form-urlencoded" -UseBasicParsing -WebSession $s -TimeoutSec 10
Write-Host "1. Login: $(if ($lr.Content -match 'restaurant1@fastbite.com') {'OK'} else {'FAIL'})"

# Get Restaurant Create page - show ALL forms
$cp = Invoke-WebRequest -Uri "http://localhost:8000/Admin/Restaurant/Create" -UseBasicParsing -WebSession $s -TimeoutSec 10
$allForms = [regex]::Matches($cp.Content, '(?s)<form.*?</form>')
Write-Host "2. Number of forms on page: $($allForms.Count)"
for ($i = 0; $i -lt $allForms.Count; $i++) {
    Write-Host "=== Form $($i+1) ==="
    Write-Host $allForms[$i].Value
    Write-Host ""
}

# Now POST directly without following redirect
$allTokens = [regex]::Matches($cp.Content, 'name="__RequestVerificationToken".*?value="([^"]*)"')
Write-Host "3. Number of tokens: $($allTokens.Count)"

# Also check what the create form's action would be
Write-Host "4. Page content around the Create form:"
$m = [regex]::Match($cp.Content, '(?s)Create.*?<form.*?</form>')
Write-Host $m.Value
