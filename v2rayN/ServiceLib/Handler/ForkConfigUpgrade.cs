namespace ServiceLib.Handler;

// Fork-only one-time config upgrades. UiItem.ForkUiUpgradeVersion records the last applied step, so each step runs once:
// a setting the user changes afterwards is kept on later loads.
public static class ForkConfigUpgrade
{
    public const int CurrentVersion = 1;

    // Traffic statistics columns (Tag names in ProfilesView). Hidden by default.
    private static readonly string[] TrafficColumnNames = ["TodayUp", "TodayDown", "TotalUp", "TotalDown"];

    public static void Apply(UIItem uiItem)
    {
        if (uiItem.ForkUiUpgradeVersion >= CurrentVersion)
        {
            return;
        }

        // v1: auto-adjust column width on for existing configs; traffic statistics columns hidden (Width < 0 = hidden).
        uiItem.EnableAutoAdjustMainLvColWidth = true;
        foreach (var name in TrafficColumnNames)
        {
            var column = uiItem.MainColumnItem.FirstOrDefault(t => t.Name == name);
            if (column == null)
            {
                uiItem.MainColumnItem.Add(new ColumnItem { Name = name, Width = -1, Index = 0 });
            }
            else
            {
                column.Width = -1;
            }
        }

        uiItem.ForkUiUpgradeVersion = CurrentVersion;
    }
}
