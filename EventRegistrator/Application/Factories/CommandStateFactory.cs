using EventRegistrator.Application.Commands;
using EventRegistrator.Application.Enums;
using EventRegistrator.Application.Interfaces;
using EventRegistrator.Application.Services;
using EventRegistrator.Application.States;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Win32;

namespace EventRegistrator.Application.Factories
{
    public class CommandStateFactory : IStateFactory
    {
        public IState CreateState(StateType stateType)
        {
            return stateType switch
            {
                StateType.EditTemplateText => new EditTemplateTextState(),
                _ => throw new ArgumentException($"Неизвестный тип состояния: {stateType}")
            };
        }
    }

    public class CommandFactory : ICommandFactory
    {
        private readonly CommandRegistry _registry;
        private readonly IServiceProvider _serviceProvider;

        public CommandFactory(CommandRegistry registry, IServiceProvider serviceProvider)
        {
            _registry = registry;
            _serviceProvider = serviceProvider;
        }

        public ICommand CreateCommand(string name)
        {
            var type = _registry.GetSlashCommand(name) ??
                   _registry.GetCallbackCommand(name);

            return (ICommand)ActivatorUtilities.CreateInstance(_serviceProvider, type);
        }
    }
}
