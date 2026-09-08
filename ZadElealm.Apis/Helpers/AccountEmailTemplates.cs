using System.Net;

namespace ZadElealm.Apis.Helpers;

public static class AccountEmailTemplates
{
    public static string ChangeEmailOtp(string otp) => BuildLayout(
        "تأكيد تغيير البريد الإلكتروني",
        "استخدم رمز التحقق التالي لإكمال تغيير بريد تسجيل الدخول:",
        BuildOtpBlock(otp) +
        Paragraph("الرمز صالح لفترة محدودة، فلا تشاركه مع أي شخص.") +
        SecurityNotice("إذا لم تطلب تغيير بريدك الإلكتروني، تجاهل هذه الرسالة ولن يتغير بريدك."));

    public static string PasswordResetOtp(string displayName, string otp) => BuildLayout(
        "إعادة تعيين كلمة المرور",
        $"مرحبًا {Encode(displayName)}، تلقينا طلبًا لإعادة تعيين كلمة المرور الخاصة بحسابك.",
        BuildOtpBlock(otp) +
        Paragraph("أدخل هذا الرمز في المنصة لإكمال إعادة تعيين كلمة المرور.") +
        SecurityNotice("إذا لم تطلب إعادة تعيين كلمة المرور، تجاهل هذه الرسالة ولا تشارك الرمز."));

    public static string WelcomeConfirmation(string displayName, string confirmationUrl) =>
        BuildActionEmail(
            "مرحبًا بك في زاد تعلم",
            $"مرحبًا {Encode(displayName)}، شكرًا لانضمامك إلينا. أكّد بريدك الإلكتروني لتفعيل حسابك.",
            "تأكيد البريد الإلكتروني",
            confirmationUrl,
            "إذا لم تنشئ هذا الحساب، يمكنك تجاهل الرسالة.");

    public static string ConfirmationReminder(string displayName, string confirmationUrl) =>
        BuildActionEmail(
            "تأكيد البريد الإلكتروني",
            $"مرحبًا {Encode(displayName)}، اضغط على الزر التالي لتأكيد بريدك الإلكتروني وإكمال تفعيل حسابك.",
            "تأكيد البريد الإلكتروني",
            confirmationUrl,
            "إذا لم تطلب هذه الرسالة، يمكنك تجاهلها بأمان.");

    public static string EmailChangedNewAddress() => BuildLayout(
        "تم تحديث البريد الإلكتروني",
        "تم تحديث بريد تسجيل الدخول بنجاح.",
        Paragraph("يمكنك الآن تسجيل الدخول إلى زاد تعلم باستخدام هذا البريد الإلكتروني."));

    private static string BuildActionEmail(
        string title,
        string introduction,
        string actionText,
        string actionUrl,
        string securityMessage) => BuildLayout(
            title,
            introduction,
            $"""
            <div style="margin:28px 0;text-align:center">
              <a href="{Encode(actionUrl)}" style="display:inline-block;padding:14px 28px;border-radius:8px;background:#164f43;color:#ffffff;font-size:16px;font-weight:700;text-decoration:none">{Encode(actionText)}</a>
            </div>
            {SecurityNotice(securityMessage)}
            """);

    private static string BuildOtpBlock(string otp) => $"""
        <div dir="ltr" style="margin:26px auto;padding:18px 24px;border:2px solid #c79a3b;border-radius:10px;background:#fffaf0;color:#123c34;font-family:Arial,sans-serif;font-size:32px;font-weight:700;letter-spacing:8px;text-align:center">{Encode(ZadElealm.Core.Localization.ArabicNumerals.Localize(otp))}</div>
        """;

    private static string Paragraph(string text) =>
        $"<p style=\"margin:18px 0 0;color:#53645f;font-size:15px;line-height:1.9\">{Encode(text)}</p>";

    private static string SecurityNotice(string text) => $"""
        <div style="margin-top:24px;padding:14px 16px;border-right:4px solid #c79a3b;background:#fff8e8;color:#5e5134;font-size:14px;line-height:1.8">{Encode(text)}</div>
        """;

    private static string BuildLayout(string title, string introduction, string content) => $"""
        <!doctype html>
        <html lang="ar" dir="rtl">
          <head>
            <meta charset="utf-8">
            <meta name="viewport" content="width=device-width,initial-scale=1">
            <title>{Encode(title)}</title>
          </head>
          <body dir="rtl" style="margin:0;padding:0;background:#f4efe3;color:#17211d;font-family:Tahoma,Arial,sans-serif">
            <table role="presentation" width="100%" cellspacing="0" cellpadding="0" style="background:#f4efe3">
              <tr>
                <td align="center" style="padding:28px 14px">
                  <table role="presentation" width="100%" cellspacing="0" cellpadding="0" style="max-width:600px;border:1px solid #d5cbb8;border-radius:12px;background:#ffffff;overflow:hidden;text-align:right">
                    <tr>
                      <td style="padding:20px 28px;background:#123c34;color:#ffffff;font-size:20px;font-weight:700">زاد تعلم</td>
                    </tr>
                    <tr>
                      <td style="padding:32px 28px">
                        <h1 style="margin:0 0 16px;color:#123c34;font-size:25px;line-height:1.5">{Encode(title)}</h1>
                        <p style="margin:0;color:#53645f;font-size:16px;line-height:1.9">{introduction}</p>
                        {content}
                      </td>
                    </tr>
                    <tr>
                      <td style="padding:16px 28px;border-top:1px solid #e6dfd1;color:#7a7469;font-size:12px;text-align:center">رسالة آلية من منصة زاد تعلم، يرجى عدم الرد عليها.</td>
                    </tr>
                  </table>
                </td>
              </tr>
            </table>
          </body>
        </html>
        """;

    private static string Encode(string? value) => WebUtility.HtmlEncode(value ?? string.Empty);
}
