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
        var customerId = _currentUser.UserId!.Value;

        // Business rule: one active rating per customer per product.
        var existing = await _ratingRepository.GetByCustomerAndProductAsync(
            customerId, command.ProductId, ct);

        if (existing is not null)
        {
            // Update existing rating
            existing.UpdateStarRating(new StarRating(command.StarRating));
            await _ratingRepository.UpdateAsync(existing, ct);
            await _unitOfWork.SaveChangesAsync(ct);
            return Result<Guid>.Success(existing.Id);
        }

        var rating = Rating.Create(
            customerId,
            command.ProductId,
            new StarRating(command.StarRating));

        await _ratingRepository.AddAsync(rating, ct);
        await _unitOfWork.SaveChangesAsync(ct);

        return Result<Guid>.Success(rating.Id);
    }
}