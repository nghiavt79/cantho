using FluentAssertions;
using iText.Html2pdf;
using iText.Kernel.Geom;
using iText.Kernel.Pdf;
using iText.Layout;
using iText.Layout.Element;
using Microsoft.Extensions.Logging.Abstractions;
using TechExchangeApp.Services.EContracts;
using Xunit;

namespace TechExchangeApp.Tests.Application;

public class SignaturePlacementServiceTests
{
    private readonly SignaturePlacementService _service = new(NullLogger<SignaturePlacementService>.Instance);

    private static byte[] TemplatePdf()
    {
        var dir = new DirectoryInfo(AppContext.BaseDirectory);
        while (dir != null && !File.Exists(System.IO.Path.Combine(dir.FullName, "TechExchangeApp.sln")))
            dir = dir.Parent;
        var template = System.IO.Path.Combine(dir!.FullName, "TechExchangeApp", "wwwroot", "templates", "contract_chuyen_giao_cong_nghe.html");

        using var output = new MemoryStream();
        HtmlConverter.ConvertToPdf(File.ReadAllText(template), output);
        return output.ToArray();
    }

    private static byte[] PlainPdf(int pages, string? lastPageText, int rotation = 0)
    {
        using var output = new MemoryStream();
        using (var pdf = new PdfDocument(new PdfWriter(output)))
        using (var document = new Document(pdf, PageSize.A4))
        {
            for (var i = 1; i <= pages; i++)
            {
                if (i > 1) document.Add(new AreaBreak());
                document.Add(new Paragraph(i == pages && lastPageText != null ? lastPageText : $"Trang {i}"));
            }
            for (var i = 1; i <= pages && rotation != 0; i++)
                pdf.GetPage(i).SetRotation(rotation);
        }
        return output.ToArray();
    }

    [Fact]
    public void RealTemplate_FindsBothParties_SideBySide_OnSamePage()
    {
        var pdf = TemplatePdf();

        var a = _service.FindPlacement(pdf, EContractSignatureParty.BenA);
        var b = _service.FindPlacement(pdf, EContractSignatureParty.BenB);

        a.Should().NotBeNull();
        b.Should().NotBeNull();
        a!.Page.Should().Be(b!.Page);
        a.Y.Should().Be(b.Y);
        a.X.Should().BeLessThan(b.X, "Bên A ở cột trái");
        (a.X + a.Width).Should().BeLessThanOrEqualTo(b.X, "hai vùng ký không chồng nhau");
    }

    [Fact]
    public void YCoordinate_AccumulatesPreviousPages()
    {
        const string html = """
            <html><body>
            <div style="page-break-after:always">Trang 1</div>
            <div style="page-break-after:always">Trang 2</div>
            <div>Đại diện Bên A</div>
            </body></html>
            """;
        using var output = new MemoryStream();
        HtmlConverter.ConvertToPdf(html, output);
        var pdf = output.ToArray();
        using var document = new PdfDocument(new PdfReader(new MemoryStream(pdf)));
        var pageHeight = (double)document.GetPage(1).GetPageSize().GetHeight();

        var placement = _service.FindPlacement(pdf, EContractSignatureParty.BenA)!;

        placement.Page.Should().Be(3);
        placement.Y.Should().BeGreaterThan(2 * pageHeight);
        placement.Y.Should().BeLessThan(3 * pageHeight);
    }

    [Fact]
    public void AnchorMissing_ReturnsNull()
    {
        _service.FindPlacement(PlainPdf(2, "Khong co dong ky"), EContractSignatureParty.BenA).Should().BeNull();
    }

    [Fact]
    public void RotatedPage_ReturnsNull()
    {
        _service.FindPlacement(PlainPdf(1, "Bat ky", rotation: 90), EContractSignatureParty.BenA).Should().BeNull();
    }

    [Fact]
    public void InvalidPdf_ReturnsNull()
    {
        _service.FindPlacement(new byte[] { 1, 2, 3 }, EContractSignatureParty.BenA).Should().BeNull();
    }
}
