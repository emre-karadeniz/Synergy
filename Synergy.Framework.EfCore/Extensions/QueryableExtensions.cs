using Microsoft.EntityFrameworkCore;
using Synergy.Framework.EfCore.Pagination;
using System.Linq.Dynamic.Core;

namespace Synergy.Framework.EfCore.Extensions;


public static class QueryableExtensions
{
    public static async Task<PaginationResponse<T>> ToPagedListAsync<T>(
    this IQueryable<T> query,
    PaginationRequest pagination,
    CancellationToken cancellationToken = default)
    {
        int totalRecords = await query.CountAsync(cancellationToken);
        List<T> dataList = await query.Skip((pagination.PageNumber - 1) * pagination.PageSize)
                                  .Take(pagination.PageSize)
                                  .ToListAsync(cancellationToken);

        return new PaginationResponse<T>(dataList, totalRecords, pagination.PageNumber, pagination.PageSize);
    }

    public static async Task<T?> FirstOrDefaultAsync<T>(
    this IQueryable<T> query,
    CancellationToken cancellationToken = default)
    {
        return await Microsoft.EntityFrameworkCore.EntityFrameworkQueryableExtensions
            .FirstOrDefaultAsync(query, cancellationToken);
    }

    public static async Task<List<T>> ToListAsync<T>(
   this IQueryable<T> query,
   CancellationToken cancellationToken = default)
    {
        return await Microsoft.EntityFrameworkCore.EntityFrameworkQueryableExtensions
            .ToListAsync(query, cancellationToken);
    }

    public static IQueryable<T> ApplySorting<T>(this IQueryable<T> query, SortRequest? sortRequest, string defaultOrderBy = "CreatedAt")
    {
        string? orderBy = sortRequest?.OrderBy;
        bool descending = sortRequest?.Descending ?? true;

        // Eğer OrderBy boşsa default kolonu kullan
        if (string.IsNullOrWhiteSpace(orderBy))
        {
            orderBy = defaultOrderBy;
        }

        var direction = descending ? "descending" : "ascending";
        return query.OrderBy($"{orderBy} {direction}");
    }
}

