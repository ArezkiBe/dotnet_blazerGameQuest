param([string]$User = 'admin')

$keycloakUrl = 'http://localhost:8180'
$realm = 'blazor-gamequest'
$clientId = 'blazor-client'

if ($User -eq 'admin') {
    $username = 'admin'
    $password = 'admin'
} else {
    $username = 'user1'
    $password = '1234'
}

Write-Host 'Recuperation du token JWT pour:' $username -ForegroundColor Cyan
Write-Host ''

$tokenUrl = "$keycloakUrl/realms/$realm/protocol/openid-connect/token"
$body = @{
    client_id = $clientId
    username = $username
    password = $password
    grant_type = 'password'
}

try {
    $response = Invoke-RestMethod -Uri $tokenUrl -Method Post -Body $body -ContentType 'application/x-www-form-urlencoded'
    $accessToken = $response.access_token
    $expiresIn = $response.expires_in
    
    Write-Host 'Token JWT obtenu avec succes!' -ForegroundColor Green
    Write-Host ''
    Write-Host 'Utilisateur:' $username
    Write-Host 'Expire dans:' $expiresIn 'secondes'
    Write-Host ''
    Write-Host 'Token (copier dans Swagger SANS Bearer):' -ForegroundColor Yellow
    Write-Host ''
    Write-Host $accessToken -ForegroundColor White
    Write-Host ''
    
    try {
        $accessToken | Set-Clipboard
        Write-Host 'Token copie dans le presse-papier!' -ForegroundColor Green
    } catch {
        Write-Host 'Impossible de copier dans le presse-papier' -ForegroundColor Yellow
    }
    
    Write-Host ''
    Write-Host 'Utilisation dans Swagger:' -ForegroundColor Cyan
    Write-Host '1. Allez sur http://localhost:5002/swagger'
    Write-Host '2. Cliquez sur le bouton Authorize'
    Write-Host '3. Collez le token (deja dans le presse-papier)'
    Write-Host '4. Cliquez sur Authorize puis Close'
    Write-Host ''
    
} catch {
    Write-Host 'Erreur lors de la recuperation du token:' -ForegroundColor Red
    Write-Host $_.Exception.Message -ForegroundColor Red
    Write-Host ''
    Write-Host 'Verifications:' -ForegroundColor Yellow
    Write-Host '- Keycloak est-il lance sur' $keycloakUrl '?'
    Write-Host '- Le realm' $realm 'existe-t-il ?'
    Write-Host '- Le client' $clientId 'est-il configure ?'
    Write-Host '- Direct access grants est-il activé sur le blazor-client sur keycloak?'
    exit 1
}
