using AutoMapper;
using Microsoft.EntityFrameworkCore;
using MilliyMock.DataAccess.IRepositories;
using MilliyMock.Domain.Entities;
using MilliyMock.Domain.Exceptions;
using MilliyMock.Service.Dtos.Questions;
using MilliyMock.Service.Interfaces;

namespace MilliyMock.Service.Services;

public class QuestionService(IUnitOfWork unitOfWork, IFileService fileService, IMapper mapper) : IQuestionService
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
            throw new MilliyMockException();
        }
    }

    public async Task<List<QuestionResultDto>> GetByTestIdAsync(long testId)
    {
        var questions = await unitOfWork.Questions.SelectAll(q => q.TestId == testId).ToListAsync();
        return mapper.Map<List<QuestionResultDto>>(questions);
    }
}