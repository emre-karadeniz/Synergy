namespace Synergy.Framework.EfCore.Pagination;

public class PaginationResponse<T>(List<T> dataList, int totalRecords, int pageNumber, int pageSize)
{
    public int PageNumber { get; set; } = pageNumber;
    public int PageSize { get; set; } = pageSize;
    public int TotalRecords { get; set; } = totalRecords;
    public int TotalPages => (int)Math.Ceiling((double)TotalRecords / PageSize);
    public List<T> DataList { get; set; } = dataList;
}
