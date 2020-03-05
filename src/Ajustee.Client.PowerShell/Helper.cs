using System;
using System.Collections;
using System.Collections.Generic;
using System.Management.Automation;
using System.Diagnostics;

namespace Ajustee.Client.PowerShell
{
    internal static class Helper
    {
        private const string m_DefaultAjusteeConnectionSettingsVariableName = "DefaultAjusteeConnectionSettings";
        private const char m_NamespaceSeparator = '/';

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
            if (ns[0] == m_NamespaceSeparator)
            {
                ns = ns.TrimStart(m_NamespaceSeparator);
                if (string.IsNullOrWhiteSpace(ns)) return string.Empty;
            }
            if (ns[ns.Length - 1] == m_NamespaceSeparator) return ns;
            return ns + m_NamespaceSeparator;
        }

        public static bool RemoveNamespace(ref string path, string ns)
        {
            if (string.IsNullOrEmpty(path)) return false;
            if (path[0] == m_NamespaceSeparator) path = path.TrimStart(m_NamespaceSeparator);
            if (!path.StartsWith(ns)) return false;
            path = path.Substring(ns.Length);
            return true;
        }

        public static PSObject ToPSObject(this IList<ConfigKey> configKeys, string ns)
        {
            // Try removes namespaces and collate items.
            var _items = new List<ConfigKeyPathItem>(configKeys.Count);
            var _hasNS = (ns = NormalizeNamespace(ns)) != string.Empty;
            var _itemComparer = new ConfigKeyPathEntryComparer();

            foreach (var _configKey in configKeys)
            {
                var _path = _configKey.Path;
                if (!_hasNS || RemoveNamespace(ref _path, ns))
                {
                    var _item = new ConfigKeyPathItem { Path = _path, ConfigKey = _configKey };
                    var _index = _items.BinarySearch(_item, _itemComparer);
                    _items.Insert(_index < 0 ? ~_index : _index, _item);
                }
            }

            var _root = new PSObject();
            ConfigKeyPathItem _prevItem = null;
            foreach (var _item in _items)
            {
                var _owner = _root;
                string _propertyName, _ownerPath;
                List<ConfigKeyPathSubItem> _subs = null;
                var _index = _item.Path.LastIndexOf(m_NamespaceSeparator);
                if (_index == -1)
                {
                    _propertyName = _item.Path;
                    _ownerPath = null;
                }
                else
                {
                    _propertyName = _item.Path.Substring(_index + 1);
                    _ownerPath = _item.Path.Remove(_index + 1);
                }

                if (_ownerPath != null)
                {
                    var _prevLastSub = _prevItem == null || _prevItem.Subs == null || _prevItem.Subs.Count == 0 ? null : _prevItem.Subs[_prevItem.Subs.Count - 1];

                    if (_prevLastSub != null && string.Equals(_ownerPath, _prevLastSub.Path, StringComparison.OrdinalIgnoreCase))
                    {
                        _owner = _prevLastSub.Object;
                        _subs = _prevItem.Subs;
                    }
                    else
                    {
                        _subs = new List<ConfigKeyPathSubItem>();
                        string[] _subProperties = _ownerPath.Split(new[] { m_NamespaceSeparator }, StringSplitOptions.RemoveEmptyEntries);
                        var _subPropertiesStart = 0;

                        if (_prevLastSub != null && _ownerPath.StartsWith(_prevLastSub.Path, StringComparison.OrdinalIgnoreCase))
                        {
                            _owner = _prevLastSub.Object;
                            _subs.AddRange(_prevItem.Subs);
                            _subPropertiesStart = _subs.Count;
                        }
                        else if (_prevLastSub != null)
                        {
                            string _subPath = null;
                            for (int i = 0; i < _prevItem.Subs.Count; i++)
                            {
                                if (i >= _subProperties.Length) break;
                                var _sub = _prevItem.Subs[i];
                                _subPath += _subProperties[i] + m_NamespaceSeparator;
                                if (!string.Equals(_sub.Path, _subPath, StringComparison.OrdinalIgnoreCase)) break;
                                _owner = _sub.Object;
                                _subs.Add(_sub);
                                _subPropertiesStart = i + 1;
                            }
                        }

                        _prevLastSub = _subPropertiesStart == 0 ? null : _subs[_subPropertiesStart - 1];
                        for (int i = _subPropertiesStart; i < _subProperties.Length; i++)
                        {
                            var _subPropertyName = _subProperties[i];
                            var _object = new PSObject();
                            _owner.Properties.Add(new PSNoteProperty(_subPropertyName, _object));
                            _owner = _object;
                            _subs.Add(_prevLastSub = new ConfigKeyPathSubItem { Path = _prevLastSub?.Path + _subPropertyName + m_NamespaceSeparator, Object = _object });
                        }
                    }
                }

                _owner.Properties.Add(new PSNoteProperty(_propertyName, _item.ConfigKey.Value<object>()));

                _item.Subs = _subs;
                _prevItem = _item;
            }

            return _root;
        }

        private class ConfigKeyPathEntryComparer : IComparer<ConfigKeyPathItem>
        {
            public int Compare(ConfigKeyPathItem x, ConfigKeyPathItem y) => StringComparer.OrdinalIgnoreCase.Compare(x.Path, y.Path);
        }

        [DebuggerDisplay("{Path,nq}")]
        private class ConfigKeyPathItem
        {
            public string Path;
            public ConfigKey ConfigKey;
            public List<ConfigKeyPathSubItem> Subs;
        }

        [DebuggerDisplay("{Path,nq}, {Object}")]
        private class ConfigKeyPathSubItem
        {
            public string Path;
            public PSObject Object;
        }
    }
}
