using System;
using MilliyMock.Domain.Enums;

namespace MilliyMock.Service.Dtos.Auth;

public class RegisterDto
{
    public string FullName { get; set; } = string.Empty;
    public string Username { get; set; } = string.Empty;
    public DateOnly DateOfBirth { get; set; }
    public UserRole Role { get; set; } = UserRole.User;
}