using System;
using FluentAssertions;
using Microsoft.Extensions.Caching.Memory;
using TechExchangeApp.Services;
using TechExchangeApp.ViewModel;
using Xunit;

namespace TechExchangeApp.Tests.Security
{
    /// <summary>
    /// Kiểm thử bản vá Account Lockout và Weak Password Policy (pentest MobiFone 10/2026).
    /// </summary>
    public class LoginThrottleTests
    {
        private DateTime _now = new(2026, 10, 16, 8, 0, 0, DateTimeKind.Utc);
        private LoginThrottle Create() => new(new MemoryCache(new MemoryCacheOptions()), () => _now);

        [Fact]
        public void FiveFailures_LockAccountFor15Minutes()
        {
            var t = Create();
            for (var i = 0; i < 4; i++) t.RegisterFailure("Test", "1.1.1.1");
            t.GetLockRemaining("Test", "1.1.1.1").Should().BeNull();

            t.RegisterFailure("test", "1.1.1.1"); // không phân biệt hoa thường
            t.GetLockRemaining("TEST", "2.2.2.2").Should().Be(TimeSpan.FromMinutes(15)); // khóa theo tài khoản, đổi IP vẫn bị khóa

            _now = _now.AddMinutes(15).AddSeconds(1);
            t.GetLockRemaining("Test", "1.1.1.1").Should().BeNull();
        }

        [Fact]
        public void FailuresOutsideWindow_AreNotCounted()
        {
            var t = Create();
            for (var i = 0; i < 4; i++) t.RegisterFailure("u", "ip");
            _now = _now.AddMinutes(6);
            t.RegisterFailure("u", "ip");
            t.GetLockRemaining("u", "ip").Should().BeNull();
        }

        [Fact]
        public void SuccessfulLogin_ResetsAccountCounter()
        {
            var t = Create();
            for (var i = 0; i < 4; i++) t.RegisterFailure("u", "ip");
            t.RegisterSuccess("u");
            t.RegisterFailure("u", "ip");
            t.GetLockRemaining("u", "ip-khac").Should().BeNull();
        }

        [Fact]
        public void ManyAccountsFromOneIp_BlocksThatIp()
        {
            var t = Create();
            for (var i = 0; i < LoginThrottle.MaxIpFailures; i++) t.RegisterFailure("user" + i, "9.9.9.9");
            t.GetLockRemaining("nguoi-khac", "9.9.9.9").Should().NotBeNull();
            t.GetLockRemaining("nguoi-khac", "8.8.8.8").Should().BeNull();
        }
    }

    public class StrongPasswordTests
    {
        [Theory]
        [InlineData("111111")]
        [InlineData("Hello123")]      // thiếu ký tự đặc biệt
        [InlineData("hello@123")]     // thiếu chữ hoa
        [InlineData("HELLO@123")]     // thiếu chữ thường
        [InlineData("Hello@abc")]     // thiếu chữ số
        [InlineData("He@1234")]       // 7 ký tự
        public void WeakPasswords_AreRejected(string pwd)
            => new StrongPasswordAttribute().IsValid(pwd).Should().BeFalse();

        [Theory]
        [InlineData("Hello@123")]
        [InlineData("Sàn#CầnThơ2026")]
        public void StrongPasswords_AreAccepted(string pwd)
            => new StrongPasswordAttribute().IsValid(pwd).Should().BeTrue();

        [Fact]
        public void Empty_IsLeftToRequired()
            => new StrongPasswordAttribute().IsValid("").Should().BeTrue();
    }
}
