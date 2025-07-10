using Synergy.Framework.Shared.Exceptions;

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
                throw new SynergyException("PageNumber must be greater than 0.", "PAGINATION");
            _pageNumber = value;
        }
    }

    public int PageSize
    {
        get => _pageSize;
        set
        {
            if (value < 1)
                throw new SynergyException("PageSize must be greater than 0.", "PAGINATION");
            if (value > MaxPageSize)
                throw new SynergyException($"PageSize must not exceed {MaxPageSize}.", "PAGINATION");
            _pageSize = value;
        }
    }
}
