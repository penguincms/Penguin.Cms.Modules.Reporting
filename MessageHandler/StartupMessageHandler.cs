using Penguin.Cms.Modules.Reporting.Constants.Strings;
using Penguin.Configuration.Abstractions.Interfaces;
using Penguin.Messaging.Abstractions.Interfaces;
using Penguin.Messaging.Application.Messages;
using Penguin.Persistence.Abstractions;
using System;
using System.Collections.Generic;
using System.Text;

namespace Penguin.Cms.Modules.Reporting.MessageHandler
{
    public class StartupMessageHandler : IMessageHandler<Startup>
    {
        protected IProvideConfigurations ConfigurationProvider { get; set; }
        protected PersistenceConnectionInfo ConnectionInfo { get; set; }

        public StartupMessageHandler(IProvideConfigurations configurationProvider, PersistenceConnectionInfo connectionInfo)
        {
            ConfigurationProvider = configurationProvider;
            ConnectionInfo = connectionInfo;
        }

        public void AcceptMessage(Startup message)
        {
            string ReportingString = ConfigurationProvider.GetConfiguration(ConfigurationNames.CONNECTION_STRINGS_REPORTING);

            if (string.IsNullOrWhiteSpace(ReportingString))
            {
                ConfigurationProvider.SetConfiguration(ConfigurationNames.CONNECTION_STRINGS_REPORTING, ConnectionInfo.ConnectionString);
            }
        }
    }
}