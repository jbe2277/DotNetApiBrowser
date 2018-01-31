$BinariesFolder = "..\out\Release"
$PublishFolder = "..\..\DotNetApiBrowserPages\clickonce\"


### Helper functions

function Format-Xml([xml]$xml)
{
    $stringWriter = New-Object System.IO.StringWriter;
    $xmlWriter = New-Object System.Xml.XmlTextWriter $stringWriter;
    $xmlWriter.Formatting = "indented";
    $xml.WriteTo($xmlWriter);
    Write-Output $stringWriter.ToString();
    $xmlWriter.Dispose();
    $stringWriter.Dispose();
}

function Get-FileHash-Sha256([String] $FileName)
{
    Add-Type -Assembly System.Security
    $sha = New-Object System.Security.Cryptography.SHA256Managed
    $fileStream = [System.IO.File]::Open($FileName, "open", "read")
    $bytes = $sha.ComputeHash($fileStream)
    $fileStream.Dispose()
    $base64Hash = [System.Convert]::ToBase64String($bytes)
    return $base64Hash
}


### Main script

$BinariesFolder = "$PSScriptRoot\$BinariesFolder"
$PublishFolder = "$PSScriptRoot\$PublishFolder"

$AppFilesSubFolders = Get-ChildItem -Path "$PublishFolder\Application Files\*" -Directory
iF ($ExeList.Count -gt 1) {
	Write-Error "Error: Multiple folders found in Application Files."
	exit 101;
}
$PublishFolderAppFiles = $AppFilesSubFolders[0]

$ExeManifestFiles = Get-ChildItem -Path "$PublishFolderAppFiles\*.manifest"
$ExeManifestFile = $ExeManifestFiles[0]
Write-Host "> Manifest: $ExeManifestFile"

$AppManifestFiles = Get-ChildItem -Path "$PublishFolderAppFiles\*.application"
$AppManifestFile = $AppManifestFiles[0]
Write-Host "> Application: $AppManifestFile"

[string[]] $DllListBinariesFolder = Get-ChildItem -Path "$BinariesFolder\*.dll"
[string[]] $DllListPublishFolder = Get-ChildItem -Path "$PublishFolderAppFiles\*.dll.deploy"


$MissingDllFiles = [Linq.Enumerable]::Where($DllListBinariesFolder, 
    [Func[string,bool]] { param($x); return [Linq.Enumerable]::All($DllListPublishFolder, 
        [Func[string,bool]] { param($y); return -Join((Split-Path $x -Leaf), ".deploy") -ne (Split-Path $y -Leaf) }) })
$MissingDllFiles | % { Copy-Item -Path $_ -Destination (-Join("$PublishFolderAppFiles\", (Split-Path $_ -Leaf), ".deploy")) }


[xml] $ExeManifest = Get-Content $ExeManifestFile
$LastDependency = $ExeManifest.assembly.dependency | Where { $_.dependentAssembly.dependencyType -eq "install" } | Select-Object -Last 1
$MissingDllFiles | % { 
    $NewDependency = $LastDependency.Clone()
    $NewDependency.dependentAssembly.codebase=[string](Split-Path $_ -Leaf)
    $NewDependency.dependentAssembly.size = (Get-Item $_).Length.ToString()
    $NewDependency.dependentAssembly.assemblyIdentity.name = [io.path]::GetFileNameWithoutExtension([string](Split-Path $_ -Leaf))
    $NewDependency.dependentAssembly.assemblyIdentity.version = ([Reflection.AssemblyName]::GetAssemblyName($_).Version).ToString()
    $NewDependency.dependentAssembly.hash.DigestValue = Get-FileHash-Sha256($_)
    [void]$ExeManifest.assembly.InsertAfter($NewDependency, $LastDependency)
    $LastDependency = $NewDependency
}
$ExeManifest.Save($ExeManifestFile)


[xml] $AppManifest = Get-Content $AppManifestFile
$AppManifest.assembly.dependency.dependentAssembly.hash.DigestValue = Get-FileHash-Sha256($ExeManifestFile)
$AppManifest.Save($AppManifestFile)


Write-Host "> Finish. Added $($MissingDllFiles.Length) files."