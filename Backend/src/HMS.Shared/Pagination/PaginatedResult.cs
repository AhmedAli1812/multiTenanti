public class PaginatedResult<T>
{
    public int PageNumber { get; set; }
    public int PageSize { get; set; }

    public int TotalCount { get; set; }
    public int TotalPages { get; set; } // 🔥 مهم

    public List<T> Items { get; set; } = new();
}