﻿namespace ZadElealm.Apis.Dtos.DtosCourse
{
    public class AllEnrollementData
    {
        public IEnumerable<CourseDto> Courses { get; set; }
        public int AllEnrolledCourses { get; set; }
    }
}
