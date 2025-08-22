﻿using EventRegistrator.Application.DTOs;
using EventRegistrator.Application.Interfaces;
using EventRegistrator.Domain.DTO;
using EventRegistrator.Domain.Models;
using EventRegistrator.Infrastructure.Utils;

namespace EventRegistrator.Application.States
{
    internal class AddChatState : IState
    {
        public async Task<List<Response>> Execute(MessageDTO message, UserAdmin user)
        {
            return [new Response { ChatId = user.Id, Text = Constants.Error , MessageToEditId = user.LastMessageId }];
        }

        public async Task<Response> Handle(MessageDTO message, UserAdmin user)
        {
            return new Response { ChatId = user.Id, Text = Constants.Error, MessageToEditId = user.LastMessageId };
        }
    }
}