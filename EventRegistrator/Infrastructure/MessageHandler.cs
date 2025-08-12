using EventRegistrator.Application;
using EventRegistrator.Application.DTOs;
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
        private readonly MessageRouter _messageRouter;

        public MessageHandler(UserRepository userRepository, MessageSender messageSender)
        {
            _userRepository = userRepository;
            _messageSender = messageSender;
            _privateMessageHandler = new PrivateMessageHandler(userRepository);
            _targetChatMessageHandler = new TargetChatMessageHandler(userRepository);
            _repositoryLoader = new RepositoryLoader(EnvLoader.GetDataPath());
            _messageRouter = new MessageRouter([_privateMessageHandler, _targetChatMessageHandler]);
        }

        public async Task ProcessMessage(Message message)
        {
            var messageDto = MessageMapper.Map(message);
            var response = _messageRouter.Route(messageDto);
            await ProcessMessagesAsync(response.Result);
            await SaveRepositoryAsync();
        }

        public async Task ProcessEditMessage(Message message)
        {
            await ProcessMessage(message);
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