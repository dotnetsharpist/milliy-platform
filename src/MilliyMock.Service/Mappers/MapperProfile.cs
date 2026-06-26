using AutoMapper;
using MilliyMock.Domain.Entities;
using MilliyMock.Service.Dtos.BotUsers;
using MilliyMock.Service.Dtos.Options;
using MilliyMock.Service.Dtos.QuestionExplanations;
using MilliyMock.Service.Dtos.QuestionGroups;
using MilliyMock.Service.Dtos.Questions;
using MilliyMock.Service.Dtos.TempUsers;
using MilliyMock.Service.Dtos.Tests;
using MilliyMock.Service.Dtos.TransactionHistories;
using MilliyMock.Service.Dtos.UserAnswers;
using MilliyMock.Service.Dtos.UserBalances;
using MilliyMock.Service.Dtos.Users;
using MilliyMock.Service.Dtos.UserTestAttempt;
using TgUser = Telegram.Bot.Types.User;

namespace MilliyMock.Service.Mappers;

public class MapperProfile : Profile
{
    public MapperProfile()
    {
        // User
        CreateMap<User, CreateUserDto>().ReverseMap();
        CreateMap<User, UserResultDto>().ReverseMap();
        CreateMap<User, UpdateUserDto>().ReverseMap();
        
        CreateMap<TempUser, CreateTempUserDto>().ReverseMap();
        CreateMap<TempUser, TempUserResultDto>().ReverseMap();
        
        // Test
        CreateMap<Test, CreateTestDto>().ReverseMap();
        CreateMap<Test, UpdateTestDto>().ReverseMap();
        CreateMap<Test, TestResultDto>().ReverseMap();
        CreateMap<Test, StartTestResultDto>().ReverseMap();
        CreateMap<Test, ResumeAttemptDto>();
        CreateMap<Test, TestAttemptResultsDto>();
        
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
        CreateMap<QuestionGroup, QuestionGroupAttemptResultDto>().ReverseMap();
        
        // Question
        CreateMap<Question, CreateQuestionDto>()
            .ForMember(dest => dest.ImageUz, opt => opt.Ignore())
            //.ForMember(dest => dest.ImageRu, opt => opt.Ignore())
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
        CreateMap<Question, QuestionAttemptResultDto>().ReverseMap();
        CreateMap<Question, QuestionAttemptForQuestionGroupDto>().ReverseMap();
        CreateMap<Question, QuestionResultForGroupDto>().ReverseMap();

        // Question Explanation
        CreateMap<QuestionExplanation, CreateQuestionExplanationDto>().ReverseMap();
        CreateMap<QuestionExplanation, QuestionExplanationResultDto>().ReverseMap();
        CreateMap<QuestionExplanation, UpdateQuestionExplanationDto>().ReverseMap();
        
        // Option
        CreateMap<Option, CreateOptionDto>().ReverseMap();
        CreateMap<CreateQuestionOptionDto, Option>();
        CreateMap<UpdateQuestionOptionDto, Option>()
            .ForMember(dest => dest.Id, opt => opt.Ignore());
        CreateMap<UpdateOptionDto, Option>().ReverseMap();
        CreateMap<Option, OptionResultDto>().ReverseMap();
        CreateMap<Option, OptionAttemptDto>().ReverseMap();
        CreateMap<Option, OptionAttemptResultDto>().ReverseMap();
        
        // UserTestAttempt
        CreateMap<UserTestAttempt, CreateUserTestAttemptDto>().ReverseMap();
        CreateMap<UserTestAttempt, UserTestAttemptResultDto>().ReverseMap();
        
        // UserAnswer
        CreateMap<UserAnswer, CreateUserAnswerDto>().ReverseMap();
        CreateMap<UserAnswer, UserAnswerResultDto>().ReverseMap();
        CreateMap<UserAnswer, UserAnswerAttemptResultDto>();
        
        // Bot user
        CreateMap<BotUser, CreateBotUserDto>().ReverseMap();
        CreateMap<BotUser, BotUserResultDto>().ReverseMap();
        CreateMap<TgUser, CreateBotUserDto>()
            .ForMember(dest => dest.TgUserId, opt => opt.MapFrom(src => src.Id))
            .ForMember(dest => dest.FullName,
                opt => opt.MapFrom(src => (src.FirstName + " " + src.LastName).Trim()));

        // Balance & Transactions
        CreateMap<UserBalance, UserBalanceResultDto>().ReverseMap();
        CreateMap<TransactionHistory, TransactionHistoryResultDto>().ReverseMap();
    }
}