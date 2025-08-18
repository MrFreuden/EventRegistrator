using EventRegistrator.Application.Interfaces;
using EventRegistrator.Application.Objects.DTOs;
using EventRegistrator.Domain.Models;

namespace EventRegistrator.Application.States
{
    public class DefaultState : IState
    {
        private Dictionary<string, Func<ICommand>> _commands;

        public DefaultState(Dictionary<string, Func<ICommand>> commands)
        {
            _commands = commands;
        }

        public async Task<List<Response>> Execute(MessageDTO message, UserAdmin user)
        {
            throw new NotImplementedException();
        }

        public async Task<Response> Handle(MessageDTO message, UserAdmin user)
        {
            user.StateHistory.Clear();
            var command = _commands[message.Text].Invoke();
            var response = command.Execute(message, user).Result.First();
            response.SaveMessageIdCallback = id => user.LastMessageId = id;
            return response;
        }
    }
}
