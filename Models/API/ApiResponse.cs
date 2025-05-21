using System.Net;

namespace ProjectManagementSystem.Models.API
{
    public class ApiResponse
    {
        public bool Success { get; set; }
        public string Message { get; set; }
        public HttpStatusCode StatusCode { get; set; }
        public object Data { get; set; }

        public ApiResponse(bool success, string message, HttpStatusCode statusCode, object data = null)
        {
            Success = success;
            Message = message;
            StatusCode = statusCode;
            Data = data;
        }

        public static ApiResponse SuccessResponse(string message = "Thao tác thành công", object data = null)
        {
            return new ApiResponse(true, message, HttpStatusCode.OK, data);
        }

        public static ApiResponse ErrorResponse(string message = "Thao tác thất bại", HttpStatusCode statusCode = HttpStatusCode.BadRequest, object data = null)
        {
            return new ApiResponse(false, message, statusCode, data);
        }
    }
} 