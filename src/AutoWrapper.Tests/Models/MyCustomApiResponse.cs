using System;

namespace AutoWrapper.Tests.Models
{
    public class MyCustomApiResponse
    {
        public int Code { get; set; }
        public string Message { get; set; }
        public object Payload { get; set; }
        public DateTime SentDate { get; set; }
        public Pagination Pagination { get; set; }

        public MyCustomApiResponse(DateTime sentDate,
            object payload = null,
            string message = "",
            int statusCode = 200,
            Pagination pagination = null)
        {
            this.Code = statusCode;
            this.Message = message == string.Empty ? "Success" : message;
            this.Payload = payload;
            this.SentDate = sentDate;
            this.Pagination = pagination;
        }

        public MyCustomApiResponse(DateTime sentDate,
            object payload = null,
            Pagination pagination = null)
        {
            this.Code = 200;
            this.Message = "Success";
            this.Payload = payload;
            this.SentDate = sentDate;
            this.Pagination = pagination;
        }

        public MyCustomApiResponse(object payload)
        {
            this.Code = 200;
            this.Payload = payload;
        }

    }

    public class Pagination
    {
        public int TotalItemsCount { get; set; }
        public int PageSize { get; set; }
        public int CurrentPage { get; set; }
        public int TotalPages { get; set; }
    }
}
