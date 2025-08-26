using EventRegistrator.Application.Commands;
using EventRegistrator.Application.Enums;
using EventRegistrator.Application.Interfaces;
using EventRegistrator.Application.Services;
using EventRegistrator.Application.States;
using EventRegistrator.Domain.Interfaces;
using CommandType = EventRegistrator.Application.Enums.CommandType;

namespace EventRegistrator.Application.Factories
{
    public class CommandFactory : ICommandFactory
    {
        private readonly IUserRepository _userRepository;
        private readonly IMenuStateFactory _menuStateFactory;
        private readonly EventService _eventService;
        private readonly RegistrationService _registrationService;
        private readonly ResponseManager _responseManager;

        public CommandFactory(
            IUserRepository userRepository, 
            IMenuStateFactory menuStateFactory, 
            EventService eventService, 
            RegistrationService registrationService, 
            ResponseManager responseManager)
        {
            _userRepository = userRepository;
            _menuStateFactory = menuStateFactory;
            _eventService = eventService;
            _registrationService = registrationService;
            _responseManager = responseManager;
        }

        public ICommand CreateCommand(CommandType commandType)
        {
            return commandType switch
            {
                CommandType.CreateEvent => new CreateEventCommand(_eventService),
                CommandType.Register => new RegisterCommand(_registrationService, _responseManager),
                CommandType.CancelRegistration => new DeleteRegistrationsCommand(_registrationService, _responseManager),
                CommandType.CancelRegistrations => new CancelAllRegistrationsCommand(_registrationService, _responseManager),
                _ => throw new ArgumentException($"Неизвестный тип команды: {commandType}")
            };
        }

        public ICommand CreateSlashCommand(string? text)
        {
            var commands = new Dictionary<string, Func<ICommand>>
            {
                { "/start", () => new StartCommand(_userRepository) },
                { "/settings", () => new SettingsCommand(_menuStateFactory) },
                { "/admin", () => new AdminCommand(_userRepository) }
            };

            if (commands.TryGetValue(text, out var command))
            {
                return command.Invoke();
            }

            throw new NotImplementedException("UnknownSlashCommand");
        }
    }

    public class StateFactory : IStateFactory
    {
        private readonly IStateManager _stateManager;
        private readonly Lazy<ICommandFactory> _commandFactory;

        public StateFactory(IStateManager stateManager, Lazy<ICommandFactory> commandFactory)
        {
            _stateManager = stateManager;
            _commandFactory = commandFactory;
        }

        public IState CreateState(StateType stateType)
        {
            return stateType switch
            {
                StateType.EditTemplateText => CreateEditTemplateTextState(),
                StateType.AddHashtag => CreateAddHashtagState(),
                StateType.AddChat => CreateAddChatState(),
                _ => throw new ArgumentException($"Неизвестный тип состояния: {stateType}")
            };
        }

        public IState CreateEditTemplateTextState()
        {
            return new EditTemplateTextState(_stateManager, this, _commandFactory.Value);
        }

        public IState CreateAddHashtagState()
        {
            return new AddHashtagState(_stateManager, this);
        }

        public IState CreateAddChatState()
        {
            return new AddChatState();
        }
    }
}
