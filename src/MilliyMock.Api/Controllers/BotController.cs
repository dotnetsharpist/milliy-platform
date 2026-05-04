using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using MilliyMock.Models;
using Telegram.Bot;
using Telegram.Bot.Polling;
using Telegram.Bot.Types;

namespace MilliyMock.Controllers;

[Route("api/bot")]
public class BotController(
    IOptions<BotConfiguration> config,
    IUpdateHandler updateHandler) : ControllerBase
{
    [HttpGet("setWebhook")]
    public async Task<string> SetWebHook([FromServices] ITelegramBotClient bot, CancellationToken ct)
    {
        var webhookUrl = config.Value.BotWebhookUrl.AbsoluteUri;
        await bot.SetWebhook(webhookUrl, allowedUpdates: [], secretToken: config.Value.SecretToken, maxConnections: 10, dropPendingUpdates: true, cancellationToken: ct);
        return $"Webhook set to {webhookUrl}";
    }

    [HttpPost]
    public async Task<IActionResult> Post([FromBody] Update update, [FromServices] ITelegramBotClient bot,
        CancellationToken ct)
    {
        if (Request.Headers["X-Telegram-Bot-Api-Secret-Token"] != config.Value.SecretToken)
            return Forbid();

        return Ok();
    }
}