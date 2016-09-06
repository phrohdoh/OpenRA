[CmdletBinding()]
Param(
    [Parameter(Mandatory=$true, Position=1)]
    [string] $VersionPrefix,

    [Parameter(Mandatory=$true, Position=2)]
    [string] $Version,

    [Parameter(Mandatory=$false, Position=3)]
    [int] $PlaytestVersion = 0,

    [Parameter(Mandatory=$false, Position=4)]
    [int] $HotfixVersion = 0
)

$toPackNames = @(
   "OpenRA.Game.exe"
   "OpenRA.Mods.Common.dll"
   "OpenRA.Mods.RA.dll"
   "OpenRA.Mods.Cnc.dll"
   "OpenRA.Mods.D2k.dll"
)

For ($i = 0; $i -lt $toPackNames.Length; $i++) {
   $name = $toPackNames[$i]

   $ext = [System.IO.Path]::GetExtension($name)
   $name = [System.IO.Path]::GetFileNameWithoutExtension($name)

   $template = "$name/$name.nuspec.template"
   & "./GenerateNuspec-FromTemplate.ps1" `
     -Id $name `
     -Filetype $ext `
     -VersionPrefix $VersionPrefix `
     -Version $Version `
     -NuspecTemplateFilename $template `
     -PlaytestVersion $PlaytestVersion `
     -HotfixVersion $HotfixVersion
}
