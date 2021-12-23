using System.IO;
using Ninject.Activation;
using Serilog;

namespace PowerSchemaFlyout.Services.LoggerService
{
    public class LoggerProvider : Provider<ILogger>
    {
        protected override ILogger CreateInstance(IContext context)
        {
            //TODO: Fine tune settings
            ILogger _logger = new LoggerConfiguration()
                .WriteTo.File(Path.Combine("logs", "log-.log"), rollingInterval: RollingInterval.Day, retainedFileCountLimit: 10, fileSizeLimitBytes: 8192)
                .MinimumLevel.Verbose()
                .CreateLogger();
            return _logger;
        }
    }
}
