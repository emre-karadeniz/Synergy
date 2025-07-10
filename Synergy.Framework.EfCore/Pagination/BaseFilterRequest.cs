namespace Synergy.Framework.EfCore.Pagination;

public record BaseFilterRequest
{
    public PaginationRequest Pagination { get; set; } = new();
    public SortRequest? Sort { get; set; }
}
