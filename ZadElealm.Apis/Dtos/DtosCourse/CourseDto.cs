﻿using ZadElealm.Apis.Dtos.DtosCategory;
using ZadElealm.Core.Models;

namespace ZadElealm.Apis.Dtos.DtosCourse
{
    public class CourseDto
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public string Author { get; set; }
        public string CourseLanguage { get; set; }
        public int CourseVideosCount { get; set; }
        public decimal rating { get; set; }
        public string ImageUrl { get; set; }
        public CategoryResponseDto Category { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.Now;
    }
}
