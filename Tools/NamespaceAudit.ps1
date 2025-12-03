# Santa Project - Namespace Audit Script
# Este script analiza todos los archivos C# del proyecto y reporta:
# 1. Archivos sin namespace
# 2. Archivos con namespace incorrecto según la capa
# 3. Estadísticas por carpeta

param(
    [string]$ProjectPath = "d:\isabe\Documents\Santa\Assets\Scripts",
    [switch]$Verbose = $false,
    [switch]$ExportCsv = $false
)

# Colores para output
$colorCorrect = "Green"
$colorWarning = "Yellow"
$colorError = "Red"
$colorInfo = "Cyan"

# Mapeo de carpetas a namespaces esperados
$namespaceRules = @{
    "_Core\Interfaces"          = "Santa.Core"
    "_Core\Events"              = "Santa.Core.Events"
    "_Core\Pooling"             = "Santa.Core.Pooling"
    "_Core\Constants"           = "Santa.Core.Config"
    "_Core\Addressables"        = "Santa.Core.Addressables"
    "_Core\DI"                  = ""  # GameLifetimeScope no tiene namespace
    "_Core\Utils"               = "Santa.Core.Utils"
    "_Core\Tasks"               = ""  # Tasks no tienen namespace (Unity-specific)
    
    "Domain\Combat"             = "Santa.Domain.Combat"
    "Domain\Upgrades"           = "Santa.Domain.Upgrades"
    "Domain\Entities"           = "Santa.Domain.Entities"
    "Domain\Dialogue"           = "Santa.Domain.Dialogue"
    "Domain\Player"             = "Santa.Core.Player"  # Excepción: contratos en Core
    
    "Infrastructure\Combat"     = "Santa.Infrastructure.Combat"
    "Infrastructure\Audio"      = "Santa.Infrastructure.Audio"
    "Infrastructure\VFX"        = "Santa.Infrastructure.VFX"
    "Infrastructure\Save"       = "Santa.Core.Save"  # Consolidated under Santa.Core.Save
    "Infrastructure\State"      = "Santa.Infrastructure.State"
    "Infrastructure\Level"      = "Santa.Infrastructure.Level"
    "Infrastructure\Camera"     = "Santa.Infrastructure.Camera"
    "Infrastructure\Input"      = "Santa.Infrastructure.Input"
    
    "Presentation\Managers"     = "Santa.Presentation.UI"
    "Presentation\Combat"       = "Santa.Presentation.Combat"
    "Presentation\Upgrades"     = "Santa.Presentation.Upgrades"
    "Presentation\Menus"        = "Santa.UI"
    "Presentation\HUD"          = "Santa.Presentation.HUD"
    
    "UI"                        = "Santa.UI"
    "Utilities"                 = "Santa.Utils"
    "Editor"                    = "Santa.Editor"
}

# Archivos excluidos (generados o legacy)
$excludePatterns = @(
    "*\.meta",
    "*\.asmdef",
    "*.txt",
    "*ActionMap.cs",  # Auto-generated
    "*AssemblyInfo.cs"
)

# Estructura para resultados
class FileAnalysis {
    [string]$FilePath
    [string]$RelativePath
    [string]$CurrentNamespace
    [string]$ExpectedNamespace
    [string]$Status  # "Correct", "Missing", "Incorrect", "Excluded"
    [string]$Layer
    [int]$LineCount
}

$results = @()
$stats = @{
    Total = 0
    Correct = 0
    Missing = 0
    Incorrect = 0
    NoNamespaceExpected = 0
}

# Función para extraer namespace de un archivo
function Get-CSharpNamespace {
    param([string]$filePath)
    
    $content = Get-Content $filePath -Raw -ErrorAction SilentlyContinue
    if (-not $content) { return $null }
    
    # Buscar declaración de namespace
    if ($content -match 'namespace\s+([\w\.]+)') {
        return $matches[1]
    }
    
    return $null
}

# Función para determinar namespace esperado
function Get-ExpectedNamespace {
    param([string]$relativePath)
    
    foreach ($rule in $namespaceRules.GetEnumerator()) {
        $pattern = $rule.Key -replace '\\', '\\'
        if ($relativePath -match $pattern) {
            return $rule.Value
        }
    }
    
    # Default: sin namespace esperado para archivos en raíz de Scripts
    return ""
}

# Función para determinar capa
function Get-Layer {
    param([string]$relativePath)
    
    if ($relativePath -match '^_Core') { return "_Core" }
    if ($relativePath -match '^Domain') { return "Domain" }
    if ($relativePath -match '^Infrastructure') { return "Infrastructure" }
    if ($relativePath -match '^Presentation') { return "Presentation" }
    if ($relativePath -match '^UI\\') { return "UI" }
    if ($relativePath -match '^Editor') { return "Editor" }
    if ($relativePath -match '^Utilities') { return "Utilities" }
    
    return "Root"
}

# Validar path del proyecto
if (-not (Test-Path $ProjectPath)) {
    Write-Host "❌ Error: No se encuentra la ruta $ProjectPath" -ForegroundColor $colorError
    exit 1
}

Write-Host "`n🔍 Analizando proyecto Unity: Santa" -ForegroundColor $colorInfo
Write-Host "📁 Path: $ProjectPath`n" -ForegroundColor $colorInfo

# Obtener todos los archivos C#
$allFiles = Get-ChildItem -Path $ProjectPath -Filter "*.cs" -Recurse -File

foreach ($file in $allFiles) {
    $stats.Total++
    
    # Verificar si está excluido
    $excluded = $false
    foreach ($pattern in $excludePatterns) {
        if ($file.Name -like $pattern) {
            $excluded = $true
            break
        }
    }
    
    if ($excluded) {
        if ($Verbose) {
            Write-Host "⏭️  Excluido: $($file.Name)" -ForegroundColor Gray
        }
        continue
    }
    
    $relativePath = $file.FullName.Replace("$ProjectPath\", "")
    $currentNamespace = Get-CSharpNamespace -filePath $file.FullName
    $expectedNamespace = Get-ExpectedNamespace -relativePath $relativePath
    $layer = Get-Layer -relativePath $relativePath
    
    $analysis = [FileAnalysis]@{
        FilePath = $file.FullName
        RelativePath = $relativePath
        CurrentNamespace = if ($currentNamespace) { $currentNamespace } else { "(none)" }
        ExpectedNamespace = if ($expectedNamespace) { $expectedNamespace } else { "(none)" }
        Layer = $layer
        LineCount = (Get-Content $file.FullName).Count
    }
    
    # Determinar status
    if (-not $expectedNamespace -or $expectedNamespace -eq "") {
        # No se espera namespace (archivos en raíz, DI, etc.)
        $analysis.Status = "NoNamespaceExpected"
        $stats.NoNamespaceExpected++
        
        if ($Verbose) {
            Write-Host "ℹ️  $relativePath - Sin namespace esperado" -ForegroundColor Gray
        }
    }
    elseif (-not $currentNamespace) {
        # Falta namespace
        $analysis.Status = "Missing"
        $stats.Missing++
        
        Write-Host "❌ FALTA NAMESPACE: $relativePath" -ForegroundColor $colorError
        Write-Host "   Esperado: $expectedNamespace`n" -ForegroundColor $colorWarning
    }
    elseif ($expectedNamespace -contains '|') {
        # Múltiples namespaces válidos (Save/Security)
        $validNamespaces = $expectedNamespace -split '\|'
        if ($validNamespaces -contains $currentNamespace) {
            $analysis.Status = "Correct"
            $stats.Correct++
            
            if ($Verbose) {
                Write-Host "✅ $relativePath - $currentNamespace" -ForegroundColor $colorCorrect
            }
        }
        else {
            $analysis.Status = "Incorrect"
            $stats.Incorrect++
            
            Write-Host "⚠️  NAMESPACE INCORRECTO: $relativePath" -ForegroundColor $colorWarning
            Write-Host "   Actual: $currentNamespace" -ForegroundColor $colorError
            Write-Host "   Esperado: uno de [ $($validNamespaces -join ' | ') ]`n" -ForegroundColor $colorCorrect
        }
    }
    elseif ($currentNamespace -ne $expectedNamespace) {
        # Namespace incorrecto
        $analysis.Status = "Incorrect"
        $stats.Incorrect++
        
        Write-Host "⚠️  NAMESPACE INCORRECTO: $relativePath" -ForegroundColor $colorWarning
        Write-Host "   Actual: $currentNamespace" -ForegroundColor $colorError
        Write-Host "   Esperado: $expectedNamespace`n" -ForegroundColor $colorCorrect
    }
    else {
        # Correcto
        $analysis.Status = "Correct"
        $stats.Correct++
        
        if ($Verbose) {
            Write-Host "✅ $relativePath - $currentNamespace" -ForegroundColor $colorCorrect
        }
    }
    
    $results += $analysis
}

# Reportar estadísticas
Write-Host "`n" + ("="*70) -ForegroundColor $colorInfo
Write-Host "📊 ESTADÍSTICAS" -ForegroundColor $colorInfo
Write-Host ("="*70) -ForegroundColor $colorInfo

Write-Host "`nTotal de archivos analizados: $($stats.Total)" -ForegroundColor White
Write-Host "✅ Correctos: $($stats.Correct)" -ForegroundColor $colorCorrect
Write-Host "❌ Sin namespace: $($stats.Missing)" -ForegroundColor $colorError
Write-Host "⚠️  Namespace incorrecto: $($stats.Incorrect)" -ForegroundColor $colorWarning
Write-Host "ℹ️  Sin namespace esperado: $($stats.NoNamespaceExpected)" -ForegroundColor Gray

# Calcular porcentaje de conformidad
$requireNamespace = $stats.Total - $stats.NoNamespaceExpected
$conformant = $stats.Correct
$conformancePercentage = if ($requireNamespace -gt 0) { 
    [math]::Round(($conformant / $requireNamespace) * 100, 2) 
} else { 
    100 
}

Write-Host "`n📈 Conformidad con estándares: $conformancePercentage% ($conformant/$requireNamespace)" -ForegroundColor $(
    if ($conformancePercentage -ge 90) { $colorCorrect }
    elseif ($conformancePercentage -ge 70) { $colorWarning }
    else { $colorError }
)

# Estadísticas por capa
Write-Host "`n" + ("="*70) -ForegroundColor $colorInfo
Write-Host "📂 POR CAPA" -ForegroundColor $colorInfo
Write-Host ("="*70) -ForegroundColor $colorInfo

$layerStats = $results | Group-Object -Property Layer | Sort-Object Name
foreach ($layer in $layerStats) {
    $layerName = $layer.Name
    $total = $layer.Count
    $correct = ($layer.Group | Where-Object { $_.Status -eq "Correct" }).Count
    $missing = ($layer.Group | Where-Object { $_.Status -eq "Missing" }).Count
    $incorrect = ($layer.Group | Where-Object { $_.Status -eq "Incorrect" }).Count
    
    Write-Host "`n📁 $layerName" -ForegroundColor White
    Write-Host "   Total: $total | ✅ $correct | ❌ $missing | ⚠️ $incorrect"
}

# Archivos prioritarios para corregir
Write-Host "`n" + ("="*70) -ForegroundColor $colorInfo
Write-Host "🎯 ARCHIVOS PRIORITARIOS PARA CORREGIR" -ForegroundColor $colorInfo
Write-Host ("="*70) -ForegroundColor $colorInfo

$priority = $results | Where-Object { 
    $_.Status -in @("Missing", "Incorrect") -and 
    $_.Layer -in @("_Core", "Domain", "Infrastructure") 
} | Sort-Object Layer, RelativePath

if ($priority.Count -gt 0) {
    Write-Host "`nSe encontraron $($priority.Count) archivos prioritarios (capas core):`n"
    
    foreach ($file in $priority) {
        $icon = if ($file.Status -eq "Missing") { "❌" } else { "⚠️" }
        Write-Host "$icon $($file.RelativePath)" -ForegroundColor $(
            if ($file.Status -eq "Missing") { $colorError } else { $colorWarning }
        )
        Write-Host "   Actual: $($file.CurrentNamespace)" -ForegroundColor Gray
        Write-Host "   Esperado: $($file.ExpectedNamespace)`n" -ForegroundColor $colorCorrect
    }
}
else {
    Write-Host "`n✅ No hay archivos prioritarios pendientes. ¡Excelente trabajo!" -ForegroundColor $colorCorrect
}

# Exportar a CSV si se solicita
if ($ExportCsv) {
    $csvPath = Join-Path $ProjectPath "..\..\namespace_audit.csv"
    $results | Export-Csv -Path $csvPath -NoTypeInformation -Encoding UTF8
    Write-Host "`n💾 Resultados exportados a: $csvPath" -ForegroundColor $colorInfo
}

Write-Host "`n" + ("="*70) -ForegroundColor $colorInfo
Write-Host "✅ Auditoría completada" -ForegroundColor $colorInfo
Write-Host ("="*70) + "`n" -ForegroundColor $colorInfo

# Return code basado en conformidad
if ($conformancePercentage -lt 70) {
    exit 1  # Fallo crítico
}
elseif ($conformancePercentage -lt 90) {
    exit 2  # Advertencia
}
else {
    exit 0  # Éxito
}
