[CmdletBinding()]
Param(
    [Parameter(Mandatory=$true, Position=1)]
    [string] $Id,

    [Parameter(Mandatory=$true, Position=2)]
    [string] $Filetype,

    [Parameter(Mandatory=$true, Position=3)]
    [string] $VersionPrefix,

    [Parameter(Mandatory=$true, Position=4)]
    [string] $Version,

    [Parameter(Mandatory=$true, Position=5)]
    [string] $NuspecTemplateFilename,

    [Parameter(Mandatory=$false, Position=6)]
    [int] $PlaytestVersion = 0,

    [Parameter(Mandatory=$false, Position=7)]
    [int] $HotfixVersion = 0
)

$fileItem = Get-Item -Path $NuspecTemplateFilename -ErrorAction SilentlyContinue

if ($fileItem -eq $null) {
  Write-Host "[ error ] $NuspecTemplateFilename does not exist, aborting."
  exit 1
}

$outputFilename = $fileItem.FullName -replace ".nuspec.template",".nuspec"

$replacedContents = (Get-Content -Path $NuspecTemplateFilename) `
  -replace "{ID}",$Id `
  -replace "{VERSION_PREFIX}",$VersionPrefix `
  -replace "{VERSION}",$Version `
  -replace "{YEAR}",[DateTime]::UtcNow.Year `
  -replace "{FILETYPE}",$Filetype `
  -replace "{PLAYTEST_VERSION}",$PlaytestVersion `
  -replace "{HOTFIX_VERSION}",$HotfixVersion

Set-Content -Path $outputFilename -Value $replacedContents
Write-Host "[  okay ] $outputFilename"
