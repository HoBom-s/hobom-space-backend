using FluentAssertions;
using HobomSpace.Application.Helpers;

namespace HobomSpace.Tests.Unit.Helpers;

public class LineDiffHelperTests
{
    [Fact]
    public void ComputeDiff_IdenticalTexts_AllUnchanged()
    {
        var result = LineDiffHelper.ComputeDiff("line1\nline2", "line1\nline2");

        result.Should().HaveCount(2);
        result.Should().AllSatisfy(e => e.Type.Should().Be(DiffType.UNCHANGED));
    }

    [Fact]
    public void ComputeDiff_AddedLines_ShowsAdded()
    {
        var result = LineDiffHelper.ComputeDiff("line1", "line1\nline2");

        result.Should().HaveCount(2);
        result[0].Type.Should().Be(DiffType.UNCHANGED);
        result[0].Content.Should().Be("line1");
        result[1].Type.Should().Be(DiffType.ADDED);
        result[1].Content.Should().Be("line2");
    }

    [Fact]
    public void ComputeDiff_RemovedLines_ShowsRemoved()
    {
        var result = LineDiffHelper.ComputeDiff("line1\nline2", "line1");

        result.Should().HaveCount(2);
        result[0].Type.Should().Be(DiffType.UNCHANGED);
        result[1].Type.Should().Be(DiffType.REMOVED);
        result[1].Content.Should().Be("line2");
    }

    [Fact]
    public void ComputeDiff_MixedChanges_ShowsCorrectTypes()
    {
        var result = LineDiffHelper.ComputeDiff("a\nb\nc", "a\nx\nc");

        result.Should().Contain(e => e.Type == DiffType.UNCHANGED && e.Content == "a");
        result.Should().Contain(e => e.Type == DiffType.REMOVED && e.Content == "b");
        result.Should().Contain(e => e.Type == DiffType.ADDED && e.Content == "x");
        result.Should().Contain(e => e.Type == DiffType.UNCHANGED && e.Content == "c");
    }

    [Fact]
    public void ComputeDiff_EmptyOld_AllAdded()
    {
        var result = LineDiffHelper.ComputeDiff("", "line1\nline2");

        result.Should().HaveCount(2);
        result.Should().AllSatisfy(e => e.Type.Should().Be(DiffType.ADDED));
    }

    [Fact]
    public void ComputeDiff_EmptyNew_AllRemoved()
    {
        var result = LineDiffHelper.ComputeDiff("line1\nline2", "");

        result.Should().HaveCount(2);
        result.Should().AllSatisfy(e => e.Type.Should().Be(DiffType.REMOVED));
    }

    [Fact]
    public void ComputeDiff_BothEmpty_EmptyResult()
    {
        var result = LineDiffHelper.ComputeDiff("", "");

        result.Should().BeEmpty();
    }

    [Fact]
    public void ComputeDiff_LineNumbers_AreSequential()
    {
        var result = LineDiffHelper.ComputeDiff("a\nb", "a\nc");

        for (var i = 0; i < result.Count; i++)
            result[i].LineNumber.Should().Be(i + 1);
    }
}
