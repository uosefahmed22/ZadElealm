﻿namespace ZadElealm.Apis.Dtos
{
    public class QuizResponseForCourseDto
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.Now;
    }
}
