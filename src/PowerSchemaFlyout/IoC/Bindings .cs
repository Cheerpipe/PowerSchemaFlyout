using Ninject.Modules;
using PowerSchemaFlyout.Services;

namespace PowerSchemaFlyout.IoC
{
    public class Bindings : NinjectModule
    {
        public override void Load()
        {
            Bind<ITrayIconService>().To<TrayIconService>().InSingletonScope();
            Bind<IFlyoutService>().To<FlyoutService>().InSingletonScope();
        }
    }
}