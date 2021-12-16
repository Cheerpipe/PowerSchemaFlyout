using Ninject.Modules;
using PowerSchemaFlyout.Services;
using PowerSchemaFlyout.Services.Configuration;

namespace PowerSchemaFlyout.IoC
{
    public class Bindings : NinjectModule
    {
        public override void Load()
        {
            Bind<ITrayIconService>().To<WindowsTrayIconService>().InSingletonScope();
            Bind<IFlyoutService>().To<FlyoutService>().InSingletonScope();
            Bind<IPresetDetectionService>().To<PresetDetectionService>().InSingletonScope();
            Bind<IPowerSchemaWatcherService>().To<PowerSchemaWatcherService>().InSingletonScope();
            Bind<ICaffeineService>().To<CaffeineService>().InSingletonScope();
            Bind<ISettingsService>().To<SettingsService>().InSingletonScope();
            Bind<IPowerManagementServices>().To<PowerManagementServices>().InSingletonScope();
            Bind<IConfigurationService>().To<ConfigurationService>().InSingletonScope();
        }
    }
}