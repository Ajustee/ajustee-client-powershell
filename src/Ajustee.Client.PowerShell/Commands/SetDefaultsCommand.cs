using System;
using System.Collections;
using System.Collections.Generic;
using System.Management.Automation;

namespace Ajustee.Client.PowerShell
{
    [Cmdlet(VerbsCommon.Set, "AjusteeDefaults")]
    [Alias("aj-defs")]
    [OutputType(typeof(IEnumerable<ConfigKey>))]
    public class SetDefaultsCommand : PSCmdlet
    {
        private Uri m_ApiUrl;
        private bool m_IsApiUrlSet;
        private string m_AppId;
        private bool m_IsAppIdSet;
        private object m_TrackerId;
        private bool m_IsTrackerIdSet;
        private string m_Path;
        private bool m_IsPathSet;
        private IDictionary m_Props;
        private bool m_IsPropsSet;
        private AjusteeConnectionSettings m_DefaultConnectionSettings;

        [Parameter(Mandatory = false)]
        public Uri ApiUrl
        {
            get { return m_ApiUrl; }
            set
            {
                m_ApiUrl = value;
                m_IsApiUrlSet = true;
            }
        }

        [Parameter(Mandatory = false)]
        public string AppId
        {
            get { return m_AppId; }
            set
            {
                m_AppId = value;
                m_IsAppIdSet = true;
            }
        }

        [Parameter(Mandatory = false)]
        public object TrackerId
        {
            get { return m_TrackerId; }
            set
            {
                m_TrackerId = value;
                m_IsTrackerIdSet = true;
            }
        }

        [Parameter(Mandatory = false)]
        public string Path
        {
            get { return m_Path; }
            set
            {
                m_Path = value;
                m_IsPathSet = true;
            }
        }

        [Parameter(Mandatory = false)]
        public IDictionary Props
        {
            get { return m_Props; }
            set
            {
                m_Props = value;
                m_IsPropsSet = true;
            }
        }

        private AjusteeConnectionSettings DefaultConnectionSettings
        {
            get
            {
                if (m_DefaultConnectionSettings == null)
                    m_DefaultConnectionSettings = this.GetDefaultConnectionSettings() ?? new AjusteeConnectionSettings();
                return m_DefaultConnectionSettings;
            }
        }

        protected override void ProcessRecord()
        {
            if (m_IsApiUrlSet)
                DefaultConnectionSettings.ApiUrl = m_ApiUrl;

            if (m_IsAppIdSet)
                DefaultConnectionSettings.ApplicationId = m_AppId;

            if (m_IsTrackerIdSet)
                DefaultConnectionSettings.TrackerId = m_TrackerId;

            if (m_IsPathSet)
                DefaultConnectionSettings.DefaultPath = m_Path;

            if (m_IsPropsSet)
                DefaultConnectionSettings.DefaultProperties = m_Props.ToStringDictionary();
        }

        protected override void EndProcessing()
        {
            if (m_DefaultConnectionSettings != null)
                this.SetDefaultConnectionSettings(m_DefaultConnectionSettings);

            base.EndProcessing();
        }

    }
}
