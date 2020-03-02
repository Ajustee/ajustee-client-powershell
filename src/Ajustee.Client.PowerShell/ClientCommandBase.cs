using System;
using System.Management.Automation;

namespace Ajustee.Client.PowerShell
{
    public abstract class CommandBase : PSCmdlet
    {
        private Uri m_ApiUrl;
        private string m_AppId;
        private object m_TrackerId;
        private AjusteeConnectionSettings m_DefaultConnectionSettings;
        private AjusteeClient m_Client;

        [Parameter(Mandatory = false)]
        public Uri ApiUrl
        {
            get => m_ApiUrl ?? DefaultConnectionSettings.ApiUrl;
            set => m_ApiUrl = value;
        }

        [Parameter(Mandatory = false)]
        public string AppId
        {
            get => m_AppId ?? DefaultConnectionSettings.ApplicationId;
            set => m_AppId = value;
        }

        [Parameter(Mandatory = false)]
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
                    m_DefaultConnectionSettings = this.GetDefaultConnectionSettings() ?? new AjusteeConnectionSettings();
                return m_DefaultConnectionSettings;
            }
        }

        protected AjusteeClient Client => m_Client;

        protected override void BeginProcessing()
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

            base.WriteVerbose($"AppUrl: {_settings.ApiUrl}");
            base.WriteVerbose($"AppId: {_settings.ApplicationId}");

            m_Client = new AjusteeClient(_settings);
        }

        protected override void EndProcessing()
        {
            base.EndProcessing();

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
