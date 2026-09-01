namespace ZadElealm.Core.Specifications.Review
{
    public class ReviewSpecification : BaseSpecification<Core.Models.Review>
    {
        public ReviewSpecification(string userId, int courseId)
            : base(review => review.AppUserId == userId && review.CourseId == courseId)
        {
        }
    }
}
