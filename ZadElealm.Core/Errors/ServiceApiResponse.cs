using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ZadElealm.Service.Errors
{
    public class ServiceApiResponse<T>
    {
        public bool IsSuccess { get; set; }
        public string Message { get; set; }
        public T Data { get; set; }

        public static ServiceApiResponse<T> Success(T data, string message = "Success")
        {
            return new ServiceApiResponse<T>
            {
                IsSuccess = true,
                Message = message,
                Data = data
            };
        }

        public static ServiceApiResponse<T> Failure(string message, int v)
        {
            return new ServiceApiResponse<T>
            {
                IsSuccess = false,
                Message = message,
                Data = default
            };
        }
    }
}
