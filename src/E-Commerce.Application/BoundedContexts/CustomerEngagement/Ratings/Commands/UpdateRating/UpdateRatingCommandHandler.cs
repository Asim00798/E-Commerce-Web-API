using E_Commerce.Application.Shared.Models;
using E_Commerce.Application.Shared.Security.Identity;
using E_Commerce.Domain.BoundedContexts.Core.CustomerEngagement.AggregateRoots.Rating.ValueObjects;
using E_Commerce.Domain.BoundedContexts.Core.CustomerEngagement.Repositories;
using E_Commerce.Domain.SharedKernel.PersistenceAbstractions;
using MediatR;

namespace E_Commerce.Application.BoundedContexts.CustomerEngagement.Ratings.Commands.UpdateRating;

public sealed class UpdateRatingCommandHandler
    : IRequestHandler<UpdateRatingCommand, Result>
{
    private readonly IRatingRepository _ratingRepository;
    private readonly ICurrentUser _currentUser;
    private readonly IUnitOfWork _unitOfWork;

    public UpdateRatingCommandHandler(
        IRatingRepository ratingRepository,
        ICurrentUser currentUser,
        IUnitOfWork unitOfWork)
    {
        _ratingRepository = ratingRepository;
        _currentUser = currentUser;
        _unitOfWork = unitOfWork;
    }

    public async Task<Result> Handle(
        UpdateRatingCommand command,
        CancellationToken ct)
    {
        var rating = await _ratingRepository.GetByIdAsync(command.RatingId, ct);
        if (rating is null)
            return Result.Failure("Rating not found.");

        // Resource authorization: customer can only update their own rating.
        if (rating.CustomerId != _currentUser.UserId!.Value)
            return Result.Failure("You are not authorized to update this rating.");

        rating.UpdateStarRating(new StarRating(command.StarRating));
        await _ratingRepository.UpdateAsync(rating, ct);
        await _unitOfWork.SaveChangesAsync(ct);

        return Result.Success();
    }
}