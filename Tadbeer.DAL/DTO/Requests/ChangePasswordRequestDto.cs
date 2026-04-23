using System.ComponentModel.DataAnnotations;

namespace Tadbeer.DAL.DTO.Requests;

public class ChangePasswordRequestDto
{
    [Required]
    [DataType(DataType.Password)]
    public string CurrentPassword { get; set; } = null!;

    [Required]
    [DataType(DataType.Password)]
    [StringLength(100, MinimumLength = 6)]
    public string NewPassword { get; set; } = null!;

    [Required]
    [DataType(DataType.Password)]
    [Compare(nameof(NewPassword), ErrorMessage = "The new password and confirmation password do not match.")]
    public string ConfirmNewPassword { get; set; } = null!;
}

