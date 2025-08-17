using EventRegistrator.Application.Interfaces;
using EventRegistrator.Application.Objects.DTOs;
using EventRegistrator.Domain.Models;

namespace EventRegistrator.Application.States
{
    internal class AddHashtagState : IState
    {
        private long _value;

        public AddHashtagState(long value)
        {
            _value = value;
        }

        public async Task<List<Response>> Execute(MessageDTO message, UserAdmin user)
        {
            throw new NotImplementedException();
        }

        public async Task<Response> Handle(MessageDTO message, UserAdmin user)
        {
            throw new NotImplementedException();
        }
    }
}