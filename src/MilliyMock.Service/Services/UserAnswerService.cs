using AutoMapper;
using MilliyMock.DataAccess.IRepositories;
using MilliyMock.Domain.Entities;
using MilliyMock.Service.Dtos.UserAnswers;
using MilliyMock.Service.Interfaces;

namespace MilliyMock.Service.Services;

public class UserAnswerService(IUnitOfWork unitOfWork, IMapper mapper) : IUserAnswerService
{
    public async Task<bool> CreateAsync(CreateUserAnswerDto dto)
    {
        var answer = mapper.Map<UserAnswer>(dto);
        await unitOfWork.UserAnswer.InsertAsync(answer);
        return await unitOfWork.UserAnswer.SaveAsync();
    }
}