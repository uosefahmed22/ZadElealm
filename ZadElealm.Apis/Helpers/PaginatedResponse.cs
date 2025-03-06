using ZadElealm.Apis.Errors;

namespace ZadElealm.Apis.Helpers
{
    public class PaginatedResponse<T> : ApiResponse
    {
        public MetaData MetaData { get; set; }
        public IReadOnlyList<T> Data { get; set; }

        public PaginatedResponse(int statusCode, IReadOnlyList<T> data, MetaData metaData, string? message = null)
            : base(statusCode, message)
        {
            Data = data;
            MetaData = metaData;
        }
    }
}
