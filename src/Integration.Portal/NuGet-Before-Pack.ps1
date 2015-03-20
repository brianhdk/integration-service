$script_directory = Split-Path -Parent $PSCommandPath
$base_directory = Resolve-Path $script_directory\..\..
$tools_directory = Join-Path $base_directory "tools"
$zipper = Join-Path $tools_directory "7-Zip\7za.exe"

&$zipper a "Portal.Assets.zip" "Assets/*"
