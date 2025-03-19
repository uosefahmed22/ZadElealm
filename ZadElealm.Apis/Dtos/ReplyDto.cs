namespace ZadElealm.Apis.Dtos
{
    public class ReplyDto
    {
        public int Id { get; set; }
        public string Text { get; set; }
        public string DisplayName { get; set; }
        public string UserImage { get; set; }
        public DateTime CreatedAt { get; set; }
    }
}
