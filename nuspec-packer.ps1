[CmdletBinding()]
Param(
    [Parameter(Mandatory=$true, Position=1)]
    [string] $Version,

    [Parameter(Mandatory=$true, Position=2)]
    [string] $VersionPrefix
)

$toPackArr = @(
   "OpenRA.Game"        = "OpenRA.Game.exe"
   "OpenRA.Mods.Common" = "OpenRA.Mods.Common/bin/Debug/OpenRA.Mods.Common.dll"
   "OpenRA.Mods.RA"     = "OpenRA.Mods.RA/bin/Debug/OpenRA.Mods.RA.dll"
   "OpenRA.Mods.Cnc"    = "OpenRA.Mods.Cnc/bin/Debug/OpenRA.Mods.Cnc.dll"
   "OpenRA.Mods.D2k"    = "OpenRA.Mods.D2k/bin/Debug/OpenRA.Mods.D2k.dll"
)

ForEach ($toPackFile in $toPackArr) {
   $toPackNoExt = [System.IO.Path]::GetFileNameWithoutExtension($toPackFile)
   $ext = [System.IO.Path]::GetExtension($toPackFile).Substring(1)
   $fullFileName = $toPackNoExt + "/bin/Debug/" + $toPackFile

   ./nuspec-replacer.ps1 -Id $toPackNoExt `
     -Filetype $ext `
     -Version $Version `
     -HumanVersion $VersionPrefix `
     -TemplateFilename $fullFileName

   $nuspecFilename = $fullFileName -replace ".nuspec.template",".nuspec"
   Write-Host $nuspecFilename
}
