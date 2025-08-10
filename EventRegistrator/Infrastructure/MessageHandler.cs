using EventRegistrator.Application;
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
            MessageDTO messageDTO = null;
            if (IsPrivateMessage(message))
            {
                messageDTO = await _privateMessageHandler.Handle(message);
            }
            else if (IsMessageFromTargetChat(message))
            {
                await ProcessMessagesAsync(await _targetChatMessageHandler.Handle(message));
                return;
            }
            else
            {
                messageDTO = new MessageDTO { ChatId = message.Chat.Id, Text = Constants.Error };
            }
            await _messageSender.SendMessage(messageDTO);
            await SaveRepositoryAsync();
        }
        private async Task ProcessMessagesAsync(List<MessageDTO> messages)
        {
            foreach (var message in messages)
            {
                var sentMessage = await _messageSender.SendMessage(message);

                message.SaveMessageIdCallback?.Invoke(sentMessage.MessageId);
            }
        }
        public async Task ProcessEditMessage(Message message)
        {
            if (IsMessageFromTargetChat(message))
            {
                await _targetChatMessageHandler.HandleEdit(message);
            }
        }

        private bool IsPrivateMessage(Message message)
        {
            return message.Chat.Type == Telegram.Bot.Types.Enums.ChatType.Private;
        }

        private bool IsMessageFromTargetChat(Message message)
        {
            var user = _userRepository.GetUserByTargetChat(message.Chat.Id);
            return user.TargetChatId == message.Chat.Id;
        }

        private async Task SaveRepositoryAsync()
        {
            await _repositoryLoader.SaveDataAsync(_userRepository as UserRepository);
        }
    }
}