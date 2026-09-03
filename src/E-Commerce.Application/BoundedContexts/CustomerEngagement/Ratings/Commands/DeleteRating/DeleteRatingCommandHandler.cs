using E_Commerce.Application.Shared.Models;
using E_Commerce.Application.Shared.Security.Identity;
using E_Commerce.Domain.BoundedContexts.Core.CustomerEngagement.Repositories;
using MediatR;

namespace E_Commerce.Application.BoundedContexts.CustomerEngagement.Ratings.Commands.DeleteRating;

public sealed class DeleteRatingCommandHandler
    : IRequestHandler<DeleteRatingCommand, Result>
{
    private readonly IRatingRepository _ratingRepository;
    private readonly ICurrentUser _currentUser;
    //No need for IUnitOfWork here since we are not performing
    //any transactional operations that require committing changes to the database.
    public DeleteRatingCommandHandler(
        IRatingRepository ratingRepository,
        ICurrentUser currentUser)
    {
        _ratingRepository = ratingRepository;
        _currentUser = currentUser;
    }

    public async Task<Result> Handle(
        DeleteRatingCommand command,
        CancellationToken ct)
    {
        var rating = await _ratingRepository.GetByIdAsync(command.RatingId, ct);
        if (rating is null)
            return Result.Failure("Rating not found.");

        // Resource authorization
        if (rating.CustomerId != _currentUser.UserId!.Value)
            return Result.Failure("You are not authorized to delete this rating.");

        // Hard delete the rating
        var deleted = await _ratingRepository.HardDeleteAsync(command.RatingId, ct);
        if (!deleted)
            return Result.Failure("Rating could not be deleted.");

        return Result.Success();
    }
}