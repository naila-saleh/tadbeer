using Tadbeer.DAL.DTO.Requests;
using Tadbeer.DAL.DTO.Responses;
using Tadbeer.DAL.Models;

namespace Tadbeer.BLL.Services.Interfaces.Specifics;

public interface IPhoneNumberService : IGenericService<PhoneNumberRequestDto, PhoneNumberResponseDto, PhoneNumber>
{
	Task<PhoneNumberResponseDto> CreateOwnAsync(Guid userId, PhoneNumberRequestDto dto);
	Task<PhoneNumberResponseDto?> UpdateOwnAsync(Guid userId, Guid phoneNumberId, PhoneNumberRequestDto dto);
	Task<bool> DeleteOwnAsync(Guid userId, Guid phoneNumberId);
	Task<PhoneNumberPagedResponseDto> GetByWorkerPagedAsync(Guid workerId, int pageNumber, int pageSize);
	Task<IEnumerable<PhoneNumberResponseDto>> GetOwnAsync(Guid userId);
}
