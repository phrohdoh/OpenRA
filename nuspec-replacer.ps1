[CmdletBinding()]
Param(
    [Parameter(Mandatory=$true, Position=1)]
    [string] $Id,

    [Parameter(Mandatory=$true, Position=2)]
    [string] $Filetype,

    [Parameter(Mandatory=$true, Position=3)]
    [string] $Version,

    [Parameter(Mandatory=$true, Position=4)]
    [string] $HumanVersion,

    [Parameter(Mandatory=$true, Position=5)]
    [string] $TemplateFilename
)

$fileItem = Get-Item -Path $TemplateFilename
$nuspecFilename = $fileItem.FullName -replace ".nuspec.template",".nuspec"

try {
$fileContents = (Get-Content -Path $TemplateFilename) `
  -replace "{ID}",$Id `
  -replace "{VERSION}",$Version `
  -replace "{HUMAN_FRIENDLY_VERSION}",$HumanVersion `
  -replace "{YEAR}",[DateTime]::UtcNow.Year `
  -replace "{FILETYPE}",$Filetype
} catch [ItemNotFoundException] {
  Write-Host "Could not find $TemplateFilename, aborting."
  exit 1
}

Set-Content -Path $nuspecFilename -Value $fileContents
