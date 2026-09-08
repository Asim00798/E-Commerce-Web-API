using E_Commerce.Application.Shared.Models;
using E_Commerce.Application.Shared.Security.Identity;
using E_Commerce.Domain.BoundedContexts.Core.CustomerEngagement.AggregateRoots.Rating.Behaviors;
using E_Commerce.Domain.BoundedContexts.Core.CustomerEngagement.AggregateRoots.Rating.ValueObjects;
using E_Commerce.Domain.BoundedContexts.Core.CustomerEngagement.Repositories;
using E_Commerce.Domain.SharedKernel.PersistenceAbstractions;
using MediatR;

namespace E_Commerce.Application.BoundedContexts.CustomerEngagement.Ratings.Commands.SubmitRating;

public sealed class SubmitRatingCommandHandler
    : IRequestHandler<SubmitRatingCommand, Result<Guid>>
{
    private readonly IRatingRepository _ratingRepository;
    private readonly ICurrentUser _currentUser;
    private readonly IUnitOfWork _unitOfWork;

    public SubmitRatingCommandHandler(
        IRatingRepository ratingRepository,
        ICurrentUser currentUser,
        IUnitOfWork unitOfWork)
    {
        _ratingRepository = ratingRepository;
        _currentUser = currentUser;
        _unitOfWork = unitOfWork;
    }

    public async Task<Result<Guid>> Handle(
        SubmitRatingCommand command,
        CancellationToken ct)
    {
        var customerId = GetCustomerId();

        var existing = await GetExistingRatingAsync(
            customerId,
            command.ProductId,
            ct);

        if (existing is not null)
        {
            await UpdateExistingRatingAsync(existing, command, ct);
            return Result<Guid>.Success(existing.Id);
        }

        var ratingId = await CreateNewRatingAsync(
            customerId,
            command,
            ct);

        return Result<Guid>.Success(ratingId);
    }

    private Guid GetCustomerId()
    {
        return _currentUser.UserId!.Value;
    }

    private async Task<Rating?> GetExistingRatingAsync(
        Guid customerId,
        Guid productId,
        CancellationToken ct)
    {
        return await _ratingRepository.GetByCustomerAndProductAsync(
            customerId,
            productId,
            ct);
    }

    private async Task UpdateExistingRatingAsync(
        Rating existing,
        SubmitRatingCommand command,
        CancellationToken ct)
    {
        existing.UpdateStarRating(
            new StarRating(command.StarRating));

        await _ratingRepository.UpdateAsync(existing, ct);
        await _unitOfWork.SaveChangesAsync(ct);
    }

    private async Task<Guid> CreateNewRatingAsync(
        Guid customerId,
        SubmitRatingCommand command,
        CancellationToken ct)
    {
        var rating = Rating.Create(
            customerId,
            command.ProductId,
            new StarRating(command.StarRating));

        await _ratingRepository.AddAsync(rating, ct);
        await _unitOfWork.SaveChangesAsync(ct);

        return rating.Id;
    }
}