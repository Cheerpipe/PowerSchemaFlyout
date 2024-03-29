﻿using System;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using Avalonia.Input;
using Ninject;
using PowerSchemaFlyout.Platform.Windows;
using PowerSchemaFlyout.Screens.FlyoutContainer;
using PowerSchemaFlyout.ViewModels;

namespace PowerSchemaFlyout.Services
{
    public class WindowsFlyoutService : IFlyoutService
    {
        public static FlyoutContainer? FlyoutWindowInstance { get; private set; }
        private readonly IKernel _kernel;
        private Func<ViewModelBase> _populateViewModelFunc;
        private bool _opening;
        private bool _closing;

        public WindowsFlyoutService(IKernel kernel)
        {
            _kernel = kernel;
        }

        public async void Show(bool animate = true)
        {
            if (_opening)
                return;
            _opening = true;

            if (FlyoutWindowInstance != null) return;
            FlyoutWindowInstance = CreateInstance();

            FlyoutWindowInstance.Deactivated += (_, _) =>
            {
                _ = CloseAndRelease();
            };

            FlyoutWindowInstance.KeyDown += (_, e) =>
            {
                if (e.Key == Key.Escape)
                    _ = CloseAndRelease();
            };

            FlyoutWindowInstance.Topmost = true;
            uint currentChildLong = NativeMethods.GetWindowLong(FlyoutWindowInstance.PlatformImpl.Handle.Handle, (int)WindowLongParam.GWL_EXSTYLE);
            NativeMethods.SetWindowLong(FlyoutWindowInstance.PlatformImpl.Handle.Handle, (int)WindowLongParam.GWL_EXSTYLE,
                currentChildLong | (uint)WindowStyles.WS_POPUPWINDOW);

            if (animate)
                await FlyoutWindowInstance.ShowAnimated();
            else
                FlyoutWindowInstance.Show();

            FlyoutWindowInstance?.Activate();

            _opening = false;
        }

        public void SetHeight(double newHeight)
        {
            FlyoutWindowInstance?.SetHeight(newHeight + 1);
        }

        public void SetWidth(double newWidth)
        {
            FlyoutWindowInstance?.SetWidth(newWidth + 1);
        }

        //TODO: Move ViewModel creation outside the Service
        private FlyoutContainer CreateInstance()
        {
            if (_populateViewModelFunc == null)
                throw new Exception("PopulateViewModelFunc delegate must be seted using SetPopulateViewModelFunc() before using a Flyout");

            FlyoutContainer flyoutInstance = _kernel.Get<FlyoutContainer>();
            flyoutInstance.DataContext = _populateViewModelFunc();
            return flyoutInstance;
        }

        public async Task PreLoad()
        {
            if (FlyoutWindowInstance != null) return;

            FlyoutWindowInstance = CreateInstance();

            // This DWM call will make Window invisible for users while preloading
            int value = 0x01;
            NativeMethods.DwmSetWindowAttribute(FlyoutWindowInstance.PlatformImpl.Handle.Handle, DwmWindowAttribute.DWMWA_CLOAK, ref value, Marshal.SizeOf(typeof(int)));

            await FlyoutWindowInstance.ShowAnimated();
            await Task.Delay(500);
            await CloseAndRelease(false);
        }

        public async void Toggle()
        {
            if (FlyoutWindowInstance is null)
            {
                Show();
            }
            else
            {
                await CloseAndRelease();
            }
        }

        public void SetPopulateViewModelFunc(Func<ViewModelBase> populateViewModelFunc)
        {
            _populateViewModelFunc = populateViewModelFunc;
        }

        public async Task CloseAndRelease(bool animate = true)
        {
            if (_closing)
                return;

            _closing = true;

            if (animate)
                await FlyoutWindowInstance.CloseAnimated();
            else
                FlyoutWindowInstance.Close();

            FlyoutWindowInstance.DataContext = null;
            FlyoutWindowInstance = null;

            _closing = false;

            GC.Collect();
            GC.WaitForPendingFinalizers();
            GC.Collect();
        }
    }
}
