using EventRegistrator.Application;
using EventRegistrator.Application.Objects.DTOs;
using EventRegistrator.Domain;
using EventRegistrator.Domain.Models;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;

namespace EventRegistrator.Infrastructure
{
    public class MessageHandler
    {
        private readonly MessageSender _messageSender;
        private readonly UpdateRouter _updateRouter;

        public MessageHandler(MessageSender messageSender, UpdateRouter updateRouter)
        {
            _messageSender = messageSender;
            _updateRouter = updateRouter;
        }

        public async Task ProcessMessage(Message message)
        {
            if (message.Type == MessageType.MigrateFromChatId || message.Type == MessageType.MigrateToChatId || message.Text == null) return;
            var messageDto = UpdateMapper.Map(message);
            var responses = GetResponse(messageDto);
            await ProcessMessagesAsync(responses.Result);
        }

        public async Task ProcessEditMessage(Message message)
        {
            var messageDto = UpdateMapper.Map(message);
            messageDto.IsEdit = true;
            var responses = GetResponse(messageDto);
            await ProcessMessagesAsync(responses.Result);
        }

        private async Task<List<Response>> GetResponse(MessageDTO message)
        {
            return await _updateRouter.RouteMessage(message);
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