Set-StrictMode -Version Latest
#region script variables

$script:ModuleName = 'Ajustee.Client'
$script:ModuleBinaryName = 'Ajustee.Client.PowerShell'
$script:IsInbox = $PSHOME.EndsWith('\WindowsPowerShell\v1.0', [System.StringComparison]::OrdinalIgnoreCase)
$script:IsWindows = (-not (Get-Variable -Name IsWindows -ErrorAction Ignore)) -or $IsWindows
$script:IsCoreCLR = $PSVersionTable.ContainsKey('PSEdition') -and $PSVersionTable.PSEdition -eq 'Core'

$script:ProjectRoot = Split-Path -Path $PSScriptRoot -Parent
$script:ModuleRoot = Join-Path -Path $ProjectRoot -ChildPath "src"
$script:ModuleFile = Join-Path -Path $ModuleRoot -ChildPath "$($script:ModuleName).psm1"
$script:ArtifactRoot = Join-Path -Path $ProjectRoot -ChildPath "dist"
$script:ToolsRoot = Join-Path -Path $script:ProjectRoot -ChildPath "tools"

$script:VersionSuffix = @{ Prod = "" } + @{ Dev = ".9999" }
$script:DefaultApiUrl = @{ Prod = "https://api.ajustee.com/" } + @{ Dev = "https://i3q7cdnjlc.execute-api.us-west-2.amazonaws.com/fo" }

if ($script:IsInbox) {
    $script:ProgramFilesPSPath = Microsoft.PowerShell.Management\Join-Path -Path $env:ProgramFiles -ChildPath "WindowsPowerShell"
}
elseif ($script:IsCoreCLR) {
    if ($script:IsWindows) {
        $script:ProgramFilesPSPath = Microsoft.PowerShell.Management\Join-Path -Path $env:ProgramFiles -ChildPath 'PowerShell'
    }
    else {
        $script:ProgramFilesPSPath = Split-Path -Path ([System.Management.Automation.Platform]::SelectProductNameForDirectory('SHARED_MODULES')) -Parent
    }
}

$script:TempPath = [System.IO.Path]::GetTempPath()
$script:ProgramFilesModulesPath = Microsoft.PowerShell.Management\Join-Path -Path $script:ProgramFilesPSPath -ChildPath "Modules"
$script:AllUsersModulesPath = $script:ProgramFilesModulesPath

# AppVeyor.yml sets a value to $env:PowerShellEdition variable,
# otherwise set $script:PowerShellEdition value based on the current PowerShell Edition.
$script:PowerShellEdition = [System.Environment]::GetEnvironmentVariable("PowerShellEdition")
if (-not $script:PowerShellEdition) {
    if ($script:IsCoreCLR) {
        $script:PowerShellEdition = 'Core'
    }
    else {
        $script:PowerShellEdition = 'Desktop'
    }
}
elseif (($script:PowerShellEdition -eq 'Core') -and ($script:IsWindows)) {
    # In AppVeyor test runs, OneGet and PSGet modules are installed from Windows PowerShell process
    # Set AllUsersModulesPath to $env:ProgramFiles\PowerShell\Modules
    $AllUsersModulesPath = Join-Path -Path $env:ProgramFiles -ChildPath 'PowerShell' | Join-Path -ChildPath 'Modules'
}
Write-Host "PowerShellEdition value: $script:PowerShellEdition"

# Load build manifest.
$script:BuildManifest = Get-Content -Path (Join-Path -Path $ToolsRoot -ChildPath "build.json") | ConvertFrom-Json

#endregion script variables

Import-Module $PSScriptRoot\common.psm1

function Start-Build
{
    [CmdletBinding(DefaultParameterSetName="Build")]
    param
    (
        [Parameter(ParameterSetName="Build")]
        [ValidateSet("Debug", "Release")]
        [string]$Configuration = "Debug",

        [Parameter(ParameterSetName="Build")]
        [switch]$Development,

        # For building documentation only or re-building it since docs gets built automatically only the first time
        [Parameter(ParameterSetName="Documentation")]
        [switch]$Documentation,

        [Parameter(Mandatory=$true,ParameterSetName='Test')]
        [switch] $Test,

        [Parameter(ParameterSetName='Test')]
        [switch] $InProcess
    )

    begin
    {
        if (-not $Development) {
            $Configuration = "Release"
        }

        DotNetInstall
    }

    process
    {
        $setName = $PSCmdlet.ParameterSetName
        switch ( $setName ) {
            "Build" {
                DotNetBuild -Configuration $Configuration
                Publish-ModuleArtifacts -Configuration $Configuration -Development:$Development
                Install-PublishedModule -Development:$Development
            }
            "Documentation" {
                # Generate documentation here
            }
            "Test" {
                # test here
            }
            default {
                throw "Unexpected parameter set '$setName'"
            }
        }
    }
}

function DotNetInstall {
}

function DotNetBuild {
    param (
        [string]$Configuration
    )
    Push-Location -Path $ProjectRoot
    & dotnet build --configuration $Configuration -p:TreatWarningsAsErrors=True
    Pop-Location
}

function Update-ModuleManifest {
    param (
        [switch]$Development
    )
    if ($Development) {
        Write-Verbose -Message "Local dev installation specified."
    }

    # Module
    $moduleFullName = Join-Path -Path $ArtifactRoot (Join-Path -Path $ModuleName -ChildPath "$ModuleName.psm1")
    $rawModule = Get-Content -Path $moduleFullName -Raw
    $rawModule = $rawModule -replace "\#(\`$Global\:DefaultAjusteeConnectionSettings\.ApiUrl\s*=\s*)""""", "`$1""$(if ($Development) { $DefaultApiUrl.Dev } else { $DefaultApiUrl.Prod })"""
    Set-Content -Path $moduleFullName -Value $rawModule

    # Manifest
    $manifestFullName = Join-Path -Path $ArtifactRoot (Join-Path -Path $ModuleName -ChildPath "$ModuleName.psd1")
    $moduleInfo = Test-ModuleManifest $manifestFullName
    $rawManifest = Get-Content -Path $manifestFullName -Raw
    $rawManifest = $rawManifest -replace "(?<=RootModule\s*=\s*').*?(?=')", "$($ModuleName).psm1"
    $rawManifest = $rawManifest -replace "(?<=TypesToProcess\s*=\s*@\().*?(?=\))", "'$($ModuleName).types.ps1xml'"
    $rawManifest = $rawManifest -replace "(?<=FormatsToProcess\s*=\s*@\().*?(?=\))", "'$($ModuleName).format.ps1xml'"
    $rawManifest = $rawManifest -replace "(?<=ModuleVersion\s*=\s*').*?(?=')", "$($moduleInfo.Version)$(if ($Development) { $VersionSuffix.Dev } else { $VersionSuffix.Prod })"
    Set-Content -Path $manifestFullName -Value $rawManifest
}

function Publish-ModuleArtifacts {
    param (
        [string]$Configuration,
        [switch]$Development
    )

    # Test artifact root and forced remove
    if (Test-Path -Path $ArtifactRoot) {
        Remove-Item -Path $ArtifactRoot -Recurse -Force
    }

    # Creates destination artifact folder.
    $destModuleRoot = Join-Path -Path $ArtifactRoot -ChildPath $ModuleName
    New-Item -Path $destModuleRoot -ItemType Directory | Out-Null

    # Copy the module into the artifact root
    foreach ($item in $BuildManifest.modulefiles) {
        Copy-Item -Path (Join-Path -Path $ModuleRoot -ChildPath $item) -Destination (Join-Path -Path $destModuleRoot -ChildPath ($item -replace "PSModule", $ModuleName))
    }

    # Copy the framework dependencies into the artifact root
    foreach ($cmdlet in $BuildManifest.cmdlets) {

        $srcDepsFolder = Join-Path $ModuleRoot "$ModuleBinaryName\bin\$Configuration\$($cmdlet.framework)"
        $destTargetFolder = Join-Path -Path $destModuleRoot -ChildPath $cmdlet.target

        # Creates destination target folder.
        New-Item -Path $destTargetFolder -ItemType Directory | Out-Null

        # Copy framework dependency files.
        foreach ($file in $cmdlet.files) {
            Copy-Item -Path (Join-Path $srcDepsFolder -ChildPath $file) -Destination $destTargetFolder
        }
    }

    # Updates manifest file
    Update-ModuleManifest -Development:$Development

    # Package the module in /dist
    $zipFileName = "$ModuleName.zip"
    $artifactZipFile = Join-Path -Path $ArtifactRoot -ChildPath $zipFileName
    $tempZipfile = Join-Path -Path $TempPath -ChildPath $zipFileName

    if ($PSEdition -ne 'Core') {
        Add-Type -AssemblyName System.IO.Compression.FileSystem
    }

    if (Test-Path -Path $tempZipfile) {
        Remove-Item -Path $tempZipfile -Force
    }

    Write-Verbose "Zipping module artifacts in $ArtifactRoot"
    [System.IO.Compression.ZipFile]::CreateFromDirectory($ArtifactRoot, $tempZipfile)

    Move-Item -Path $tempZipfile -Destination $artifactZipFile -Force
}

function Install-PublishedModule {

    $moduleFolder = Join-Path -Path $ArtifactRoot -ChildPath $ModuleName
    $moduleInfo = Test-ModuleManifest (Join-Path -Path $moduleFolder -ChildPath "$ModuleName.psd1")
    $installLocation = Join-Path -Path (Get-UserModulesPath) -ChildPath $ModuleName

    if (($PowerShellEdition -eq 'Core') -or ($PSVersionTable.PSVersion -ge '5.0.0')) {
        $installLocation = Join-Path $installLocation $moduleInfo.Version
    }
    New-Item -Path $installLocation -ItemType Directory -Force | Out-Null
    Copy-Item -Path "$moduleFolder\*" -Destination $InstallLocation -Recurse -Force

    Write-Verbose -Message "Copied module artifacts from $moduleFolder merged module artifact to`n$installLocation"
}