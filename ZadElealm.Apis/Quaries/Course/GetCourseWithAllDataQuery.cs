﻿using ZadElealm.Apis.Errors;

namespace ZadElealm.Apis.Quaries.Course
{
    public class GetCourseWithAllDataQuery : BaseQuery<ApiResponse>
    {
        public int CourseId { get; }
        public string UserId { get; }

        public GetCourseWithAllDataQuery(int courseId, string userId)
        {
            CourseId = courseId;
            UserId = userId;
        }
    }
}
