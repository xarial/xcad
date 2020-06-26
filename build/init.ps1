param($installPath, $toolsPath, $package, $project)

$project.Object.References | Where-Object { $_.Name -eq "SolidWorks.Interop.sldworks" } |  ForEach-Object { $_.EmbedInteropTypes = $false }
$project.Object.References | Where-Object { $_.Name -eq "SolidWorks.Interop.swconst" } |  ForEach-Object { $_.EmbedInteropTypes = $false }
$project.Object.References | Where-Object { $_.Name -eq "SolidWorks.Interop.swpublished" } |  ForEach-Object { $_.EmbedInteropTypes = $false }