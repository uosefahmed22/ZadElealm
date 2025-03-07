namespace ZadElealm.Core.Models.ServiceDto
{
    public class CertificateDto
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public string PdfUrl { get; set; }
        public DateTime CompletedDate { get; set; }
        public string UserName { get; set; }
        public string QuizName { get; set; }
    }
}
