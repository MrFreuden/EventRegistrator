using EventRegistrator.Application.DTOs;
using EventRegistrator.Application.Services;
using EventRegistrator.Domain;
using EventRegistrator.Domain.Models;
using EventRegistrator.Infrastructure;

namespace EventRegistrator.Application
{
    public class MessageRouter
    {
        private readonly List<IHandler> _handlers;
        public MessageRouter(IEnumerable<IHandler> handlers)
        {
            _handlers = handlers.ToList();
        }

        public async Task<List<Response>> Route(MessageDTO message)
        {
            var handler = _handlers.FirstOrDefault(h => h.CanHandle(message));
            if (handler != null)
            {
                return await handler.HandleAsync(message);
            }
            return new List<Response> { new Response { ChatId = message.ChatId, Text = Constants.Error } };
        }
    }

    public interface ICommand
    {
        Task<List<Response>> Execute(MessageDTO message, UserAdmin user = null);
    }

    public class StartCommand : ICommand
    {
        private readonly IUserRepository _userRepository;

        public StartCommand(IUserRepository userRepository)
        {
            _userRepository = userRepository;
        }

        public async Task<List<Response>> Execute(MessageDTO message, UserAdmin user = null)
        {
            _userRepository.AddUser(message.ChatId);
            return [new Response { ChatId = message.ChatId, Text = Constants.Greetings }];
        }
    }

    public class SettingsCommand : ICommand
    {
        public async Task<List<Response>> Execute(MessageDTO message, UserAdmin user)
        {
            var text = user.GetTargetChat().GetHashtagByName("sws").TemplateText;
            return [new Response { ChatId = message.ChatId, Text = text }];
        }
    }
    public class AdminCommand : ICommand
    {
        private readonly IUserRepository _userRepository;

        public AdminCommand(IUserRepository userRepository)
        {
            _userRepository = userRepository;
        }
        public async Task<List<Response>> Execute(MessageDTO message, UserAdmin user = null)
        {
            var text2 = TextFormatter.GetAllUsersInfo(_userRepository as UserRepository);
            return [new Response { ChatId = message.ChatId, Text = text2 }];
        }
    }
    public class EditTemplateTextCommand : ICommand
    {
        public async Task<List<Response>> Execute(MessageDTO message, UserAdmin user)
        {
            user.IsAsked = false;
            var hashtag = user.GetTargetChat().GetHashtagByName("sws");
            hashtag.EditTemplateText(message.Text);
            return [new Response { ChatId = message.ChatId, Text = hashtag.TemplateText }];
        }
    }
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
            @event.TemplateText = user.GetTargetChat().GetHashtagByName(@event.HashtagName).TemplateText;
            var result = _eventService.AddNewEvent(@event, message.Created);
            if (result.Success)
            {
                return [new Response
                    {
                        ChatId = result.Event.TargetChatId,
                        Text = result.Event.TemplateText,
                        ButtonData = (Constants.Cancel, Constants.Cancel),
                        SaveMessageIdCallback = id => { result.Event.CommentMessageId = id; },
                        MessageToReplyId = message.Id
                    }];
            }
            return [new Response()];
        }
    }
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
            var map = TimeSlotParser.GetMaper(lastEvent.TemplateText);
            var regs = TimeSlotParser.ParseRegistrationMessage(message, map);

            var result = _registrationService.ProcessRegistration(lastEvent, regs);
            if (result.Success)
            {
                result.MessageId = message.Id;
                return GetSuccessResponses(user, result);
            }
            return [];
        }


        private List<Response> GetSuccessResponses(UserAdmin user, RegistrationResult result)
        {
            var messages = _responseManager.PrepareNotificationMessages(user, result.Event);
            messages.Add(_responseManager.CreateLikeMessage(result.Event.TargetChatId, result.MessageId));
            return messages;
        }
    }
    public interface IState
    {
        Task<Response> Handle(MessageDTO message);
    }

    public class DefaultState : IState
    {
        private Dictionary<string, Func<ICommand>> _commands;

        public DefaultState(Dictionary<string, Func<ICommand>> commands)
        {
            _commands = commands;
        }

        public async Task<Response> Handle(MessageDTO message)
        {
            throw new NotImplementedException();
        }
    }
    public class EditTemplateTextState : IState
    {
        private readonly IUserRepository _userRepository;

        public EditTemplateTextState(IUserRepository userRepository)
        {
            _userRepository = userRepository;
        }

        public async Task<Response> Handle(MessageDTO message)
        {
            throw new NotImplementedException();
        }
    }
}
