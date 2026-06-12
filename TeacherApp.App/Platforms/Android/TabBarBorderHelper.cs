using Android.Util;
using Google.Android.Material.BottomNavigation;
using Google.Android.Material.Navigation;
using AColor = Android.Graphics.Color;
using AndroidView = Android.Views.View;
using AndroidViewGroup = Android.Views.ViewGroup;

namespace TeacherApp.App.Platforms.Android;

internal static class TabBarBorderHelper
{
    private const float TabBarMinHeightDp = 68f;
    private const float TabBarElevationDp = 10f;

    private static readonly AColor LightBg = AColor.ParseColor("#FFFFFF");
    private static readonly AColor DarkBg = AColor.ParseColor("#1A1E2E");

    public static void ApplyToShell(object? platformView)
    {
        if (platformView is not AndroidView root)
            return;

        var bottomNav = FindBottomNavigationView(root);
        if (bottomNav is null)
            return;

        var metrics = bottomNav.Context?.Resources?.DisplayMetrics;
        if (metrics is null)
            return;

        bool isDark = Application.Current?.RequestedTheme == AppTheme.Dark;
        int drawableRes = isDark
            ? Resource.Drawable.tabbar_top_border_dark
            : Resource.Drawable.tabbar_top_border;
        AColor bgColor = isDark ? DarkBg : LightBg;

        bottomNav.SetBackgroundResource(drawableRes);

        var elevationPx = TypedValue.ApplyDimension(ComplexUnitType.Dip, TabBarElevationDp, metrics);
        bottomNav.Elevation = elevationPx;
        bottomNav.TranslationZ = elevationPx;
        bottomNav.OutlineProvider = null;

        bottomNav.SetMinimumHeight((int)TypedValue.ApplyDimension(
            ComplexUnitType.Dip, TabBarMinHeightDp, metrics));

        bottomNav.LabelVisibilityMode = NavigationBarView.LabelVisibilityLabeled;
        bottomNav.ItemActiveIndicatorEnabled = false;

        var horizontalPad = (int)TypedValue.ApplyDimension(ComplexUnitType.Dip, 8f, metrics);
        bottomNav.SetPadding(horizontalPad, 0, horizontalPad, 0);

        StyleParentChain(bottomNav, bgColor);
    }

    private static void StyleParentChain(BottomNavigationView bottomNav, AColor bgColor)
    {
        var current = bottomNav.Parent as AndroidView;
        var depth = 0;

        while (current is not null && depth < 4)
        {
            current.SetBackgroundColor(bgColor);
            current.Elevation = 0;
            current = current.Parent as AndroidView;
            depth++;
        }
    }

    private static BottomNavigationView? FindBottomNavigationView(AndroidView view)
    {
        if (view is BottomNavigationView nav)
            return nav;

        if (view is not AndroidViewGroup group)
            return null;

        for (var i = 0; i < group.ChildCount; i++)
        {
            var child = group.GetChildAt(i);
            if (child is null)
                continue;

            var found = FindBottomNavigationView(child);
            if (found is not null)
                return found;
        }

        return null;
    }
}
