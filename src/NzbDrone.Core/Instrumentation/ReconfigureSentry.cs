using System.Linq;
using Microsoft.Extensions.Options;
using NLog;
using NzbDrone.Common.EnvironmentInfo;
using NzbDrone.Common.Instrumentation.Sentry;
using NzbDrone.Core.Configuration;
using NzbDrone.Core.Datastore;
using NzbDrone.Core.Lifecycle;
using NzbDrone.Core.Messaging.Events;

namespace NzbDrone.Core.Instrumentation
{
    public class ReconfigureSentry : IHandleAsync<ApplicationStartedEvent>
    {
        private readonly IOptionsMonitor<ConfigFileOptions> _configFileProvider;
        private readonly IPlatformInfo _platformInfo;
        private readonly IMainDatabase _database;

        public ReconfigureSentry(IOptionsMonitor<ConfigFileOptions> configFileProvider,
                                 IPlatformInfo platformInfo,
                                 IMainDatabase database)
        {
            _configFileProvider = configFileProvider;
            _platformInfo = platformInfo;
            _database = database;
        }

        public void Reconfigure()
        {
            // Extended sentry config
            var sentryTarget = LogManager.Configuration.AllTargets.OfType<SentryTarget>().FirstOrDefault();
            if (sentryTarget != null)
            {
                sentryTarget.UpdateScope(_database.Version, _database.Migration, _configFileProvider.CurrentValue.Branch, _platformInfo);
            }
        }

        public void HandleAsync(ApplicationStartedEvent message)
        {
            Reconfigure();
        }
    }
}
