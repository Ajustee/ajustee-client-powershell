using System;
using System.Collections;
using System.Collections.Generic;
using System.Management.Automation;

namespace Ajustee.Client.PowerShell
{
    [Cmdlet(VerbsCommon.Get, "ConfigurationKeys")]
    [Alias("aj-get")]
    [OutputType(typeof(IEnumerable<ConfigKey>))]
    public class GetConfigurationKeysCommand : CommandBase
    {
        [Parameter(Mandatory = false)]
        public string Path { get; set; }

        [Parameter(Mandatory = false)]
        public IDictionary Props { get; set; }

        protected override void ProcessRecord()
        {
            base.WriteVerbose($"Path: {Path}");
            base.WriteVerbose($"Properties: {Props}");
            try
            {
                var _configKeys = Client.GetConfigurations(Path, Props.GetStringified());

                WriteObject(_configKeys);
            }
            catch (Exception _ex)
            {
                WriteError(new ErrorRecord(_ex, null, ErrorCategory.WriteError, this));
            }
        }
    }
}
