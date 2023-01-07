using Microsoft.UI.Composition.SystemBackdrops;
using Microsoft.UI.Composition;
using Microsoft.UI.Xaml;
using Windows.UI.ViewManagement;
using WinRT;

namespace MauiAcrylic.Platforms.Windows
{
    public static class WindowHelpers
    {
        public static void TryAcrylic(this Microsoft.UI.Xaml.Window window)
        {
            var dispatcherQueueHelper = new WindowsSystemDispatcherQueueHelper(); // in Platforms.Windows folder
            dispatcherQueueHelper.EnsureWindowsSystemDispatcherQueueController();

            // Hooking up the policy object
            var configurationSource = new SystemBackdropConfiguration();
            configurationSource.IsInputActive = true;

            switch (((FrameworkElement)window.Content).ActualTheme)
            {
                case ElementTheme.Dark:
                    configurationSource.Theme = SystemBackdropTheme.Dark;
                    break;
                case ElementTheme.Light:
                    configurationSource.Theme = SystemBackdropTheme.Light;
                    break;
                case ElementTheme.Default:
                    configurationSource.Theme = SystemBackdropTheme.Default;
                    break;
            }

            if (DesktopAcrylicController.IsSupported())
            {
                var acrylicController = new DesktopAcrylicController();
                acrylicController.LuminosityOpacity = 0.3f;
                acrylicController.TintOpacity = 0.7f;
                var uiSettings = new UISettings();
                var color = uiSettings.GetColorValue(UIColorType.AccentDark2);
                acrylicController.TintColor = color;
                acrylicController.FallbackColor = color;
                acrylicController.AddSystemBackdropTarget(window.As<ICompositionSupportsSystemBackdrop>());
                acrylicController.SetSystemBackdropConfiguration(configurationSource);



                window.Activated += (object sender, WindowActivatedEventArgs args) =>
                {
                    if (args.WindowActivationState is WindowActivationState.CodeActivated or WindowActivationState.PointerActivated)
                    {
                        // Handle situation where a window is activated and placed on top of other active windows.
                        if (acrylicController == null)
                        {
                            acrylicController = new DesktopAcrylicController();
                            acrylicController.AddSystemBackdropTarget(window.As<ICompositionSupportsSystemBackdrop>()); // Requires 'using WinRT;'
                            acrylicController.SetSystemBackdropConfiguration(configurationSource);
                        }
                    }

                    if (configurationSource != null)
                        configurationSource.IsInputActive = args.WindowActivationState != WindowActivationState.Deactivated;
                };

                window.Closed += (object sender, WindowEventArgs args) =>
                {
                    if (acrylicController != null)
                    {
                        acrylicController.Dispose();
                        acrylicController = null;
                    }

                    configurationSource = null;
                };
            }

        }
        
    }
}
