using System.Text;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using NzbDrone.Common;
using NzbDrone.Common.EnvironmentInfo;
using NzbDrone.Core.Analytics;
using NzbDrone.Core.Configuration;

namespace Radarr.Http.Frontend
{
    [Authorize(Policy = "UI")]
    [ApiController]
    public class InitializeJsController : Controller
    {
        private readonly IOptionsMonitor<ConfigFileOptions> _configFileProvider;
        private readonly IAnalyticsService _analyticsService;

        private string ApiKey => _configFileProvider.CurrentValue.ApiKey;
        private string UrlBase => _configFileProvider.CurrentValue.UrlBase;

        public InitializeJsController(IOptionsMonitor<ConfigFileOptions> configFileProvider,
                                      IAnalyticsService analyticsService)
        {
            _configFileProvider = configFileProvider;
            _analyticsService = analyticsService;
        }

        [HttpGet("/initialize.js")]
        public IActionResult Index()
        {
            return Content(GetContent(), "application/javascript");
        }

        private string GetContent()
        {
            var builder = new StringBuilder();
            builder.AppendLine("window.Radarr = {");
            builder.AppendLine($"  apiRoot: '{UrlBase}/api/v3',");
            builder.AppendLine($"  apiKey: '{ApiKey}',");
            builder.AppendLine($"  release: '{BuildInfo.Release}',");
            builder.AppendLine($"  version: '{BuildInfo.Version.ToString()}',");
            builder.AppendLine($"  instanceName: '{_configFileProvider.CurrentValue.InstanceName}',");
            builder.AppendLine($"  branch: '{_configFileProvider.CurrentValue.Branch.ToLower()}',");
            builder.AppendLine($"  analytics: {_analyticsService.IsEnabled.ToString().ToLowerInvariant()},");
            builder.AppendLine($"  userHash: '{HashUtil.AnonymousToken()}',");
            builder.AppendLine($"  urlBase: '{UrlBase}',");
            builder.AppendLine($"  isProduction: {RuntimeInfo.IsProduction.ToString().ToLowerInvariant()}");
            builder.AppendLine("};");

            return builder.ToString();
        }
    }
}
