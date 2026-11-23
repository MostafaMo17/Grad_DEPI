using System;
using System.Collections.Generic;
using System.Text;

namespace MediCare.Services.Dtos
{
    // One page of a list. Every list endpoint in the API returns this shape
    public class PagedResultDto<T>
    {
        public List<T> Items { get; set; } = new List<T>();
        public int Page { get; set; }
        public int PageSize { get; set; }

        // The number of all items not only this page
        public int TotalCount { get; set; }
    }
}
