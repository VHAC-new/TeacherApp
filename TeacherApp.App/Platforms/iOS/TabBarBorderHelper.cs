using UIKit;

namespace TeacherApp.App.Platforms.iOS;

internal static class TabBarBorderHelper
{
    public static void ApplyToShell(object? platformView)
    {
        if (platformView is not UIView rootView)
            return;

        var tabBar = FindTabBar(rootView);
        if (tabBar is null)
            return;

        bool isDark = Application.Current?.RequestedTheme == AppTheme.Dark;

        var appearance = new UITabBarAppearance();
        appearance.ConfigureWithOpaqueBackground();

        var bgColor = isDark
            ? UIColor.FromRGB(0x1A, 0x1E, 0x2E)
            : UIColor.White;
        var borderColor = isDark
            ? UIColor.FromRGB(0x2A, 0x30, 0x50)
            : UIColor.FromRGB(0xE5, 0xE7, 0xEB);

        appearance.BackgroundColor = bgColor;
        appearance.ShadowColor = borderColor;

        tabBar.StandardAppearance = appearance;
        if (UIDevice.CurrentDevice.CheckSystemVersion(15, 0))
            tabBar.ScrollEdgeAppearance = appearance;
    }

    private static UITabBar? FindTabBar(UIView view)
    {
        if (view is UITabBar bar)
            return bar;

        foreach (var sub in view.Subviews)
        {
            var found = FindTabBar(sub);
            if (found is not null)
                return found;
        }

        return null;
    }
}
