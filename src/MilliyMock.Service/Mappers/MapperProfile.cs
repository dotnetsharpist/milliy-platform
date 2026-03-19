using AutoMapper;
using MilliyMock.Domain.Entities;
using MilliyMock.Service.Dtos.Options;
using MilliyMock.Service.Dtos.QuestionGroups;
using MilliyMock.Service.Dtos.Questions;
using MilliyMock.Service.Dtos.Tests;
using MilliyMock.Service.Dtos.UserAnswers;
using MilliyMock.Service.Dtos.Users;
using MilliyMock.Service.Dtos.UserTestAttempt;

namespace MilliyMock.Service.Mappers;

public class MapperProfile : Profile
{
    public MapperProfile()
    {
        // User
        CreateMap<User, CreateUserDto>().ReverseMap();
        CreateMap<User, UserResultDto>().ReverseMap();
        CreateMap<User, UpdateUserDto>().ReverseMap();
        
        
        // Test
        CreateMap<Test, CreateTestDto>().ReverseMap();
        CreateMap<Test, TestResultDto>().ReverseMap();
        
        // QuestionGroup
        CreateMap<QuestionGroup, CreateQuestionGroupDto>().ReverseMap();
        CreateMap<QuestionGroup, QuestionGroupResultDto>().ReverseMap();
        
        // Question
        CreateMap<Question, CreateQuestionDto>().ForMember(dest => dest.Image, opt => opt.Ignore());
        CreateMap<Question, QuestionResultDto>().ReverseMap();
        
        // Option
        CreateMap<Option, CreateOptionDto>().ReverseMap();
        CreateMap<Option, OptionResultDto>().ReverseMap();
        
        // UserTestAttempt
        CreateMap<UserTestAttempt, CreateUserTestAttemptDto>().ReverseMap();
        CreateMap<UserTestAttempt, UserTestAttemptResultDto>().ReverseMap();
        
        // UserAnswer
        CreateMap<UserAnswer, CreateUserAnswer>().ReverseMap();
        CreateMap<UserAnswer, UserAnswerResultDto>().ReverseMap();
    }
}