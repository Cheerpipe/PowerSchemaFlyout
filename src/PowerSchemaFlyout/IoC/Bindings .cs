using Ninject.Modules;
using PowerSchemaFlyout.Services;

namespace PowerSchemaFlyout.IoC
{
    public class Bindings : NinjectModule
    {
        public override void Load()
        {
            Bind<ITrayIconService>().To<WindowsTrayIconService>().InSingletonScope();
            Bind<IFlyoutService>().To<FlyoutService>().InSingletonScope();
            Bind<IGameDetectionService>().To<GameDetectionService>().InSingletonScope();
            Bind<IPowerSchemaWatcherService>().To<PowerSchemaWatcherService>().InSingletonScope();
            Bind<ICaffeineService>().To<CaffeineService>().InSingletonScope();
            Bind<ISettingsService>().To<SettingsService>().InSingletonScope();
            Bind<IPowerManagementServices>().To<PowerManagementServices>().InSingletonScope();
        }
    }
}