using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ZadElealm.Core.Specifications.Course
{
    public class CourseSpecParams
    {
        public int CategoryId { get; set; }

        private string? _search;
        public string? Search
        {
            get => _search;
            set => _search = value?.Trim().ToLower();
        }

        private string? _author;
        public string? Author
        {
            get => _author;
            set => _author = value?.Trim().ToLower();
        }

        private string? _language;
        public string? Language
        {
            get => _language;
            set => _language = value?.Trim().ToLower();
        }

        public DateTime? FromDate { get; set; }
        public DateTime? ToDate { get; set; }

        public int? MinRating { get; set; }
        public int? MaxRating { get; set; }

        public string? SortBy { get; set; }
        public string? SortDirection { get; set; } = "asc"; 

        public int PageNumber { get; set; } = 1;
        private int _pageSize = 10;
        public int PageSize
        {
            get => _pageSize;
            set => _pageSize = Math.Clamp(value, 1, 50);
        }
    }
}
