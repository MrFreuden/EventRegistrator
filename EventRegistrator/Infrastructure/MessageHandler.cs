using EventRegistrator.Application;
using EventRegistrator.Application.DTOs;
using EventRegistrator.Domain;
using EventRegistrator.Domain.Models;
using Telegram.Bot.Types;

namespace EventRegistrator.Infrastructure
{
    public class MessageHandler
    {
        private readonly IUserRepository _userRepository;
        private readonly MessageSender _messageSender;
        private readonly RepositoryLoader _repositoryLoader;
        private readonly UpdateRouter _updateRouter;

        public MessageHandler(IUserRepository userRepository, MessageSender messageSender, RepositoryLoader repositoryLoader, UpdateRouter updateRouter)
        {
            _userRepository = userRepository;
            _messageSender = messageSender;
            _repositoryLoader = repositoryLoader;
            _updateRouter = updateRouter;
        }

        public async Task ProcessMessage(Message message)
        {
            var messageDto = UpdateMapper.Map(message);
            var responses = GetResponse(messageDto);
            await ProcessMessagesAsync(responses.Result);
            await SaveRepositoryAsync();
        }

        public async Task ProcessEditMessage(Message message)
        {
            var messageDto = UpdateMapper.Map(message);
            messageDto.IsEdit = true;
            var responses = GetResponse(messageDto);
            await ProcessMessagesAsync(responses.Result);
            await SaveRepositoryAsync();
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

        private async Task SaveRepositoryAsync()
        {
            await _repositoryLoader.SaveDataAsync(_userRepository as UserRepository);
        }
    }
}