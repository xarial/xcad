param($installPath, $toolsPath, $package, $project)

$project.Object.References | Where-Object { $_.Name -eq "SolidWorks.Interop.swdocumentmgr" } |  ForEach-Object { $_.EmbedInteropTypes = $false }