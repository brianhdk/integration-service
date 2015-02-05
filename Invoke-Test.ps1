$nunit_dir = (Get-ChildItem .\packages\* -Directory | where {$_.Name.StartsWith('NUnit.Runners',[System.StringComparison]::OrdinalIgnoreCase)})[0]
$tests = Get-ChildItem $base_dir -Filter *Tests.csproj -Recurse
$nunit_console = "$nunit_dir\tools\nunit-console.exe"
New-Item -ItemType directory results -Force | Out-Null
foreach($test in $tests) {
    & $nunit_console $test.FullName /config:Debug /framework=v4.0.30319 /domain=multiple /noshadow /nologo /nodots /result="results\$($test.BaseName).xml"
}