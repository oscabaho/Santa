# Generates JSON and CSV indexes of classes/interfaces/structs and method signatures under Assets/Scripts
# Enhanced: captures namespace and method return type
# Usage: powershell -NoProfile -ExecutionPolicy Bypass -File tools\generate_class_method_index.ps1

$root = Join-Path $PSScriptRoot '..\Assets\Scripts' | Resolve-Path
$outJson = Join-Path $root 'ACTIVE_FULL.json'
$outCsv = Join-Path $root 'ACTIVE_FULL.csv'

Write-Host "Scanning $root ..."

$files = Get-ChildItem -Path $root -Recurse -Include *.cs | Where-Object { $_.FullName -notmatch '\Samples\' }

$result = [System.Collections.Generic.List[psobject]]::new()

foreach ($f in $files) {
    $content = Get-Content -Raw -Path $f.FullName -ErrorAction SilentlyContinue
    if ([string]::IsNullOrEmpty($content)) { continue }

    $lines = $content -split "\r?\n"

    $classes = [System.Collections.Generic.List[psobject]]::new()
    $methods = [System.Collections.Generic.List[psobject]]::new()
    $currentNamespace = ''

    for ($i=0; $i -lt $lines.Length; $i++) {
        $line = $lines[$i].Trim()
        # capture namespace
        if ($line -match '^namespace\s+([A-Za-z0-9_.]+)') {
            $currentNamespace = $matches[1]
            continue
        }

        # class/interface/struct declaration
        if ($line -match '^(public|internal|protected|private)?\s*(abstract\s+|static\s+)?(class|interface|struct)\s+([A-Za-z0-9_]+)') {
            $kind = $matches[3]
            $name = $matches[4]
            $classes.Add([pscustomobject]@{ name = $name; kind = $kind; line = $i+1; namespace = $currentNamespace })
            continue
        }

        # method-like lines: start with access modifier and contain '('
        # capture return type and name if possible
        if ($line -match '^(public|protected|private|internal|protected\s+internal|private\s+protected)\s+([^\s\(]+(?:\s*<[^>]+>)?\s*(?:[\\]\[\])?)\s+([A-Za-z0-9_]+)\s*\(([^)]*)\)\s*(\{|;)?$') {
            $access = $matches[1]
            $returnType = $matches[2]
            $methodName = $matches[3]
            $params = $matches[4]
            $sig = ($line -replace '\s+', ' ').Trim()
            $methods.Add([pscustomobject]@{ signature = $sig; line = $i+1; returnType = $returnType; methodName = $methodName; namespace = $currentNamespace })
        }
    }

    $entry = [pscustomobject]@{ 
        file = $f.FullName.Substring(($root.Path).Length+1) -replace '\\','/'
        classes = $classes
        methods = $methods
    }
    $result.Add($entry)
}

# Write JSON
$result | ConvertTo-Json -Depth 8 | Out-File -FilePath $outJson -Encoding UTF8
Write-Host "Wrote $outJson"

# Write CSV: flatten entries (with namespace and returnType)
$csvRows = [System.Collections.Generic.List[psobject]]::new()
foreach ($r in $result) {
    $file = $r.file
    if ($r.classes.Count -eq 0 -and $r.methods.Count -eq 0) {
        $csvRows.Add([pscustomobject]@{ file=$file; namespace=''; kind='(none)'; name='(none)'; returnType=''; signature='(none)'; line=''; })
    } else {
        foreach ($c in $r.classes) {
            $csvRows.Add([pscustomobject]@{ file=$file; namespace=$c.namespace; kind=$c.kind; name=$c.name; returnType=''; signature=''; line=$c.line })
        }
        foreach ($m in $r.methods) {
            $csvRows.Add([pscustomobject]@{ file=$file; namespace=$m.namespace; kind='method'; name=$m.methodName; returnType=$m.returnType; signature=$m.signature; line=$m.line })
        }
    }
}

$csvRows | Export-Csv -Path $outCsv -NoTypeInformation -Encoding UTF8
Write-Host "Wrote $outCsv"

Write-Host "Done."