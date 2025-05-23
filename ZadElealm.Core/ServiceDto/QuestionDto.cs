﻿namespace ZadElealm.Core.ServiceDto
{
    public class QuestionDto
    {
        public int Id { get; set; }
        public string Text { get; set; }
        public int QuizId { get; set; }
        public List<ChoiceDto> Choices { get; set; } = new List<ChoiceDto>();
    }
}
