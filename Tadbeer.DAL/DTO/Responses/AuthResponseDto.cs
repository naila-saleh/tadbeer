namespace Tadbeer.DAL.DTO.Responses;

public class AuthResponseDto
{
    public bool IsSuccess { get; set; }
    public string? Token { get; set; }
    public string? Message { get; set; }
    public IEnumerable<string>? Errors { get; set; }
}
