namespace E_Commerce.ReadModel.Common.Paging;

public record PagingRequest(int PageNumber = 1, int PageSize = 10);
