namespace ProfanityFilter.DTOs.Response
{
    public class GeneralResponseDto<T>
    {
        public bool Success { get; set; }
        public string ErrorMessage { get; set; } = string.Empty;
        public T? Data { get; set; }
        public int StatusCode { get; set; }

        public static GeneralResponseDto<T> SuccesResult(T data, string message = "successful operation")
        {
            return new GeneralResponseDto<T>
            {
                Success = true,
                ErrorMessage = message,
                Data = data,
                StatusCode = 200
            };
        }

        public static GeneralResponseDto<T> FailureResult(int statusCode, string message = "erroneous transaction")
        {
            return new GeneralResponseDto<T>
            {
                Success = false,
                ErrorMessage = message,
                Data = default,
                StatusCode = statusCode
            };
        }
    }
}



