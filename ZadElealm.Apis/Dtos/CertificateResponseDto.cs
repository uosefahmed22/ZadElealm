namespace ZadElealm.Apis.Dtos
{
    public class CertificateResponseDto
    {
        public string StudentName { get; set; }
        public string QuizName { get; set; }
        public string PdfUrl { get; set; }
        public DateTime CreatedAt { get; set; }
    }
}
