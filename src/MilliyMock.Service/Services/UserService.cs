using AutoMapper;
using MilliyMock.DataAccess.IRepositories;
using MilliyMock.Domain.Entities;
using MilliyMock.Domain.Exceptions;
using MilliyMock.Service.Dtos.Users;
using MilliyMock.Service.Interfaces;
using MilliyMock.Shared.Helpers;
using Microsoft.EntityFrameworkCore;

namespace MilliyMock.Service.Services;

public class UserService(IUnitOfWork unitOfWork, IMapper mapper) : IUserService
{
    public async ValueTask<bool> Add(CreateUserDto dto)
    {
        var exists = await unitOfWork.Users
            .SelectAsync(u => u.Email == dto.Email);

        if (exists == null)
            throw new MilliyMockException(409, "User with ts Email already exists");

        var user = mapper.Map<User>(dto);

        await unitOfWork.Users.InsertAsync(user);
        await unitOfWork.SaveChangesAsync();

        return true;
    }

    public async ValueTask<bool> Update(UpdateUserDto dto)
    {
        var userId = HttpContextHelper.UserId ?? throw new MilliyMockException();
        var user = await unitOfWork.Users
            .SelectAsync(u => u.Id == userId);

        if (HttpContextHelper.UserId != user.Id)
            throw new MilliyMockException(409, "nah");

        if (user is null)
            throw new MilliyMockException(404, "User not found");
        
        mapper.Map(dto, user);
        unitOfWork.Users.Update(user);

        await unitOfWork.SaveChangesAsync();
        return true;
    }

    public async ValueTask<bool> Delete(long id)
    {
        var user = await unitOfWork.Users
            .SelectAsync(u => u.Id == id);

        if (user is null)
            throw new MilliyMockException(404, "User not found");

        await unitOfWork.Users.DeleteAsync(u => u.Id == id);
        await unitOfWork.SaveChangesAsync();

        return true;
    }

    
    public async ValueTask<List<UserResultDto>> GetAll()
    {
        var users = await unitOfWork.Users
            .SelectAll(u => !u.IsDeleted).ToListAsync();

        return mapper.Map<List<UserResultDto>>(users);
    }
    
    public async ValueTask<UserResultDto> GeById(long id)
    {
        var user = await unitOfWork.Users
            .SelectAsync(u => u.Id == id && !u.IsDeleted);

        return user is null ? throw new MilliyMockException(404, "User not found") : mapper.Map<UserResultDto>(user);
    }

    public async ValueTask<UserResultDto?> GeByTelegramUserId(long id)
    {
        var user = await unitOfWork.Users.SelectAsync(u => u.BotUserId == id);
        return mapper.Map<UserResultDto>(user);
    }

    public async ValueTask<UserResultDto> GetMe()
    {
        var userId = HttpContextHelper.UserId;
        if (userId == null) throw new MilliyMockException();
        var user = await unitOfWork.Users.SelectAsync(u => u.Id == userId);
        return mapper.Map<UserResultDto>(user);
    }
}