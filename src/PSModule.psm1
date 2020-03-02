#
# Script module for module 'Ajustee.Client'
#
Set-StrictMode -Version Latest

# Set up some helper variables to make it easier to work with the module
$PSModule = $ExecutionContext.SessionState.Module
$PSModuleRoot = $PSModule.ModuleBase

# Import the appropriate nested binary module based on the current PowerShell version
$binaryModuleRoot = $PSModuleRoot

if (($PSVersionTable.Keys -contains "PSEdition") -and ($PSVersionTable.PSEdition -ne 'Desktop')) {
    $binaryModuleRoot = Join-Path -Path $PSModuleRoot -ChildPath 'coreclr'
    [Version] $minimumPowerShellCoreVersion = '6.2.1'
    if ($PSVersionTable.PSVersion -lt $minimumPowerShellCoreVersion) {
        throw "Minimum supported version of PSScriptAnalyzer for PowerShell Core is $minimumPowerShellCoreVersion but current version is '$($PSVersionTable.PSVersion)'. Please update PowerShell Core."
    }
}
else {
    $binaryModuleRoot = Join-Path -Path $PSModuleRoot -ChildPath "clr"
    Add-Type -Path "$binaryModuleRoot/Ajustee.Client.dll"
}

# Initializes a global variables
$Global:DefaultAjusteeConnectionSettings = [Ajustee.AjusteeConnectionSettings]::new()
#$Global:DefaultAjusteeConnectionSettings.ApiUrl = ""

# Imports the module
$binaryModulePath = Join-Path -Path $binaryModuleRoot -ChildPath 'Ajustee.Client.PowerShell.dll'
$binaryModule = Import-Module -Name $binaryModulePath -PassThru

# When the module is unloaded, remove the nested binary module that was loaded with it
$PSModule.OnRemove = {
    Remove-Module -ModuleInfo $binaryModule
}
