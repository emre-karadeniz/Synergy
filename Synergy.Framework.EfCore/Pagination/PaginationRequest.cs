using Synergy.Framework.EfCore.Exceptions;

namespace Synergy.Framework.EfCore.Pagination;

public class PaginationRequest
{
    private int _pageNumber = 1;
    private int _pageSize = 10;
    private const int MaxPageSize = 1000;

    public int PageNumber
    {
        get => _pageNumber;
        set
        {
            if (value < 1)
                throw new EfCoreException("PageNumber must be greater than 0.", "PAGINATION");
            _pageNumber = value;
        }
    }

    public int PageSize
    {
        get => _pageSize;
        set
        {
            if (value < 1)
                throw new EfCoreException("PageSize must be greater than 0.", "PAGINATION");
            if (value > MaxPageSize)
                throw new EfCoreException($"PageSize must not exceed {MaxPageSize}.", "PAGINATION");
            _pageSize = value;
        }
    }
}
