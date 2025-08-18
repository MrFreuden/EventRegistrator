using EventRegistrator.Application.Interfaces;
using EventRegistrator.Application.Objects.DTOs;
using EventRegistrator.Application.Services;
using EventRegistrator.Domain.Models;

namespace EventRegistrator.Application.Commands
{
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
            var lastEvent = user.GetLastEvent();

            var resultUndo = _registrationService.CancelRegistration(lastEvent, message.Id);
            if (resultUndo.Success)
            {
                message.IsEdit = false;
                var text = TimeSlotParser.UpdateTemplateText(lastEvent.TemplateText, lastEvent.GetSlots());
                lastEvent.TemplateText = text;
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
