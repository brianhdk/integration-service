param($installPath, $toolsPath, $package, $project)

$item = $project.ProjectItems.Item("Portal.Assets.zip")

# set 'Copy To Output Directory' to 'Copy if newer'
$copy = $item.Properties.Item("CopyToOutputDirectory")
$copy.Value = 2