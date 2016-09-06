[CmdletBinding()]
$nuget = (Get-Command "nuget").Path

ForEach ($n in (Get-Item "*.nuspec").Name) {
   $success = $true

   try {
      & $nuget "pack" "$n"
   } catch {
      $success = $false
   } finally {
      $okErr = if ($success) {
         "[  okay ]"
      } else {
         "[ error ]"
      }

      Write-Host "$okErr $n"
   }
}
