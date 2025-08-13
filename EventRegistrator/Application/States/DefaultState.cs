using EventRegistrator.Application.DTOs;
using EventRegistrator.Application.Interfaces;
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
            var s = _commands[message.Text].Invoke();
            return s.Execute(message).Result.First();
        }
    }
}
