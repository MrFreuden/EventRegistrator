using EventRegistrator.Application;
using EventRegistrator.Application.DTOs;
using EventRegistrator.Domain.Models;
using Telegram.Bot.Types;

namespace EventRegistrator.Infrastructure
{
    public class MessageHandler
    {
        private readonly UserRepository _userRepository;
        private readonly MessageSender _messageSender;
        private readonly PrivateMessageHandler _privateMessageHandler;
        private readonly TargetChatMessageHandler _targetChatMessageHandler; 
        private readonly RepositoryLoader _repositoryLoader;
        private readonly UpdateRouter _messageRouter;

        public MessageHandler(UserRepository userRepository, MessageSender messageSender)
        {
            _userRepository = userRepository;
            _messageSender = messageSender;
            _privateMessageHandler = new PrivateMessageHandler(userRepository);
            _targetChatMessageHandler = new TargetChatMessageHandler(userRepository);
            _repositoryLoader = new RepositoryLoader(EnvLoader.GetDataPath());
            _messageRouter = new UpdateRouter([_privateMessageHandler, _targetChatMessageHandler]);
        }

        public async Task ProcessMessage(Message message)
        {
            var messageDto = MessageMapper.Map(message);
            var responses = GetResponse(messageDto);
            await ProcessMessagesAsync(responses.Result);
            await SaveRepositoryAsync();
        }

        public async Task ProcessEditMessage(Message message)
        {
            var messageDto = MessageMapper.Map(message);
            messageDto.IsEdit = true;
            var responses = GetResponse(messageDto);
            await ProcessMessagesAsync(responses.Result);
            await SaveRepositoryAsync();
        }

        private async Task<List<Response>> GetResponse(MessageDTO message)
        {
            return await _messageRouter.RouteMessage(message);
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