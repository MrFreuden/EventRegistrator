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

        public MessageHandler(UserRepository userRepository, MessageSender messageSender)
        {
            _userRepository = userRepository;
            _messageSender = messageSender;
            _privateMessageHandler = new PrivateMessageHandler(userRepository);
            _targetChatMessageHandler = new TargetChatMessageHandler(userRepository);
            _repositoryLoader = new RepositoryLoader(EnvLoader.GetDataPath());
        }

        public async Task ProcessMessage(Message message)
        {
            Response response = null;
            var messageDto = MessageMapper.Map(message);
            if (IsPrivateMessage(message))
            {
                response = _privateMessageHandler.Handle(messageDto);
            }
            else if (IsMessageFromTargetChat(message))
            {
                await ProcessMessagesAsync(_targetChatMessageHandler.Handle(messageDto));
                return;
            }
            else
            {
                response = new Response { ChatId = message.Chat.Id, Text = Constants.Error };
            }
            await _messageSender.SendMessage(response);
            await SaveRepositoryAsync();
        }

        public async Task ProcessEditMessage(Message message)
        {
            var m = MessageMapper.Map(message);
            if (IsMessageFromTargetChat(message))
            {
                _targetChatMessageHandler.HandleEdit(m);
            }
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