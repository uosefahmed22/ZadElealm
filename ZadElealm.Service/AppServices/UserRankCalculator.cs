using AutoMapper;
using Org.BouncyCastle.Crypto;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ZadElealm.Apis.Dtos;
using ZadElealm.Core.Enums;
using ZadElealm.Core.Models;
using ZadElealm.Core.Repositories;
using ZadElealm.Core.Service;
using ZadElealm.Core.Specifications;
using ZadElealm.Core.Specifications.UserRank;

namespace ZadElealm.Service.AppServices
{
    public class UserRankCalculator : IUserRankCalculator
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;

        public UserRankCalculator(IUnitOfWork unitOfWork, IMapper mapper)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
        }

        public async Task<int> CalculatePoints(string userId)
        {
            int totalPoints = 0;
            int completedCoursesCount = 0;
            int certificatesCount = 0;
            double averageQuizScore = 0;

            var progressSpec = new ProgressWithQuizSpecification(userId, true);
            var completedProgresses = await _unitOfWork.Repository<Progress>()
                .GetAllWithSpecNoTrackingAsync(progressSpec);

            completedCoursesCount = completedProgresses
                .Select(p => p.Quiz.CourseId)
                .Distinct()
                .Count();

            totalPoints += completedCoursesCount * 10;

            var certificateSpec = new CertificatesByUserSpecification(userId);
            var certificates = await _unitOfWork.Repository<Certificate>()
                .GetAllWithSpecNoTrackingAsync(certificateSpec);

            certificatesCount = certificates.Count();
            totalPoints += certificatesCount * 20;

            if (completedProgresses.Any())
            {
                averageQuizScore = completedProgresses.Average(p => p.Score);
                totalPoints += (int)(averageQuizScore * 0.5);
            }

            var userRankSpec = new UserRankWithUserSpecification(userId);
            var userRank = await _unitOfWork.Repository<UserRank>()
                .GetEntityWithSpecAsync(userRankSpec);

            if (userRank != null)
            {
                userRank.CompletedCoursesCount = completedCoursesCount;
                userRank.CertificatesCount = certificatesCount;
                userRank.AverageQuizScore = averageQuizScore;
                _unitOfWork.Repository<UserRank>().Update(userRank);
                await _unitOfWork.Complete();
            }

            return totalPoints;
        }
        public UserRankEnum DetermineRank(int points)
        {
            return points switch
            {
                < 100 => UserRankEnum.Bronze,
                < 300 => UserRankEnum.Silver,
                < 600 => UserRankEnum.Gold,
                < 1000 => UserRankEnum.Platinum,
                _ => UserRankEnum.Diamond
            };
        }
        public async Task<UserRankDto> GetUserRank(string userId)
        {
            var spec = new UserRankWithUserSpecification(userId);
            var userRank = await _unitOfWork.Repository<UserRank>()
                .GetEntityWithSpecNoTrackingAsync(spec);

            return _mapper.Map<UserRankDto>(userRank);
        }
    }
}
