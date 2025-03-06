namespace ZadElealm.Apis.Helpers
{
    public class MetaData
    {
        public int PageSize { get; set; }
        public int CurrentPage { get; set; }
        public int TotalMatchedItems { get; set; }
        public int? NextPage { get; set; }
        public int? PreviousPage { get; set; }
        public int NumberOfPages { get; set; }

        public MetaData()
        {
        }

        public static MetaData Create(int totalItems, int pageIndex, int pageSize)
        {
            pageSize = Math.Max(pageSize, 1);
            var totalPages = (int)Math.Ceiling(totalItems / (double)pageSize);

            pageIndex = Math.Max(1, Math.Min(pageIndex, totalPages));

            int? nextPage = pageIndex < totalPages ? pageIndex + 1 : (int?)null;
            int? previousPage = pageIndex > 1 ? pageIndex - 1 : (int?)null;

            return new MetaData
            {
                CurrentPage = pageIndex,
                NumberOfPages = totalPages,
                TotalMatchedItems = totalItems,
                PageSize = pageSize,
                NextPage = nextPage,
                PreviousPage = previousPage
            };
        }
    }
}
