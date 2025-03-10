using AutoMapper;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;
using ZadElealm.Apis.Dtos;
using ZadElealm.Core.Models;
using ZadElealm.Core.Repositories;

namespace AdminDashboard.Controllers
{
    public class CategoryController : Controller
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;

        public CategoryController(IUnitOfWork unitOfWork,IMapper mapper)
        {
           _unitOfWork = unitOfWork;
            _mapper = mapper;
        }

        public async Task<IActionResult> Index()
        {
            var categories =await _unitOfWork.Repository<Category>().GetAllAsync();
            var mappedCategories = _mapper.Map<IReadOnlyList<Category>, IReadOnlyList<CategoryResponseDto>>(categories);
            return View(mappedCategories);
        }
    }
}
