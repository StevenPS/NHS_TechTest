using System.ComponentModel.DataAnnotations;

namespace ClinicSpace.Api.Contracts;

public sealed record LoginRequest(
    [Required, EmailAddress, StringLength(256)] string Email,
    [Required, StringLength(128)] string Password);
