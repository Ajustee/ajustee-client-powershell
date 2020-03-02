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
    Import-Module -Force (Join-Path $PSScriptRoot build.psm1)

    if ($PSVersion -gt 6) {
        # due to netstandard2.0 we do not need to treat PS version 7 differently
        $PSVersion = 6
    }
}
end
{
    $setName = $PSCmdlet.ParameterSetName
    switch ( $setName ) {
        "Build" {
            Start-Build -Development:$Development -Configuration $Configuration
        }
        "Documentation" {
            Start-Build -Documentation
        }
        "Test" {
            Start-Test -Test:$Test -InProcess:$InProcess
            return
        }
        default {
            throw "Unexpected parameter set '$setName'"
        }
    }
}
