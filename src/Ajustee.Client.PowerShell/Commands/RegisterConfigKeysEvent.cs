using System;
using System.Management.Automation;

namespace Ajustee.Client.PowerShell
{
    [Cmdlet(VerbsLifecycle.Register, "ConfigKeysEvent")]
    [Alias("aj-rgch")]
    public class RegisterConfigKeysEvent : ClientEventBase
    {

        [Parameter(Position = 0, Mandatory = false, ValueFromPipeline = true)]
        public string Path { get; set; }

        [Parameter(Position = 1, Mandatory = false, ValueFromPipelineByPropertyName = true)]
        public object Props { get; set; }

        protected override void ProcessRecord()
        {
            base.WriteVerbose($"Path: {Path}");
            base.WriteVerbose($"Properties: {Props}");
            try
            {
                Client.Subscribe(Path);
            }
            catch (Exception _ex)
            {
                WriteError(new ErrorRecord(_ex, null, ErrorCategory.WriteError, this));
            }
        }
    }
}
