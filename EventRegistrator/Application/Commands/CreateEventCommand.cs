using EventRegistrator.Application.DTOs;
using EventRegistrator.Application.Interfaces;
using EventRegistrator.Application.Services;
using EventRegistrator.Domain.DTO;
using EventRegistrator.Domain.Models;
using EventRegistrator.Infrastructure.Utils;

namespace EventRegistrator.Application.Commands
{
    public class CreateEventCommand : ICommand
    {
        private readonly EventService _eventService;

        public CreateEventCommand(EventService eventService)
        {
            _eventService = eventService;
        }

        public async Task<List<Response>> Execute(MessageDTO message, UserAdmin user)
        {
            var @event = EventService.Create(message);
            @event.UpdateTemplate(user.GetTargetChat(user.CurrentContext.TargetChatId.Value).GetHashtagByName(@event.HashtagName).TemplateText);
            var result = _eventService.AddNewEvent(@event, message.Created);
            if (result.Success)
            {
                return [new Response
                    {
                        ChatId = result.Event.TargetChatId,
                        Text = result.Event.TemplateText,
                        ButtonData = new(Constants.Cancel, Constants.Cancel),
                        SaveMessageIdCallback = id => { result.Event.CommentMessageId = id; },
                        MessageToReplyId = message.Id
                    }];
            }
            return [new Response()];
        }
    }
}
