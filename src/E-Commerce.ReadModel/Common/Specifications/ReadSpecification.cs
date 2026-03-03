using E_Commerce.ReadModel.Common.Paging;
using E_Commerce.ReadModel.Common.Sorting;
using E_Commerce.ReadModel.Common.Filtering;

namespace E_Commerce.ReadModel.Common.Specifications;

public abstract class ReadSpecification<T>
{
    public PagingRequest? Paging { get; set; }
    public SortRequest? Sorting { get; set; }
    public List<FilterRule> Filters { get; set; } = new();
}
