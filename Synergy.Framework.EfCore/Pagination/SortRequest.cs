namespace Synergy.Framework.EfCore.Pagination;

public record SortRequest(string? OrderBy, bool Descending = false);
