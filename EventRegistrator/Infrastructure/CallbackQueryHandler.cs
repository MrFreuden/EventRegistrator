using EventRegistrator.Application;
using EventRegistrator.Application.Objects.DTOs;
using EventRegistrator.Domain;
using Telegram.Bot.Types;

namespace EventRegistrator.Infrastructure
{
    public class CallbackQueryHandler
    {
        private readonly MessageSender _messageSender;
        private readonly UpdateRouter _updateRouter;

        public CallbackQueryHandler(MessageSender messageSender, UpdateRouter updateRouter)
        {
            _messageSender = messageSender;
            _updateRouter = updateRouter;
        }

        public async Task ProcessCallbackQuery(CallbackQuery callbackQuery)
        {
            await _messageSender.AnswerAsync(callbackQuery.Id);
            var messageDto = UpdateMapper.Map(callbackQuery);
            var responses = await _updateRouter.RouteCallback(messageDto);
            await ProcessMessagesAsync(responses);
        }

        private async Task ProcessMessagesAsync(List<Response> messages)
        {
            foreach (var message in messages)
            {
                var sentMessage = await _messageSender.SendMessage(message);

                message.SaveMessageIdCallback?.Invoke(sentMessage.MessageId);
            }
        }
    }
}