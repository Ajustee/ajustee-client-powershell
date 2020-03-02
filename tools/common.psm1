function Get-UserModulesPath
{
    if ( $IsCoreCLR -and -not $IsWindows )
    {
        $platformType = "System.Management.Automation.Platform" -as [Type]
        if ( $platformType )
        {
            ${platformType}::SelectProductNameForDirectory("USER_MODULES")
        }
        else
        {
            throw "Could not determine users module path"
        }
    }
    else
    {
        "${HOME}/Documents/WindowsPowerShell/Modules"
    }
}

function Get-AllUsersModulePath
{
    if ( $IsCoreCLR -and -not $IsWindows )
    {
        $platformType = "System.Management.Automation.Platform" -as [Type]
        if ( $platformType )
        {
            ${platformType}::SelectProductNameForDirectory("USER_MODULES")
        }
        else
        {
            throw "Could not determine users module path"
        }
    }
    else
    {
        "${HOME}/Documents/WindowsPowerShell/Modules"
    }
}
