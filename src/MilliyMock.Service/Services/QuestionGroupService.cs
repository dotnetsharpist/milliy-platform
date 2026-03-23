using AutoMapper;
using Microsoft.EntityFrameworkCore;
using MilliyMock.DataAccess.IRepositories;
using MilliyMock.Domain.Entities;
using MilliyMock.Service.Dtos.QuestionGroups;
using MilliyMock.Service.Interfaces;
using MilliyMock.Shared.Helpers;

namespace MilliyMock.Service.Services;

public class QuestionGroupService(IUnitOfWork unitOfWork, IMapper mapper) : IQuestionGroupService
{
    public async Task<bool> CreateAsync(CreateQuestionGroupDto dto)
    {
        var questionGroup = mapper.Map<QuestionGroup>(dto);
        questionGroup.CreatedBy = HttpContextHelper.UserId;
        await unitOfWork.QuestionGroups.InsertAsync(questionGroup);
        return await unitOfWork.QuestionGroups.SaveAsync();
    }

    public async Task<List<QuestionGroupResultDto>> GetByTestIdAsync(long testId)
    {
        var questiongroupsblyat = await unitOfWork.QuestionGroups.SelectAll().ToListAsync();
        var questionGroups = await unitOfWork.QuestionGroups
            .SelectAll(qg => qg.TestId == testId)
            .Include(g => g.Questions)
            .Include(g => g.Options)
            .ToListAsync();
        return mapper.Map<List<QuestionGroupResultDto>>(questionGroups);
    }
}