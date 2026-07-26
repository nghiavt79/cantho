using System.Linq;
using FluentAssertions;
using TechExchangeApp.Configuration;
using Xunit;

namespace TechExchangeApp.Tests.Application
{
    /// <summary>
    /// Kiểm thử cấu hình số bước hiển thị của quy trình giao dịch (Workflow:VisibleStepCount).
    /// Chủ đầu tư có thể chọn hiển thị 7, 10 hoặc 14 bước; giá trị ngoài khoảng [1,14] được kẹp lại.
    /// </summary>
    public class WorkflowOptionsTests
    {
        [Fact]
        public void DefaultVisibleStepCount_Is7()
        {
            var options = new WorkflowOptions();
            options.EffectiveVisibleStepCount.Should().Be(7);
        }

        [Theory]
        [InlineData(7, 7)]
        [InlineData(10, 10)]
        [InlineData(14, 14)]
        [InlineData(1, 1)]
        public void EffectiveVisibleStepCount_WithinRange_ReturnsSameValue(int configured, int expected)
        {
            var options = new WorkflowOptions { VisibleStepCount = configured };
            options.EffectiveVisibleStepCount.Should().Be(expected);
        }

        [Theory]
        [InlineData(0, 1)]     // dưới ngưỡng -> kẹp về 1
        [InlineData(-5, 1)]    // âm -> kẹp về 1
        [InlineData(15, 14)]   // trên ngưỡng -> kẹp về 14
        [InlineData(999, 14)]  // rất lớn -> kẹp về 14
        public void EffectiveVisibleStepCount_OutOfRange_IsClamped(int configured, int expected)
        {
            var options = new WorkflowOptions { VisibleStepCount = configured };
            options.EffectiveVisibleStepCount.Should().Be(expected);
        }

        [Theory]
        [InlineData(7, 7)]
        [InlineData(10, 10)]
        [InlineData(14, 14)]
        public void IsStepVisible_MarksExactlyNStepsVisible(int visibleCount, int expectedVisible)
        {
            var options = new WorkflowOptions { VisibleStepCount = visibleCount };

            var visibleSteps = Enumerable.Range(1, WorkflowOptions.TotalSteps)
                .Count(stepNumber => options.IsStepVisible(stepNumber));

            visibleSteps.Should().Be(expectedVisible);
        }

        [Fact]
        public void IsStepVisible_HidesStepsBeyondConfiguredCount()
        {
            var options = new WorkflowOptions { VisibleStepCount = 10 };

            options.IsStepVisible(1).Should().BeTrue();
            options.IsStepVisible(10).Should().BeTrue();
            options.IsStepVisible(11).Should().BeFalse();
            options.IsStepVisible(14).Should().BeFalse();
        }
    }
}
