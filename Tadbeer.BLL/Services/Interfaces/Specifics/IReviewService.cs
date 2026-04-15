using Tadbeer.DAL.DTO.Requests;
using Tadbeer.DAL.DTO.Responses;
using Tadbeer.DAL.Models;

namespace Tadbeer.BLL.Services.Interfaces.Specifics;

public interface IReviewService : IGenericService<ReviewRequestDto, ReviewResponseDto, Review>
{
	Task<ReviewResponseDto> CreateForUserAsync(Guid userId, ReviewRequestDto dto);
	Task<ReviewResponseDto?> UpdateOwnAsync(Guid userId, Guid reviewId, ReviewUpdateRequestDto dto);
	Task<bool> DeleteOwnAsync(Guid userId, Guid reviewId);
	Task<bool> DeleteAnyAsync(Guid reviewId);
	Task<ReviewPagedResponseDto> GetByWorkerPagedAsync(Guid workerId, int pageNumber, int pageSize);
}
