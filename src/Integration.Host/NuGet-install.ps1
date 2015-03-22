param($installPath, $toolsPath, $package, $project)

$files = "Tasks.Core.config", "Tasks.config"

foreach ($file in $files) {

	$item = $project.ProjectItems.Item($file)

	# set 'Copy To Output Directory' to 'Copy if newer'
	$copy = $item.Properties.Item("CopyToOutputDirectory")
	$copy.Value = 2
}