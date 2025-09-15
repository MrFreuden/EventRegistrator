﻿using EventRegistrator.Application.Commands;
using EventRegistrator.Application.Enums;
using EventRegistrator.Application.Interfaces;
using EventRegistrator.Application.Services;
using EventRegistrator.Application.States;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Win32;
using Telegram.Bot.Types;

namespace EventRegistrator.Application.Factories
{
    public class StateFactory : IStateFactory
    {
        private readonly ResponseManager _responseManager;

        public StateFactory(ResponseManager responseManager)
        {
            _responseManager = responseManager;
        }

        public IState CreateState(StateType stateType)
        {
            return stateType switch
            {
                StateType.EditTemplateText => new EditTemplateTextState(_responseManager),
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
            if (string.IsNullOrEmpty(name))
                throw new ArgumentNullException("name");
            var type = _registry.GetSlashCommand(name) ??
                   _registry.GetCallbackCommand(name);

            return (ICommand)ActivatorUtilities.CreateInstance(_serviceProvider, type);
        }
    }
}
