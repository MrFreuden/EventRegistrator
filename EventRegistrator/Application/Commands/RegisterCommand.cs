using EventRegistrator.Application.Interfaces;
using EventRegistrator.Application.Objects.DTOs;
using EventRegistrator.Application.Services;
using EventRegistrator.Domain.Models;

namespace EventRegistrator.Application.Commands
{
    public class RegisterCommand : ICommand
    {
        private readonly ResponseManager _responseManager;
        private readonly RegistrationService _registrationService;

        public RegisterCommand(RegistrationService registrationService, ResponseManager responseManager)
        {
            _registrationService = registrationService;
            _responseManager = responseManager;
        }

        public async Task<List<Response>> Execute(MessageDTO message, UserAdmin user)
        {
            var lastEvent = user.GetLastEvent();
            if (lastEvent.PostId != message.ReplyToMessageId)
            {
                Console.WriteLine("Попытка зарегистрировать на другой пост");
                return [];
            }
            user.CurrentContext = new Objects.MenuContext(user.PrivateChatId, message.ChatId);
            var map = TimeSlotParser.GetMaper(lastEvent.TemplateText);
            var regs = TimeSlotParser.ParseRegistrationMessage(message, map);

            var result = _registrationService.ProcessRegistration(lastEvent, regs);
            if (result.Success)
            {
                var text = TimeSlotParser.UpdateTemplateText(lastEvent.TemplateText, lastEvent.Slots);
                lastEvent.TemplateText = text;
                result.MessageIds = [message.Id];
                return GetSuccessResponses(user, result);
            }
            return [];
        }


        private List<Response> GetSuccessResponses(UserAdmin user, RegistrationResult result)
        {
            var messages = _responseManager.PrepareNotificationMessages(user, result.Event);
            messages.Add(_responseManager.CreateLikeMessage(result.Event.TargetChatId, result.MessageIds.FirstOrDefault()));
            return messages;
        }
    }
}
