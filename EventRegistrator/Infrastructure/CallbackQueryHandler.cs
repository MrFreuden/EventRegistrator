using EventRegistrator.Application;
using EventRegistrator.Application.Objects.DTOs;
using EventRegistrator.Domain;
using Telegram.Bot.Types;

namespace EventRegistrator.Infrastructure
{
    public class CallbackQueryHandler
    {
        private readonly IUserRepository _userRepository;
        private readonly MessageSender _messageSender;
        private readonly RepositoryLoader _repositoryLoader;
        private readonly UpdateRouter _updateRouter;

        public CallbackQueryHandler(
            IUserRepository userRepository, 
            MessageSender messageSender, 
            RepositoryLoader repositoryLoader, 
            UpdateRouter updateRouter)
        {
            _userRepository = userRepository;
            _messageSender = messageSender;
            _repositoryLoader = repositoryLoader;
            _updateRouter = updateRouter;
        }

        public async Task ProcessCallbackQuery(CallbackQuery callbackQuery)
        {
            await _messageSender.AnswerAsync(callbackQuery.Id);
            var messageDto = UpdateMapper.Map(callbackQuery);
            var responses = await _updateRouter.RouteCallback(messageDto);
            await ProcessMessagesAsync(responses);
            await SaveRepositoryAsync();
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