using AutoMapper;
using Microsoft.Extensions.Logging;
using MilliyMock.Domain.Enums;
using MilliyMock.Service.Dtos.BotUsers;
using MilliyMock.Service.Interfaces;
using Telegram.Bot;
using Telegram.Bot.Exceptions;
using Telegram.Bot.Polling;
using Telegram.Bot.Types;

namespace MilliyMock.Service.Handlers;

public class UpdateHandler(ITelegramBotClient bot,
    ILogger<UpdateHandler> logger,
    IBotUserService botUserService,
    IUserService userService,
    IMapper mapper) : IUpdateHandler
{
    private static readonly InputPollOption[] PollOptions = ["Hello", "World!"];

    public async Task HandleErrorAsync(ITelegramBotClient botClient, Exception exception, HandleErrorSource source, CancellationToken cancellationToken)
    {
        logger.LogInformation("HandleError: {Exception}", exception);
        // Cooldown in case of network connection error
        if (exception is RequestException)
            await Task.Delay(TimeSpan.FromSeconds(2), cancellationToken);
    }

    public async Task HandleUpdateAsync(ITelegramBotClient botClient, Update update, CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();
        /*
        var dto = mapper.Map<BotUserCreationDto>(update.Message!.From);
        await botUserService.AddBotUser(dto);
        */
        await (update switch
        {
            { Message: { } message }                        => OnMessage(message),
            //{ EditedMessage: { } message }                  => OnMessage(message),
            _                                               => UnknownUpdateHandlerAsync(update)
        });
    }

    private async Task OnMessage(Message msg)
    {
        logger.LogInformation("Receive message type: {MessageType} from user: {User}", msg.Type, msg.From);
        
        var dto = mapper.Map<CreateBotUserDto>(msg.From);
        await botUserService.CreateAsync(dto);
        
        if (msg.Text is not { } messageText)
            return;
        
        await (messageText.Split(' ')[0] switch
        {
            "/start" => OnStart(msg),
            "/addbalance" => AddBalance(msg),
            _ => Usage(msg)
        });

        //var userId = msg.Chat.Id;
    }

    private async Task OnStart(Message msg) => await bot.SendMessage(msg.From!.Id, "hello");
    

    private async Task AddBalance(Message msg)
    {
        var sender = await userService.GeByTelegramUserId(msg.From!.Id);
        // if (sender is null || sender.Role == UserRole.User)
        // {
        //     await bot.SendMessage(msg.From!.Id, "You are not authorized to use this command.");
        //     return;
        // }

        var responseText = await botUserService.AddBalanceViaBotAsync(msg.Text!);
        await bot.SendMessage(msg.From!.Id, responseText);
    }
    
    /*
    private async Task MenuControl(Message msg)
    {
        if (msg.Text is not { } messageText)
            return;

        var sentMessage = await (messageText.Split(' ')[0] switch
        {
            _ => Usage(msg)
        });
    }
    */
    
 #region Usages

 async Task<Message> Usage(Message msg)
 {
     return await bot.SendMessage(msg.Chat,
         "Unknown command.\nUsage: /addbalance <user id> <amount>");
 }
 
 #endregion    
 

#region Fails
    private Task UnknownUpdateHandlerAsync(Update update)
    {
        logger.LogInformation("Unknown update type: {UpdateType}", update.Type);
        return Task.CompletedTask;
    }
    
    static Task<Message> FailingHandler(Message msg)
    {
        throw new NotImplementedException("FailingHandler");
    }
#endregion    
}