using System.Collections.Generic;

namespace JustCSharp.Data
{
    public class PagedData<T>
    {
        public PagedData()
        {
        }

        public PagedData(IEnumerable<T> data, int total, int page, int pageSize)
        {
            Data = data;
            Total = total;
            Page = page;
            PageSize = pageSize;
            CalculateTotalPage();
        }

        public int Page { get; set; }
        public int PageSize { get; set; }
        public int Total { get; set; }
        public int TotalPage { get; set; }
        public IEnumerable<T>? Data { get; set; }

        public void CalculateTotalPage()
        {
            TotalPage = Total % PageSize == 0 ? Total / PageSize : Total / PageSize + 1;
        }
    }
}