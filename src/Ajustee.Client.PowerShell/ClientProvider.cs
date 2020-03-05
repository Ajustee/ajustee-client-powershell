using System;
using System.Management.Automation;

namespace Ajustee.Client.PowerShell
{
    internal class ClientProvider : IDisposable
    {
        private PSCmdlet m_Cmdlet;
        private Uri m_ApiUrl;
        private string m_AppId;
        private object m_TrackerId;
        private AjusteeConnectionSettings m_DefaultConnectionSettings;
        private AjusteeClient m_Client;

        public ClientProvider(PSCmdlet cmdlet)
            : base()
        {
            m_Cmdlet = cmdlet;
        }

        public Uri ApiUrl
        {
            get => m_ApiUrl ?? DefaultConnectionSettings.ApiUrl;
            set => m_ApiUrl = value;
        }

        public string AppId
        {
            get => m_AppId ?? DefaultConnectionSettings.ApplicationId;
            set => m_AppId = value;
        }

        public object TrackerId
        {
            get => m_TrackerId ?? DefaultConnectionSettings.TrackerId;
            set => m_TrackerId = value;
        }

        protected AjusteeConnectionSettings DefaultConnectionSettings
        {
            get
            {
                if (m_DefaultConnectionSettings == null)
                    m_DefaultConnectionSettings = m_Cmdlet.GetDefaultConnectionSettings() ?? new AjusteeConnectionSettings();
                return m_DefaultConnectionSettings;
            }
        }

        public AjusteeClient GetClient()
        {
            if (m_Client == null)
            {
                var _settings = new AjusteeConnectionSettings
                {
                    ApiUrl = ApiUrl,
                    ApplicationId = AppId,
                    TrackerId = TrackerId,
                    DefaultPath = DefaultConnectionSettings.DefaultPath,
                    DefaultProperties = DefaultConnectionSettings.DefaultProperties,
                    ReconnectSubscriptions = DefaultConnectionSettings.ReconnectSubscriptions
                };

                m_Client = new AjusteeClient(_settings);
            }
            return m_Client;
        }

        public void Dispose()
        {
            // m_DefaultConnectionSettings
            m_DefaultConnectionSettings = null;

            // m_Client
            var _client = m_Client;
            if (_client != null)
            {
                m_Client = null;
                _client.Dispose();
            }
        }
    }
}
