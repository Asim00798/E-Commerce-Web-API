using MediatR;

namespace E_Commerce.ReadModel.Abstractions;

public interface IQuery<out TResponse> : IRequest<TResponse>
{
}
