using ServiceLib.Handler;
using ServiceLib.Models.Configs;

namespace ServiceLib.Tests.Handler;

public class ForkConfigUpgradeTests
{
    private static readonly string[] TrafficColumns = ["TodayUp", "TodayDown", "TotalUp", "TotalDown"];

    [Test]
    public async Task New_UiItem_has_autofit_on_and_no_upgrade_applied()
    {
        var ui = new UIItem();

        await ui.EnableAutoAdjustMainLvColWidth.Should().BeTrue();
        await ui.ForkUiUpgradeVersion.Should().BeEqualTo(0);
    }

    [Test]
    public async Task Upgrade_turns_autofit_on_for_existing_config()
    {
        var ui = new UIItem { EnableAutoAdjustMainLvColWidth = false, MainColumnItem = [] };

        ForkConfigUpgrade.Apply(ui);

        await ui.EnableAutoAdjustMainLvColWidth.Should().BeTrue();
        await ui.ForkUiUpgradeVersion.Should().BeEqualTo(ForkConfigUpgrade.CurrentVersion);
    }

    [Test]
    public async Task Upgrade_hides_traffic_columns_including_missing_entries()
    {
        var ui = new UIItem
        {
            MainColumnItem = [new ColumnItem { Name = "Remarks", Width = 300, Index = 1 }, new ColumnItem { Name = "TodayUp", Width = 100, Index = 9 }],
        };

        ForkConfigUpgrade.Apply(ui);

        foreach (var name in TrafficColumns)
        {
            var column = ui.MainColumnItem.Single(t => t.Name == name);
            await column.Width.Should().BeEqualTo(-1);
        }
        var remarks = ui.MainColumnItem.Single(t => t.Name == "Remarks");
        await remarks.Width.Should().BeEqualTo(300);
    }

    [Test]
    public async Task Upgrade_runs_once_so_a_later_user_choice_is_kept()
    {
        var ui = new UIItem { MainColumnItem = [] };
        ForkConfigUpgrade.Apply(ui);

        ui.EnableAutoAdjustMainLvColWidth = false;
        ui.MainColumnItem.Single(t => t.Name == "TodayDown").Width = 120;
        ForkConfigUpgrade.Apply(ui);

        await ui.EnableAutoAdjustMainLvColWidth.Should().BeFalse();
        await ui.MainColumnItem.Single(t => t.Name == "TodayDown").Width.Should().BeEqualTo(120);
    }
}
