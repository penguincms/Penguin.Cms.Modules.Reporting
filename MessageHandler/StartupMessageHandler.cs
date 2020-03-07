using Penguin.Cms.Modules.Reporting.Constants.Strings;
using Penguin.Configuration.Abstractions.Interfaces;
using Penguin.Messaging.Abstractions.Interfaces;
using Penguin.Messaging.Application.Messages;
using Penguin.Persistence.Abstractions;

namespace Penguin.Cms.Modules.Reporting.MessageHandler
{
    public class StartupMessageHandler : IMessageHandler<Startup>
    {
        protected IProvideConfigurations ConfigurationProvider { get; set; }
        protected PersistenceConnectionInfo ConnectionInfo { get; set; }

        public StartupMessageHandler(IProvideConfigurations configurationProvider, PersistenceConnectionInfo connectionInfo)
        {
            this.ConfigurationProvider = configurationProvider;
            this.ConnectionInfo = connectionInfo;
        }

        public void AcceptMessage(Startup message)
        {
            string ReportingString = this.ConfigurationProvider.GetConfiguration(ConfigurationNames.CONNECTION_STRINGS_REPORTING);

            if (string.IsNullOrWhiteSpace(ReportingString))
            {
                this.ConfigurationProvider.SetConfiguration(ConfigurationNames.CONNECTION_STRINGS_REPORTING, this.ConnectionInfo.ConnectionString);
            }
        }
    }
}