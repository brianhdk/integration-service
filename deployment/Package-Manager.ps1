$script_directory = Split-Path -Parent $PSCommandPath

$settings = @{
    "src" = @{
        "integration" = Resolve-Path $script_directory\..\src\integration
        "integration_logging_elmah" = Resolve-Path $script_directory\..\src\integration.logging.elmah
    }
    "tools" = @{
        "nuget" = Resolve-Path $script_directory\..\tools\NuGet\nuget.exe
    }
}

foreach ($project in $settings.src.Keys) {

    cd $settings.src[$project]

    # todo: test if we have a .nuspec in this folder - otherwise skip it

    # remove .nupkg files
    Get-ChildItem | Where-Object { $_.Extension -eq ".nupkg" } | Remove-Item

    &$settings.tools.nuget pack -Build -Symbols -Properties Configuration=Release -IncludeReferencedProjects

    Get-ChildItem | Where-Object { $_.Extension -eq ".nupkg" } | Move-Item -Destination "\\nuget.vertica.dk\nuget.vertica.dk\root\Packages" -Force
}

#&$settings.tools.nuget spec

# todo: fortæl hvad der er nyt
# todo: giv nyt versionsnummer
# todo: anvend master branch - og når der er merged fra dev-branch så lav ny release - inkl. release-notes
# todo: readme.txt