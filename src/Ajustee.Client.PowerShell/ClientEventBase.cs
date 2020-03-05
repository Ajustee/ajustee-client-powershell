using System;
using System.Management.Automation;

using Microsoft.PowerShell.Commands;

namespace Ajustee.Client.PowerShell
{
    public abstract class ClientEventBase : ObjectEventRegistrationBase
    {
        private ClientProvider m_ClientProvider;

        public ClientEventBase()
            : base()
        {
            m_ClientProvider = new ClientProvider(this);
        }

        [Parameter(Position = 1, Mandatory = true)]
        [ValidateSet(nameof(AjusteeClient.Changed), nameof(AjusteeClient.Deleted))]
        public string On { get; set; }

        [Parameter(Mandatory = false)]
        public Uri ApiUrl
        {
            get => m_ClientProvider.ApiUrl;
            set => m_ClientProvider.ApiUrl = value;
        }

        [Parameter(Mandatory = false)]
        public string AppId
        {
            get => m_ClientProvider.AppId;
            set => m_ClientProvider.AppId = value;
        }

        [Parameter(Mandatory = false)]
        public object TrackerId
        {
            get => m_ClientProvider.TrackerId;
            set => m_ClientProvider.TrackerId = value;
        }

        protected AjusteeClient Client => m_ClientProvider.GetClient();

        protected override object GetSourceObject()
        {
            return Client;
        }

        protected override string GetSourceObjectEventName()
        {
            return On;
        }
    }
}
