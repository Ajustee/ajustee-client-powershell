using System.Collections.Generic;
using System.Management.Automation;

namespace Ajustee.Client.PowerShell
{
    [Cmdlet(VerbsCommon.Get, "AjusteeDefaults")]
    [Alias("aj-get-defs")]
    [OutputType(typeof(AjusteeConnectionSettings))]
    public class GetDefaultsCommand : PSCmdlet
    {
        protected override void ProcessRecord()
        {
            WriteObject(this.GetDefaultConnectionSettings() ?? new AjusteeConnectionSettings());
        }
    }
}
