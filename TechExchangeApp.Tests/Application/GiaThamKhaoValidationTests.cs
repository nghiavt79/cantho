using System.ComponentModel.DataAnnotations;
using System.Collections.Generic;
using FluentAssertions;
using TechExchangeApp.Areas.Cms.Controllers;
using Xunit;

namespace TechExchangeApp.Tests.Application
{
    /// <summary>
    /// L2-15: giá bán dự kiến / giá tham khảo không được là số âm (GiaBanDuKien lưu HTML từ CKEditor).
    /// </summary>
    public class GiaThamKhaoValidationTests
    {
        private static List<ValidationResult> Validate(SanPhamCNTBFormVm vm)
        {
            var results = new List<ValidationResult>();
            Validator.TryValidateObject(vm, new ValidationContext(vm), results, validateAllProperties: true);
            return results;
        }

        [Theory]
        [InlineData("<p>-100</p>")]
        [InlineData("-100")]
        [InlineData("<p>&nbsp;- 5.000.000 VNĐ</p>")]
        [InlineData("−200")]
        [InlineData("-1 OR 2+138-138-1=0")]
        public void GiaAm_BiTuChoi(string gia)
        {
            var errors = Validate(new SanPhamCNTBFormVm { Name = "SP", GiaBanDuKien = gia });
            errors.Should().ContainSingle(e => e.MemberNames.Contains(nameof(SanPhamCNTBFormVm.GiaBanDuKien)));
        }

        [Theory]
        [InlineData(null)]
        [InlineData("")]
        [InlineData("<p>0</p>")]
        [InlineData("<p>150.000.000 VNĐ</p>")]
        [InlineData("<p>Liên hệ</p>")]
        [InlineData("<p>Từ 10 - 20 triệu</p>")]
        public void GiaHopLe_KhongBaoLoi(string? gia)
        {
            Validate(new SanPhamCNTBFormVm { Name = "SP", GiaBanDuKien = gia }).Should().BeEmpty();
        }

        [Fact]
        public void SellPriceAm_BiTuChoi()
        {
            Validate(new SanPhamCNTBFormVm { Name = "SP", SellPrice = -1 })
                .Should().ContainSingle(e => e.MemberNames.Contains(nameof(SanPhamCNTBFormVm.SellPrice)));
        }
    }
}
