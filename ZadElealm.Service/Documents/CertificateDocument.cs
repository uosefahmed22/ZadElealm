using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;

namespace ZadElealm.Service.Documents;

public sealed record CertificateDocumentModel(
    string StudentName,
    string QuizName,
    int Score,
    DateTime IssuedAtUtc,
    string CertificateReference,
    string? LogoPath);

public sealed class CertificateDocument : IDocument
{
    private static readonly Color Primary = Color.FromHex("#163A34");
    private static readonly Color Gold = Color.FromHex("#C79A3B");
    private static readonly Color Background = Color.FromHex("#F7F5F0");
    private static readonly Color Ink = Color.FromHex("#17211D");
    private static readonly Color Muted = Color.FromHex("#66746E");

    private readonly CertificateDocumentModel _model;

    public CertificateDocument(CertificateDocumentModel model)
    {
        ArgumentNullException.ThrowIfNull(model);
        _model = model;
    }

    public DocumentMetadata GetMetadata() => new()
    {
        Title = $"شهادة اجتياز - {_model.QuizName}",
        Author = "منصة زاد العلم",
        Subject = $"شهادة اجتياز للطالب {_model.StudentName}",
        Creator = "Zad Elealm",
        CreationDate = _model.IssuedAtUtc
    };

    public void Compose(IDocumentContainer container)
    {
        container.Page(page =>
        {
            page.Size(PageSizes.A4.Landscape());
            page.Margin(0);
            page.PageColor(Background);
            page.ContentFromRightToLeft();
            page.DefaultTextStyle(style => style
                .FontFamily(Fonts.Arial)
                .FontColor(Ink));

            page.Content()
                .Padding(22)
                .Border(3)
                .BorderColor(Primary)
                .Padding(6)
                .Border(1)
                .BorderColor(Gold)
                .PaddingHorizontal(48)
                .PaddingVertical(27)
                .Column(ComposeContent);
        });
    }

    private void ComposeContent(ColumnDescriptor column)
    {
        column.Spacing(6);

        // The PNG has an asymmetric transparent canvas and the document uses RTL flow.
        // Keep equal layout space on both sides, then compensate for the measured drift.
        column.Item().Row(row =>
        {
            row.RelativeItem();
            row.ConstantItem(92).TranslateX(-15.5f).Height(62).Element(ComposeLogo);
            row.RelativeItem();
        });

        column.Item().AlignCenter()
            .Text("منصة زاد العلم التعليمية")
            .FontSize(11)
            .SemiBold()
            .FontColor(Primary);

        column.Item().PaddingTop(2).AlignCenter().Width(310).Row(line =>
        {
            line.RelativeItem().AlignMiddle().Height(1).Background(Gold);
            line.ConstantItem(20).AlignCenter().Text("◆").FontSize(8).FontColor(Gold);
            line.RelativeItem().AlignMiddle().Height(1).Background(Gold);
        });

        column.Item().PaddingTop(7).AlignCenter()
            .Text("شهادة اجتياز")
            .FontSize(32)
            .Bold()
            .FontColor(Primary);

        column.Item().PaddingTop(3).AlignCenter()
            .Text("تشهد منصة زاد العلم بأن")
            .FontSize(14)
            .FontColor(Muted);

        column.Item().PaddingTop(2).AlignCenter()
            .Text(_model.StudentName)
            .FontSize(34)
            .Bold()
            .FontColor(Primary);

        column.Item().AlignCenter().Width(250).Height(2).Background(Gold);

        column.Item().PaddingTop(7).AlignCenter()
            .Text("قد أتم متطلبات التعلم واجتاز بنجاح")
            .FontSize(14)
            .FontColor(Muted);

        column.Item().AlignCenter()
            .Text(_model.QuizName)
            .FontSize(22)
            .Bold()
            .FontColor(Primary);

        column.Item().PaddingTop(12).AlignCenter().Width(500)
            .BorderTop(1)
            .BorderBottom(1)
            .BorderColor(Gold)
            .PaddingVertical(8)
            .Row(row =>
            {
                row.RelativeItem().AlignCenter().Column(score =>
                {
                    score.Item().AlignCenter().Text("الدرجة المحققة")
                        .FontSize(9)
                        .FontColor(Muted);
                    score.Item().PaddingTop(2).AlignCenter().Text($"{_model.Score}%")
                        .FontSize(16)
                        .Bold()
                        .FontColor(Primary);
                });

                row.ConstantItem(1).Background(Gold);

                row.RelativeItem().AlignCenter().Column(date =>
                {
                    date.Item().AlignCenter().Text("تاريخ الإصدار")
                        .FontSize(9)
                        .FontColor(Muted);
                    date.Item().PaddingTop(2).AlignCenter().Text(FormatArabicDate(_model.IssuedAtUtc))
                        .FontSize(13)
                        .SemiBold()
                        .FontColor(Primary);
                });
            });

        column.Item().ExtendVertical().AlignBottom().PaddingTop(12).Column(bottom =>
        {
            bottom.Item().Row(row =>
            {
                row.RelativeItem().Column(issuer =>
                {
                    issuer.Item().Text("صادرة عن")
                        .FontSize(8)
                        .FontColor(Muted);
                    issuer.Item().PaddingTop(2).Text("منصة زاد العلم")
                        .FontSize(11)
                        .SemiBold()
                        .FontColor(Primary);
                });

                row.RelativeItem().AlignLeft().Column(reference =>
                {
                    reference.Item().AlignLeft().Text("مرجع الشهادة")
                        .FontSize(8)
                        .FontColor(Muted);
                    reference.Item().PaddingTop(2).AlignLeft().Text(_model.CertificateReference)
                        .FontSize(11)
                        .SemiBold()
                        .FontColor(Primary);
                });
            });

            bottom.Item().PaddingTop(9).Height(1).Background(Gold);
            bottom.Item().PaddingTop(5).AlignCenter()
                .Text("وثيقة رقمية صادرة إلكترونيًا عن منصة زاد العلم")
                .FontSize(8)
                .FontColor(Muted);
        });
    }

    private void ComposeLogo(IContainer container)
    {
        if (!string.IsNullOrWhiteSpace(_model.LogoPath) && File.Exists(_model.LogoPath))
        {
            container.Image(_model.LogoPath).FitArea();
            return;
        }

        container
            .Border(1)
            .BorderColor(Gold)
            .Padding(8)
            .AlignCenter()
            .AlignMiddle()
            .Text("زاد العلم")
            .FontSize(12)
            .Bold()
            .FontColor(Primary);
    }

    private static string FormatArabicDate(DateTime date)
    {
        string[] monthNames =
        [
            "يناير", "فبراير", "مارس", "أبريل", "مايو", "يونيو",
            "يوليو", "أغسطس", "سبتمبر", "أكتوبر", "نوفمبر", "ديسمبر"
        ];

        return $"{date.Day} {monthNames[date.Month - 1]} {date.Year}";
    }
}
