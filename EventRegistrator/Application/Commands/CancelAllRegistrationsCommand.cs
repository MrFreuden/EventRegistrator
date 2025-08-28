using EventRegistrator.Application.Commands.Attributes;
using EventRegistrator.Application.DTOs;
using EventRegistrator.Application.Interfaces;
using EventRegistrator.Application.Services;
using EventRegistrator.Domain.DTO;
using EventRegistrator.Domain.Models;
using EventRegistrator.Infrastructure.Utils;
using Microsoft.Extensions.Logging;

namespace EventRegistrator.Application.Commands
{
    [CallbackCommand("Cancel", "Отменить все регистрации")]
    public class CancelAllRegistrationsCommand : ICommand
    {
        private readonly ResponseManager _responseManager;
        private readonly RegistrationService _registrationService;

        public CancelAllRegistrationsCommand(RegistrationService registrationService, ResponseManager responseManager)
        {
            _responseManager = responseManager;
            _registrationService = registrationService;
        }

        public async Task<List<Response>> Execute(MessageDTO message, UserAdmin user)
        {
            var @event = user.GetEvent(message.ReplyToMessageId ?? 0);
            if (@event == null)
            {
                Console.WriteLine("Не удалось найти ивент");
                return [];
            }
            var resultUndo = _registrationService.CancelAllRegistrations(@event, message.UserId.Value);
            if (resultUndo.Success)
            {
                var text = TimeSlotParser.UpdateTemplateText(@event.TemplateText, @event.Slots);
                @event.UpdateTemplate(text);
            }

            return GetSuccessResponsesForEdit(user, resultUndo);
        }

        private List<Response> GetSuccessResponsesForEdit(UserAdmin user, RegistrationResult result)
        {
            var messages = _responseManager.PrepareNotificationMessages(user, result.Event);
            foreach (var id in result.MessageIds)
            {
                messages.Add(_responseManager.CreateUnlikeMessage(result.Event.TargetChatId, id));
            }
            
            return messages;
        }
    }
}
