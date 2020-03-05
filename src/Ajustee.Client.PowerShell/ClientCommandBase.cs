﻿using System;
using System.Management.Automation;

namespace Ajustee.Client.PowerShell
{
    public abstract class ClientCommandBase : PSCmdlet
    {
        private ClientProvider m_ClientProvider;

        public ClientCommandBase()
            : base()
        {
            m_ClientProvider = new ClientProvider(this);
        }

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

        protected override void BeginProcessing()
        {
            base.WriteVerbose($"AppUrl: {m_ClientProvider.ApiUrl}");
            base.WriteVerbose($"AppId: {m_ClientProvider.AppId}");

            base.BeginProcessing();
        }

        protected override void EndProcessing()
        {
            base.EndProcessing();

            // m_ClientProvider
            var _clientProvider = m_ClientProvider;
            if (_clientProvider != null)
            {
                m_ClientProvider = null;
                _clientProvider.Dispose();
            }
        }
    }
}
