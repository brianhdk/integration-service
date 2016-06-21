[CmdletBinding()]
Param(
	[Parameter(Mandatory=$false)]
	[ValidateSet("Local", "Remote")]
	[string]$target = "Local"
)

function Get-Target() {

    if ($target -eq "Remote") {

		Return "\\nuget.vertica.dk\nuget.vertica.dk\root\Packages"
	}

	Return "D:\Dropbox\Development\NuGet.Packages"
}

$ErrorActionPreference = "Stop"
$script_directory = Split-Path -Parent $PSCommandPath

$settings = @{
    "src" = @{
        "integration" = Resolve-Path $script_directory\..\src\Integration
        "integration_host" = Resolve-Path $script_directory\..\src\Integration.Host
		"integration_webapi" = Resolve-Path $script_directory\..\src\Integration.WebApi
		"integration_webapi_signalr" = Resolve-Path $script_directory\..\src\Integration.WebApi.SignalR
        "integration_portal" = Resolve-Path $script_directory\..\src\Integration.Portal
		"integration_logging_elmah" = Resolve-Path $script_directory\..\src\Integration.Logging.Elmah
		"integration_azure" = Resolve-Path $script_directory\..\src\Integration.Azure
		"integration_paymentservice" = Resolve-Path $script_directory\..\src\Integration.PaymentService
		"integration_ravendb" = Resolve-Path $script_directory\..\src\Integration.RavenDB
		"integration_mongodb" = Resolve-Path $script_directory\..\src\Integration.MongoDB
		"integration_rebus" = Resolve-Path $script_directory\..\src\Integration.Rebus
		"integration_sqlite" = Resolve-Path $script_directory\..\src\Integration.SQLite
		"integration_perfion" = Resolve-Path $script_directory\..\src\Integration.Perfion
        "integration_hangfire" = Resolve-Path $script_directory\..\src\Integration.Hangfire
		"integration_globase" = Resolve-Path $script_directory\..\src\Integration.Globase
    }
    "tools" = @{
        "nuget" = Resolve-Path $script_directory\..\.nuget\NuGet.exe
    }
	"packages" = @{
		"destination" = Get-Target
		#"destination" = "D:\Dropbox\Development\NuGet.Packages"
	}
}

foreach ($project in $settings.src.Keys) {

    $projectDirectory = Resolve-Path $settings.src[$project]

    cd $projectDirectory

    # remove .nupkg files
    Get-ChildItem $projectDirectory | Where-Object { $_.Extension -eq ".nupkg" } | Remove-Item

	if (Test-Path (Join-Path $projectDirectory "NuGet-Before-Pack.ps1")) {
		
		$beforePack = Join-Path $projectDirectory "NuGet-Before-Pack.ps1"

		Invoke-Expression $beforePack
	}

	# https://docs.nuget.org/create/nuspec-reference
	$csproj = Get-ChildItem $projectDirectory | Where-Object { $_.Extension -eq ".csproj" }[0]

    if ($csproj -eq $null) {
        throw "No .csproj file found in $projectDirectory."
    }

	# https://docs.nuget.org/consume/command-line-reference
    &$settings.tools.nuget pack $csproj -Build -Properties Configuration=Release -IncludeReferencedProjects -Exclude "Assets/**/*.*" -Exclude "Default.html" -MSBuildVersion 14
	
	if (Test-Path (Join-Path $projectDirectory "NuGet-After-Pack.ps1")) {
		
		$afterPack = Join-Path $projectDirectory "NuGet-After-Pack.ps1"
	
		Invoke-Expression $afterPack
	}
	
    Get-ChildItem $projectDirectory | Where-Object { $_.Extension -eq ".nupkg" } | Move-Item -Destination $settings.packages.destination -Force
}

cd $script_directory