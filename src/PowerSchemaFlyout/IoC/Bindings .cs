using Ninject.Modules;
using PowerSchemaFlyout.Services;
using PowerSchemaFlyout.Services.GameDetectionService;
using PowerSchemaFlyout.Services.PowerSchemaWatcherService;

namespace PowerSchemaFlyout.IoC
{
    public class Bindings : NinjectModule
    {
        public override void Load()
        {
            Bind<ITrayIconService>().To<TrayIconService>().InSingletonScope();
            Bind<IFlyoutService>().To<FlyoutService>().InSingletonScope();
            Bind<IGameDetectionService>().To<GameDetectionService>().InSingletonScope();
            Bind<IPowerSchemaWatcherService>().To<PowerSchemaWatcherService>().InSingletonScope();
        }
    }
}