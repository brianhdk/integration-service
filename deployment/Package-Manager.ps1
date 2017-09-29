[CmdletBinding()]
Param(
	[Parameter(Mandatory=$false)]
	[ValidateSet("Local", "Remote")]
	[string]$target = "Local",

	[Parameter(Mandatory=$false)]
	[string]$nugetApiKey = $null,

	[Parameter(Mandatory=$false)]
	[string]$nugetSource = $null
)

$ErrorActionPreference = "Stop"
$script_directory = Split-Path -Parent $PSCommandPath

$settings = @{
    "src" = @{
        "integration" = Resolve-Path $script_directory\..\src\Integration
        "integration_host" = Resolve-Path $script_directory\..\src\Integration.Host
		"integration_consolehost" = Resolve-Path $script_directory\..\src\Integration.ConsoleHost
        "integration_webhost" = Resolve-Path $script_directory\..\src\Integration.WebHost
		"integration_webapi" = Resolve-Path $script_directory\..\src\Integration.WebApi
		"integration_webapi_signalr" = Resolve-Path $script_directory\..\src\Integration.WebApi.SignalR
		"integration_webapi_nswag" = Resolve-Path $script_directory\..\src\Integration.WebApi.NSwag
        "integration_portal" = Resolve-Path $script_directory\..\src\Integration.Portal
		"integration_logging_elmah" = Resolve-Path $script_directory\..\src\Integration.Logging.Elmah
		"integration_azure" = Resolve-Path $script_directory\..\src\Integration.Azure
		#"integration_paymentservice" = Resolve-Path $script_directory\..\src\Integration.PaymentService
		"integration_ravendb" = Resolve-Path $script_directory\..\src\Integration.RavenDB
		"integration_mongodb" = Resolve-Path $script_directory\..\src\Integration.MongoDB
		"integration_rebus" = Resolve-Path $script_directory\..\src\Integration.Rebus
		"integration_sqlite" = Resolve-Path $script_directory\..\src\Integration.SQLite
		"integration_perfion" = Resolve-Path $script_directory\..\src\Integration.Perfion
        "integration_hangfire" = Resolve-Path $script_directory\..\src\Integration.Hangfire
		"integration_globase" = Resolve-Path $script_directory\..\src\Integration.Globase
        "integration_slack" = Resolve-Path $script_directory\..\src\Integration.Slack
        "integration_ucommerce" = Resolve-Path $script_directory\..\src\Integration.UCommerce
        "integration_elasticsearch" = Resolve-Path $script_directory\..\src\Integration.Elasticsearch
		"integration_redis" = Resolve-Path $script_directory\..\src\Integration.Redis
		"integration_iis" = Resolve-Path $script_directory\..\src\Integration.IIS
    }
    "tools" = @{
        "nuget" = Resolve-Path $script_directory\..\.nuget\NuGet.exe
    }
}

# remove .nupkg files in script directory
Get-ChildItem $script_directory | Where-Object { $_.Extension -eq ".nupkg" } | Remove-Item

foreach ($project in $settings.src.Keys) {

	Write-Host
	Write-Host "----------------------------------" -ForegroundColor Green
	Write-Host "--- $project" -ForegroundColor Green
	Write-Host "----------------------------------" -ForegroundColor Green
	Write-Host

    $projectDirectory = Resolve-Path $settings.src[$project]

    cd $projectDirectory

    # remove .nupkg files in project directory
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
    &$settings.tools.nuget pack $csproj -Verbosity "quiet" -Build -Properties Configuration=Release -IncludeReferencedProjects -Exclude "Assets/**/*.*" -Exclude "Default.html"
	
	if (Test-Path (Join-Path $projectDirectory "NuGet-After-Pack.ps1")) {
		
		$afterPack = Join-Path $projectDirectory "NuGet-After-Pack.ps1"
	
		Invoke-Expression $afterPack
	}

    # Move .nupkg to script directory
    Get-ChildItem $projectDirectory | Where-Object { $_.Extension -eq ".nupkg" } | Move-Item -Destination $script_directory -Force
}

Get-ChildItem $script_directory | Where-Object { $_.Extension -eq ".nupkg" } | ForEach {

	If ($target -eq "Remote") {

		if ($nugetApiKey -eq $null) {
			throw "Missing mandatory value for parameter 'nugetApiKey'."
		}

		if ($nugetSource -eq $null) {
			throw "Missing mandatory value for parameter 'nugetSource'."
		}

        Try {

		    &$settings.tools.nuget push $_.FullName $nugetApiKey -Source $nugetSource
        }
        Catch {

            $message = $_.Exception.Message
            
            If ($message.Contains("already exist") -eq $true) {

                Write-Host "WARNING: $message" -ForegroundColor Yellow
            }
            Else {

                Throw $_.Exception
            }
        }
	}
    Else {

		Move-Item -Path $_.FullName -Destination "C:\Users\bhk\Dropbox\Development\NuGet.Packages" -Force
	}
}

cd $script_directory