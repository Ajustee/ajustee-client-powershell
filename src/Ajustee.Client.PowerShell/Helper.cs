using System;
using System.Collections;
using System.Collections.Generic;
using System.Management.Automation;

namespace Ajustee.Client.PowerShell
{
    internal static class Helper
    {
        private const string m_DefaultAjusteeConnectionSettingsVariableName = "DefaultAjusteeConnectionSettings";

        public static AjusteeConnectionSettings GetDefaultConnectionSettings(this PSCmdlet command)
        {
            return command.SessionState.PSVariable.GetValue(m_DefaultAjusteeConnectionSettingsVariableName) as AjusteeConnectionSettings;
        }

        public static void SetDefaultConnectionSettings(this PSCmdlet command, AjusteeConnectionSettings settings)
        {
            command.SessionState.PSVariable.Set(m_DefaultAjusteeConnectionSettingsVariableName, settings);
        }

        public static IDictionary<string, string> ToStringDictionary(this object source)
        {
            if (source == null)
                return null;

            if (source is IDictionary<string, string> _values)
                return _values;

            if (source is PSObject _obj)
            {
                _values = new Dictionary<string, string>();
                foreach (var _prop in _obj.Properties)
                {
                    if (_prop.MemberType == PSMemberTypes.NoteProperty || _prop.MemberType == PSMemberTypes.Property)
                        _values.Add(_prop.Name, _prop.Value?.ToString());
                }
                return _values;
            }

            if (source is IDictionary _dic)
            {
                _values = new Dictionary<string, string>();
                foreach (DictionaryEntry _entry in _dic)
                {
                    if (_entry.Key != null)
                        throw new ArgumentException("Invalid key of the properties", nameof(source));

                    _values.Add(_entry.Key.ToString(), _entry.Value?.ToString());
                }

                return _values;
            }

            throw new NotSupportedException("Not supported value type of the properties");
        }
    }
}
