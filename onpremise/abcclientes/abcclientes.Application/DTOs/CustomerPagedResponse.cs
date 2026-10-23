namespace abcclientes.Application.DTOs;

/// <summary>
/// Paged response containing a list of customers and pagination metadata.
/// </summary>
public class CustomerPagedResponse
{
    public IReadOnlyList<CustomerResponse> Items { get; set; } = new List<CustomerResponse>();

    public int TotalCount { get; set; }

    public int Page { get; set; }

    public int PageSize { get; set; }

    public int TotalPages { get; set; }
}
