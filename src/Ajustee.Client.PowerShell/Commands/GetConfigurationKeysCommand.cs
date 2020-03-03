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
        [Parameter(Position = 0, Mandatory = false, ValueFromPipelineByPropertyName = true, ValueFromPipeline = true)]
        public string Path { get; set; }

        [Parameter(Position = 1, Mandatory = false, ValueFromPipelineByPropertyName = true)]
        public object Props { get; set; }

        protected override void ProcessRecord()
        {
            base.WriteVerbose($"Path: {Path}");
            base.WriteVerbose($"Properties: {Props}");
            try
            {
                var _configKeys = Client.GetConfigurations(Path, Props.ToStringDictionary());

                WriteObject(_configKeys);
            }
            catch (Exception _ex)
            {
                WriteError(new ErrorRecord(_ex, null, ErrorCategory.WriteError, this));
            }
        }
    }
}
