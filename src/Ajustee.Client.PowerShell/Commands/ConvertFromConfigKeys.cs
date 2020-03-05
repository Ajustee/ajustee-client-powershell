using System.Collections.Generic;
using System.Management.Automation;

namespace Ajustee.Client.PowerShell
{
    [Cmdlet(VerbsData.ConvertFrom, "ConfigKeys")]
    [Alias("aj-cfkeys")]
    [OutputType(typeof(PSObject))]
    public class ConvertFromConfigKeys : PSCmdlet
    {
        private List<ConfigKey> m_List;

        [Parameter(Position = 0, Mandatory = true, ValueFromPipeline = true)]
        public object[] ConfigKeys { get; set; }

        [Parameter(Mandatory = false)]
        public string Namespace { get; set; }

        protected override void BeginProcessing()
        {
            m_List = new List<ConfigKey>();
        }

        protected override void ProcessRecord()
        {
            if (ConfigKeys == null) return;

            foreach (var _item in ConfigKeys)
            {
                if (_item is ConfigKey _configKey)
                {
                    m_List.Add(_configKey);
                }
                else if (_item is PSObject _obj && (_configKey = _obj.BaseObject as ConfigKey) != null)
                {
                    m_List.Add(_configKey);
                }
            }
        }

        protected override void EndProcessing()
        {
            if (m_List != null && m_List.Count != 0)
            {
                var _obj = m_List.ToPSObject(Namespace);
                if (_obj != null)
                    WriteObject(_obj);
            }
            m_List = null;
        }
    }
}
