using ServiceLib.Common;

namespace ServiceLib.Tests.Common;

public class DragSelectionHelperTests
{
    [Test]
    public async Task GetRange_AnchorBeforeCurrent_ReturnsAscendingRange()
    {
        var range = DragSelectionHelper.GetRange(3, 7, 20);
        await range.HasValue.Should().BeTrue();
        await range!.Value.First.Should().BeEqualTo(3);
        await range.Value.Last.Should().BeEqualTo(7);
    }

    [Test]
    public async Task GetRange_AnchorAfterCurrent_ReturnsSameRangeAscending()
    {
        var range = DragSelectionHelper.GetRange(10, 6, 20);
        await range.HasValue.Should().BeTrue();
        await range!.Value.First.Should().BeEqualTo(6);
        await range.Value.Last.Should().BeEqualTo(10);
    }

    [Test]
    public async Task GetRange_AnchorEqualsCurrent_ReturnsSingleRow()
    {
        var range = DragSelectionHelper.GetRange(4, 4, 20);
        await range.HasValue.Should().BeTrue();
        await range!.Value.First.Should().BeEqualTo(4);
        await range.Value.Last.Should().BeEqualTo(4);
    }

    [Test]
    public async Task GetRange_OutOfRangeValues_AreClampedToList()
    {
        var range = DragSelectionHelper.GetRange(-5, 99, 8);
        await range.HasValue.Should().BeTrue();
        await range!.Value.First.Should().BeEqualTo(0);
        await range.Value.Last.Should().BeEqualTo(7);
    }

    [Test]
    public async Task GetRange_EmptyList_ReturnsNull()
    {
        var range = DragSelectionHelper.GetRange(0, 0, 0);
        await range.HasValue.Should().BeFalse();
    }

    private static readonly (double Top, double Height)[] ThreeRows =
    [
        (30, 30), // row 0: 30..60
        (60, 30), // row 1: 60..90
        (90, 30), // row 2: 90..120
    ];

    [Test]
    public async Task GetRowIndexAt_InsideRow_ReturnsThatRow()
    {
        await DragSelectionHelper.GetRowIndexAt(ThreeRows, 45).Should().BeEqualTo(0);
        await DragSelectionHelper.GetRowIndexAt(ThreeRows, 100).Should().BeEqualTo(2);
    }

    [Test]
    public async Task GetRowIndexAt_BoundaryBetweenRows_BelongsToLowerRow()
    {
        // top edge of a row is inside it, bottom edge (= next row's top) is not
        await DragSelectionHelper.GetRowIndexAt(ThreeRows, 60).Should().BeEqualTo(1);
        await DragSelectionHelper.GetRowIndexAt(ThreeRows, 59.999).Should().BeEqualTo(0);
        await DragSelectionHelper.GetRowIndexAt(ThreeRows, 90).Should().BeEqualTo(2);
    }

    [Test]
    public async Task GetRowIndexAt_AboveFirstOrBelowLast_ReturnsMinusOne()
    {
        await DragSelectionHelper.GetRowIndexAt(ThreeRows, 29.9).Should().BeEqualTo(-1);
        await DragSelectionHelper.GetRowIndexAt(ThreeRows, 120).Should().BeEqualTo(-1);
        await DragSelectionHelper.GetRowIndexAt(ThreeRows, 500).Should().BeEqualTo(-1);
    }

    [Test]
    public async Task GetRowIndexAt_ScrolledList_UsesCurrentRowTops()
    {
        // rows scrolled up by 100 px: the first visible row starts above the viewport
        var scrolled = new (double Top, double Height)[] { (-70, 30), (-40, 30), (-10, 30), (20, 30) };
        await DragSelectionHelper.GetRowIndexAt(scrolled, 5).Should().BeEqualTo(2);
        await DragSelectionHelper.GetRowIndexAt(scrolled, -65).Should().BeEqualTo(0);
        await DragSelectionHelper.GetRowIndexAt(scrolled, 50).Should().BeEqualTo(-1);
    }

    [Test]
    public async Task GetRowIndexAt_NoRows_ReturnsMinusOne()
    {
        await DragSelectionHelper.GetRowIndexAt(Array.Empty<(double Top, double Height)>(), 10).Should().BeEqualTo(-1);
    }

    [Test]
    public async Task GetScrollDirection_InsideAboveBelow_ReturnsZeroMinusOneOne()
    {
        await DragSelectionHelper.GetScrollDirection(50, 30, 200).Should().BeEqualTo(0);
        await DragSelectionHelper.GetScrollDirection(10, 30, 200).Should().BeEqualTo(-1);
        await DragSelectionHelper.GetScrollDirection(200, 30, 200).Should().BeEqualTo(1);
    }

    [Test]
    public async Task GetAutoScrollSpeed_InsideViewport_ReturnsZero()
    {
        await DragSelectionHelper.GetAutoScrollSpeed(50, 30, 200).Should().BeEqualTo(0d);
    }

    [Test]
    public async Task GetAutoScrollSpeed_JustOutsideEdge_ScrollsSlowlyTowardsThePointer()
    {
        await DragSelectionHelper.GetAutoScrollSpeed(29, 30, 200).Should().BeEqualTo(-4.0025);
        await DragSelectionHelper.GetAutoScrollSpeed(200, 30, 200).Should().BeEqualTo(4d);
    }

    [Test]
    public async Task GetAutoScrollSpeed_GrowsWithDistanceAndCaps()
    {
        await DragSelectionHelper.GetAutoScrollSpeed(-70, 30, 200).Should().BeEqualTo(-29d);  // 100 px above
        await DragSelectionHelper.GetAutoScrollSpeed(400, 30, 200).Should().BeEqualTo(104d);  // 200 px below
        await DragSelectionHelper.GetAutoScrollSpeed(-2000, 30, 200).Should().BeEqualTo(-160d); // capped
    }

    [Test]
    public async Task Advance_MovesByStepAndClampsToList()
    {
        await DragSelectionHelper.Advance(5, 4, 20).Should().BeEqualTo(9);
        await DragSelectionHelper.Advance(5, -4, 20).Should().BeEqualTo(1);
        await DragSelectionHelper.Advance(2, -4, 20).Should().BeEqualTo(0);
        await DragSelectionHelper.Advance(18, 4, 20).Should().BeEqualTo(19);
        await DragSelectionHelper.Advance(0, 0, 0).Should().BeEqualTo(0);
    }
}
