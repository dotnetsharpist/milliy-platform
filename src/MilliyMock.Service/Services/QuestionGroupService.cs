using AutoMapper;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using MilliyMock.DataAccess.IRepositories;
using MilliyMock.Domain.Entities;
using MilliyMock.Domain.Exceptions;
using MilliyMock.Service.Dtos.QuestionGroups;
using MilliyMock.Service.Interfaces;
using MilliyMock.Shared.Helpers;

namespace MilliyMock.Service.Services;

public class QuestionGroupService(
    IUnitOfWork unitOfWork,
    IMapper mapper,
    ILogger<QuestionGroupService> logger) : IQuestionGroupService
{
    public async Task<bool> CreateAsync(CreateQuestionGroupDto dto)
    {
        try
        {
            var questionGroup = mapper.Map<QuestionGroup>(dto);
            questionGroup.CreatedBy = HttpContextHelper.UserId;
            await unitOfWork.QuestionGroups.InsertAsync(questionGroup);
            return await unitOfWork.QuestionGroups.SaveAsync();
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error creating QuestionGroup {text}", dto.Title);
            throw new MilliyMockException();
        }
    }

    public async Task<List<QuestionGroupResultDto>> GetByTestIdAsync(long testId)
    {
        var questionGroups = await unitOfWork.QuestionGroups
            .SelectAll(qg => qg.TestId == testId)
            .Include(g => g.Questions)
            .Include(g => g.Options)
            .ToListAsync();
        
        return mapper.Map<List<QuestionGroupResultDto>>(questionGroups);
    }
}