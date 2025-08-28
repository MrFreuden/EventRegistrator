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
    [Command("DeleteRegistrations", "Удаление регистраций")]
    public class DeleteRegistrationsCommand : ICommand
    {
        private readonly ResponseManager _responseManager;
        private readonly RegistrationService _registrationService;

        public DeleteRegistrationsCommand(RegistrationService registrationService, ResponseManager responseManager)
        {
            _registrationService = registrationService;
            _responseManager = responseManager;
        }

        public async Task<List<Response>> Execute(MessageDTO message, UserAdmin user)
        {
            var @event = user.GetEvent(message.ReplyToMessageId ?? 0);
            if (@event == null)
            {
                Console.WriteLine("Не удалось найти ивент");
                return [];
            }

            var resultUndo = _registrationService.CancelRegistration(@event, message.Id);
            if (resultUndo.Success)
            {
                message.IsEdit = false;
                var text = TimeSlotParser.UpdateTemplateText(@event.TemplateText, @event.Slots);
                @event.UpdateTemplate(text);
            }
          
            return GetSuccessResponsesForEdit(user, resultUndo);
        }

        private List<Response> GetSuccessResponsesForEdit(UserAdmin user, RegistrationResult result)
        {
            var messages = _responseManager.PrepareNotificationMessages(user, result.Event);
            messages.Add(_responseManager.CreateUnlikeMessage(result.Event.TargetChatId, result.MessageIds.FirstOrDefault()));
            return messages;
        }
    }
}
