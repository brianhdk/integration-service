$ErrorActionPreference = "Stop"
$script_directory = Split-Path -Parent $PSCommandPath

$settings = @{
    "src" = @{
        "integration" = Resolve-Path $script_directory\..\src\integration
        "integration_portal" = Resolve-Path $script_directory\..\src\integration.portal
		"integration_logging_elmah" = Resolve-Path $script_directory\..\src\integration.logging.elmah
    }
    "tools" = @{
        "nuget" = Resolve-Path $script_directory\..\tools\NuGet\nuget.exe
    }
	"packages" = @{
		#"destination" = "\\nuget.vertica.dk\nuget.vertica.dk\root\Packages"
		"destination" = "D:\Dropbox\Development\NuGet.Packages"
	}
}

foreach ($project in $settings.src.Keys) {

    cd $settings.src[$project]

    # remove .nupkg files
    Get-ChildItem | Where-Object { $_.Extension -eq ".nupkg" } | Remove-Item

	if (Test-Path "NuGet-Before-Pack.ps1") {
		
		$beforePack = Join-Path $settings.src[$project] "NuGet-Before-Pack.ps1"

		Invoke-Expression $beforePack
	}

	# https://docs.nuget.org/consume/command-line-reference
    &$settings.tools.nuget pack -Build -Symbols -Properties Configuration=Release -IncludeReferencedProjects -Exclude "Assets/**/*.*" -Exclude "Default.html"

	if (Test-Path "NuGet-After-Pack.ps1") {
		
		$afterPack = Join-Path $settings.src[$project] "NuGet-After-Pack.ps1"

		Invoke-Expression $afterPack
	}

    Get-ChildItem | Where-Object { $_.Extension -eq ".nupkg" } | Move-Item -Destination $settings.packages.destination -Force
}

cd $script_directory

#&$settings.tools.nuget spec

# todo: fortæl hvad der er nyt
# todo: giv nyt versionsnummer
# todo: anvend master branch - og når der er merged fra dev-branch så lav ny release - inkl. release-notes
# todo: readme.txt