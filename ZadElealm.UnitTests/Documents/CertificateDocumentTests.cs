using QuestPDF.Fluent;
using QuestPDF.Infrastructure;
using System.Text;
using Xunit;
using ZadElealm.Service.Documents;

namespace ZadElealm.UnitTests.Documents;

public class CertificateDocumentTests
{
    [Fact]
    public void GeneratePdf_ProducesAValidPdfDocument()
    {
        QuestPDF.Settings.License = LicenseType.Community;
        var model = new CertificateDocumentModel(
            "يوسف أحمد",
            "اختبار الفقه الإسلامي",
            92,
            new DateTime(2026, 8, 25, 12, 0, 0, DateTimeKind.Utc),
            "ZA-2026-PREVIEW01",
            null);

        var pdf = new CertificateDocument(model).GeneratePdf();

        Assert.True(pdf.Length > 5_000);
        Assert.Equal("%PDF", Encoding.ASCII.GetString(pdf, 0, 4));
    }
}
