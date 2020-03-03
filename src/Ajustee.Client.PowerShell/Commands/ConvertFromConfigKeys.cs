using System.Collections.Generic;
using System.Management.Automation;

namespace Ajustee.Client.PowerShell
{
    [Cmdlet(VerbsData.ConvertFrom, "ConfigKeys")]
    [Alias("aj-cfkeys")]
    [OutputType(typeof(PSObject))]
    public class ConvertFromConfigKeys : PSCmdlet
    {
        [Parameter(Position = 0, Mandatory = true, ValueFromPipeline = true)]
        public object[] ConfigKeys { get; set; }

        [Parameter(Mandatory = false)]
        public string Namespace { get; set; }

        protected override void ProcessRecord()
        {
            if (ConfigKeys == null) return;

            foreach (var _item in ConfigKeys)
            {
                if (_item is ConfigKey _configKey)
                {
                   WriteObject(_configKey);
                }
                else if (_item is PSObject _obj && (_configKey = _obj.BaseObject as ConfigKey) != null)
                {
                    WriteObject(_configKey);
                }
            }
        }

        protected override void EndProcessing()
        {
            base.EndProcessing();
        }
    }
}
