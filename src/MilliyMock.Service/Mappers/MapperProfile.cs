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
        CreateMap<Test, UpdateTestDto>().ReverseMap();
        CreateMap<Test, TestResultDto>().ReverseMap();
        CreateMap<Test, FullTestResultDto>().ReverseMap();
        
        // QuestionGroup
        CreateMap<QuestionGroup, CreateQuestionGroupDto>()
            .ForMember(dest => dest.Image, opt => opt.Ignore())
            .ReverseMap();
        CreateMap<QuestionGroup, QuestionGroupResultDto>().ReverseMap();
        CreateMap<QuestionGroup, QuestionGroupAttemptDto>().ReverseMap();
        
        // Question
        CreateMap<Question, CreateQuestionDto>().ForMember(dest => dest.Image, opt => opt.Ignore()).ReverseMap();
        CreateMap<UpdateQuestionDto, Question>()
            .ForMember(dest => dest.ImagePath, opt => opt.Ignore())
            .ForMember(dest => dest.Options, opt => opt.Ignore());
        CreateMap<Question, QuestionResultDto>().ReverseMap();
        CreateMap<Question, QuestionAttemptDto>().ReverseMap();
        CreateMap<Question, QuestionAttemptForQuestionGroupDto>().ReverseMap();
        
        // Option
        CreateMap<Option, CreateOptionDto>().ReverseMap();
        CreateMap<CreateQuestionOptionDto, Option>();
        CreateMap<UpdateOptionDto, Option>().ReverseMap();
        CreateMap<Option, OptionResultDto>().ReverseMap();
        CreateMap<Option, OptionAttemptDto>().ReverseMap();
        
        // UserTestAttempt
        CreateMap<UserTestAttempt, CreateUserTestAttemptDto>().ReverseMap();
        CreateMap<UserTestAttempt, UserTestAttemptResultDto>().ReverseMap();
        
        // UserAnswer
        CreateMap<UserAnswer, CreateUserAnswerDto>().ReverseMap();
        CreateMap<UpdateUserAnswerDto, UserAnswer>();
        CreateMap<UserAnswer, UserAnswerResultDto>().ReverseMap();
    }
}