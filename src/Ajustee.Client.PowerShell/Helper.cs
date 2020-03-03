using System;
using System.Collections;
using System.Collections.Generic;
using System.Management.Automation;
using Ajustee;

namespace Ajustee.Client.PowerShell
{
    internal static class Helper
    {
        private const string m_DefaultAjusteeConnectionSettingsVariableName = "DefaultAjusteeConnectionSettings";
        private const string m_NamespaceSeparator = "/";

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

        public static string NormalizeNamespace(string ns)
        {
            if (string.IsNullOrWhiteSpace(ns)) return string.Empty;
            if (ns.EndsWith(m_NamespaceSeparator)) return ns ?? string.Empty;
            return ns + m_NamespaceSeparator;
        }

        public static bool IsNamespaceMatch(this ConfigKey configKey, string ns)
        {
            ns = NormalizeNamespace(ns);
            if (ns == string.Empty) return false;
            return configKey.Path?.StartsWith(ns) == true;
        }

        public static IEnumerable<string> GetPropertyNames(this ConfigKey configKey, string ns)
        {
            var _path = configKey.Path;
            if (_path == null) yield break;
            
            ns = NormalizeNamespace(ns);
            if (ns != string.Empty && _path.StartsWith(ns))
                _path = _path.Substring(ns.Length);

            foreach (var _prop in _path.Split(new[] { m_NamespaceSeparator }, StringSplitOptions.RemoveEmptyEntries))
                yield return _prop;
        }

        public static void AddToObject(PSObject obj, ConfigKey configKey, string ns)
        {
            configKey.Value<object>();
            var _obj = new PSObject();
            _obj.Properties.Add(new PSNoteProperty("prop1", 12));
            var _subObj = new PSObject();
            _subObj.Properties.Add(new PSNoteProperty("prop3", "value"));
            _obj.Properties.Add(new PSNoteProperty("prop2", _subObj));
        }
    }
}
