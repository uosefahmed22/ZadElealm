using Org.BouncyCastle.Crypto;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ZadElealm.Apis.Dtos;
using ZadElealm.Core.Enums;
using ZadElealm.Core.Models;
using ZadElealm.Core.Policies;
using ZadElealm.Core.Repositories;
using ZadElealm.Core.Service;
using ZadElealm.Core.Specifications;
using ZadElealm.Core.Specifications.UserRank;
using ZadElealm.Service.Mappers;

namespace ZadElealm.Service.AppServices
{
    public class UserRankCalculator : IUserRankCalculator
    {
        private readonly IUnitOfWork _unitOfWork;

        public UserRankCalculator(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public async Task<int> CalculatePoints(string userId)
        {
            if (string.IsNullOrWhiteSpace(userId))
                throw new ArgumentException("معرف المستخدم مطلوب", nameof(userId));

            var totalPoints = 0;
            var averageQuizScore = 0d;

            var progressSpec = new ProgressWithQuizSpecification(userId, true);
            var completedProgresses = await _unitOfWork.Repository<Progress>()
                .GetAllWithSpecNoTrackingAsync(progressSpec);

            var completedCoursesCount = completedProgresses
                .Select(p => p.Quiz.CourseId)
                .Distinct()
                .Count();

            totalPoints += completedCoursesCount * UserRankPolicy.PointsPerCompletedCourse;

            var certificateSpec = new CertificatesByUserSpecification(userId);
            var certificatesCount = await _unitOfWork.Repository<Certificate>()
                .CountAsync(certificateSpec);
            totalPoints += certificatesCount * UserRankPolicy.PointsPerCertificate;

            if (completedProgresses.Any())
            {
                averageQuizScore = completedProgresses.Average(p => p.Score);
                totalPoints += UserRankPolicy.CalculateQuizAverageBonus(averageQuizScore);
            }

            var userRankSpec = new UserRankWithUserSpecification(userId);
            var userRank = await _unitOfWork.Repository<UserRank>()
                .GetEntityWithSpecAsync(userRankSpec);

            if (userRank == null)
            {
                userRank = new UserRank { UserId = userId };
                await _unitOfWork.Repository<UserRank>().AddAsync(userRank);
            }
            else
            {
                _unitOfWork.Repository<UserRank>().Update(userRank);
            }

            userRank.CompletedCoursesCount = completedCoursesCount;
            userRank.CertificatesCount = certificatesCount;
            userRank.AverageQuizScore = averageQuizScore;
            userRank.TotalPoints = totalPoints;
            userRank.Rank = DetermineRank(totalPoints);
            userRank.LastUpdated = DateTime.UtcNow;
            await _unitOfWork.Complete();

            return totalPoints;
        }

        public UserRankEnum DetermineRank(int points)
        {
            return UserRankPolicy.DetermineRank(points);
        }

        public async Task<UserRankDto> GetUserRank(string userId)
        {
            var spec = new UserRankWithUserSpecification(userId);
            var userRank = await _unitOfWork.Repository<UserRank>()
                .GetEntityWithSpecNoTrackingAsync(spec);

            return userRank?.ToDto();
        }
    }
}
