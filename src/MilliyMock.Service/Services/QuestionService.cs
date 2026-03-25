using AutoMapper;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using MilliyMock.DataAccess.IRepositories;
using MilliyMock.Domain.Entities;
using MilliyMock.Domain.Exceptions;
using MilliyMock.Service.Dtos.Questions;
using MilliyMock.Service.Interfaces;
using MilliyMock.Shared.Helpers;

namespace MilliyMock.Service.Services;

public class QuestionService(
    IUnitOfWork unitOfWork, 
    IFileService fileService,
    IMapper mapper,
    ILogger<QuestionService> logger) : IQuestionService
{
    public async Task<bool> CreateAsync(CreateQuestionDto dto)
    {
        try
        {
            var test = await unitOfWork.Tests.SelectAsync(t => t.Id == dto.TestId);
            if (test is null) throw new MilliyMockException(404, "Test not found");

            if (dto.QuestionGroupId is not null)
            {
                var questionGroup = await unitOfWork.QuestionGroups.SelectAsync(qg => qg.Id == dto.QuestionGroupId);
                if (questionGroup is null) throw new MilliyMockException(404, "Question group not found");
            }

            var question = mapper.Map<Question>(dto);
            question.CreatedBy = HttpContextHelper.UserId;
            if (dto.Image is not null)
            {
                var imagePath = await fileService.UploadImage(dto.Image);
                question.ImagePath = imagePath;
            }

            await unitOfWork.Questions.InsertAsync(question);
            return await unitOfWork.Questions.SaveAsync();
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error creating question {text}", dto.Text);
            throw new MilliyMockException();
        }
    }

    public async Task<List<QuestionResultDto>> GetByTestIdAsync(long testId)
    {
        var questions = await unitOfWork.Questions.SelectAll(q => q.TestId == testId).ToListAsync();
        return mapper.Map<List<QuestionResultDto>>(questions);
    }
}