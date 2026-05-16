$ErrorActionPreference = 'Stop'

$baseUrl = 'http://localhost:5000'

function Invoke-Api {
    param(
        [string]$Method,
        [string]$Path,
        [object]$Body = $null,
        [hashtable]$Headers = @{}
    )

    $params = @{
        Uri         = $baseUrl.TrimEnd('/') + $Path
        Method      = $Method
        Headers     = $Headers
        ContentType = 'application/json'
    }

    if ($null -ne $Body) {
        $params.Body = ($Body | ConvertTo-Json -Depth 10)
    }

    Invoke-RestMethod @params
}

function Assert-True {
    param(
        [bool]$Condition,
        [string]$Message
    )

    if (-not $Condition) {
        throw "Test failed: $Message"
    }
}

Write-Host 'Admin login test...'
$admin = Invoke-Api -Method Post -Path '/api/auth/login' -Body @{
    login = 'admin'
    password = 'admin123'
}

Assert-True ($admin.UserId -gt 0) 'admin login failed'
Assert-True ($admin.Role -eq 'Administrator') 'admin role mismatch'

$adminHeaders = @{
    'X-User-Id' = [string]$admin.UserId
    'X-User-Role' = $admin.Role
}

Write-Host 'Editor login test...'
$editor = Invoke-Api -Method Post -Path '/api/auth/login' -Body @{
    login = 'editor'
    password = 'editor123'
}

Assert-True ($editor.UserId -gt 0) 'editor login failed'
Assert-True ($editor.Role -eq 'CatalogEditor') 'editor role mismatch'

$editorHeaders = @{
    'X-User-Id' = [string]$editor.UserId
    'X-User-Role' = $editor.Role
}

Write-Host 'Lookup and movies read tests...'
$lookups = Invoke-Api -Method Get -Path '/api/movies/lookups' -Headers $adminHeaders
Assert-True ($lookups.Directors.Count -ge 1) 'directors lookup is empty'
Assert-True ($lookups.Genres.Count -ge 1) 'genres lookup is empty'
Assert-True ($lookups.Actors.Count -ge 1) 'actors lookup is empty'

$movies = Invoke-Api -Method Get -Path '/api/movies' -Headers $adminHeaders
Assert-True ($movies.Count -ge 1) 'movies list is empty'

$filteredMovies = Invoke-Api -Method Get -Path '/api/movies/filter?search=Inception&primarySort=title' -Headers $adminHeaders
Assert-True ($filteredMovies.Count -ge 0) 'movie filter endpoint failed'

$stats = Invoke-Api -Method Get -Path '/api/movies/statistics' -Headers $adminHeaders
Assert-True ($stats.TotalMovies -ge 0) 'statistics endpoint failed'

Write-Host 'Actor CRUD test...'
$actorName = 'Smoke Actor ' + [DateTime]::Now.ToString('yyyyMMddHHmmss')
$newActor = Invoke-Api -Method Post -Path '/api/actors' -Headers $adminHeaders -Body @{
    name = $actorName
}

Assert-True ($newActor.Id -gt 0) 'actor create failed'

$updatedActorName = $actorName + ' Updated'
$updatedActor = Invoke-Api -Method Put -Path "/api/actors/$($newActor.Id)" -Headers $adminHeaders -Body @{
    id = $newActor.Id
    name = $updatedActorName
}

Assert-True ($updatedActor.Name -eq $updatedActorName) 'actor update failed'

Invoke-Api -Method Delete -Path "/api/actors/$($newActor.Id)" -Headers $adminHeaders | Out-Null

Write-Host 'Editor permission test...'
$editorMovies = Invoke-Api -Method Get -Path '/api/movies' -Headers $editorHeaders
Assert-True ($editorMovies.Count -ge 0) 'editor movie list failed'

$forbiddenWorked = $false
try {
    Invoke-Api -Method Post -Path '/api/actors' -Headers $editorHeaders -Body @{ name = 'Forbidden Smoke Actor' } | Out-Null
    $forbiddenWorked = $true
} catch {
    $forbiddenWorked = $false
}

Assert-True (-not $forbiddenWorked) 'editor should not create actors'

Write-Host 'All API smoke tests passed.'
