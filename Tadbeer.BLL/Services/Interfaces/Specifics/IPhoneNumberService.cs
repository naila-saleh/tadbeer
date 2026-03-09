using Tadbeer.DAL.DTO.Requests;
using Tadbeer.DAL.DTO.Responses;
using Tadbeer.DAL.Models;

namespace Tadbeer.BLL.Services.Interfaces.Specifics;

public interface IPhoneNumberService : IGenericService<PhoneNumberRequestDto, PhoneNumberResponseDto, PhoneNumber>
{
}
