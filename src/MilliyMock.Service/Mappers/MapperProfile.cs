using AutoMapper;
using MilliyMock.Domain.Entities;
using MilliyMock.Service.Dtos.Options;
using MilliyMock.Service.Dtos.QuestionExplanations;
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
            .ForMember(dest => dest.ImageUz, opt => opt.Ignore())
            .ForMember(dest => dest.ImageRu, opt => opt.Ignore())
            .ReverseMap();
        CreateMap<UpdateQuestionGroupDto, QuestionGroup>()
            .ForMember(dest => dest.Translations, opt => opt.Ignore());
            //.ForMember(dest => dest.ImageUz, opt => opt.Ignore())
            //.ForMember(dest => dest.ImageRu, opt => opt.Ignore());
        CreateMap<QuestionGroup, QuestionGroupResultDto>()
            .ForMember(dest => dest.QuestionCount, opt => opt.MapFrom(src => src.Questions.Count))
            .ForMember(dest => dest.Translations, opt => opt.MapFrom(src => src.Translations))
            .ForMember(dest => dest.Questions, opt => opt.MapFrom(src => src.Questions))
            .ForMember(dest => dest.Options, opt => opt.MapFrom(src => src.Options))
            .ReverseMap();
        CreateMap<QuestionGroup, QuestionGroupAttemptDto>().ReverseMap();
        
        // Question
        CreateMap<Question, CreateQuestionDto>()
            .ForMember(dest => dest.ImageUz, opt => opt.Ignore())
            .ForMember(dest => dest.ImageRu, opt => opt.Ignore())
            .ReverseMap();
        CreateMap<UpdateQuestionDto, Question>()
            .ForMember(dest => dest.Options, opt => opt.Ignore())
            .ForMember(dest => dest.Translations, opt => opt.Ignore());
        CreateMap<Question, QuestionResultDto>()
            .ForMember(dest => dest.Translations, opt => opt.MapFrom(src => src.Translations))
            .ForMember(dest => dest.Options, opt => opt.MapFrom(src => src.Options))
            .ReverseMap();
        CreateMap<Translation, TranslationResultDto>();
        CreateMap<Question, QuestionAttemptDto>().ReverseMap();
        CreateMap<Question, QuestionAttemptForQuestionGroupDto>().ReverseMap();

        // Question Explanation
        CreateMap<QuestionExplanation, CreateQuestionExplanationDto>()
            .ForMember(dest => dest.Image, opt => opt.Ignore())
            .ReverseMap();
        CreateMap<QuestionExplanation, QuestionExplanationResultDto>().ReverseMap();
        CreateMap<QuestionExplanation, UpdateQuestionExplanationDto>()
            .ForMember(dest => dest.Image, opt => opt.Ignore())
            .ReverseMap();
        
        // Option
        CreateMap<Option, CreateOptionDto>().ReverseMap();
        CreateMap<CreateQuestionOptionDto, Option>();
        CreateMap<UpdateQuestionOptionDto, Option>()
            .ForMember(dest => dest.Id, opt => opt.Ignore());
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