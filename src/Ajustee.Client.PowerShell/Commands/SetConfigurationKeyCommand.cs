using System;
using System.Collections.Generic;
using System.Management.Automation;

namespace Ajustee.Client.PowerShell
{
    [Cmdlet(VerbsCommon.Set, "ConfigurationKey")]
    [Alias("aj-set")]
    public class SetConfigurationKeyCommand : ClientCommandBase
    {
        [Parameter(Mandatory = true, Position = 0, ValueFromPipelineByPropertyName = true)]
        public string Path { get; set; }

        [Parameter(Mandatory = true, Position = 1, ValueFromPipelineByPropertyName = true)]
        public string Value { get; set; }

        protected override void ProcessRecord()
        {
            base.WriteVerbose($"Path: {Path}");
            base.WriteVerbose($"Value: {Value}");
            try
            {
                Client.Update(Path, Value);
            }
            catch (Exception _ex)
            {
                WriteError(new ErrorRecord(_ex, null, ErrorCategory.WriteError, this));
            }
        }
    }
}
