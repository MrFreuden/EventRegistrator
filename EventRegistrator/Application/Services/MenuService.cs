using EventRegistrator.Application.Enums;
using EventRegistrator.Application.Interfaces;
using EventRegistrator.Application.States;
using EventRegistrator.Domain.Interfaces;
using EventRegistrator.Domain.Models;
using EventRegistrator.Infrastructure.Utils;

namespace EventRegistrator.Application.Services
{
    public class MenuService : IMenuService
    {
        private readonly IUserRepository _userRepository;
        private readonly IStateFactory _stateFactory;
        private const int _maxObjPerPage = 3;
        public MenuService(IUserRepository userRepository, IStateFactory stateFactory)
        {
            _userRepository = userRepository;
            _stateFactory = stateFactory;
        }

        public MenuDescriptor Get(MenuKey key, MenuContext ctx) => key switch
        {
            MenuKey.TargetChats => new MenuDescriptor(
                Title: (ctx) => "Виберiть канал",
                GetItems: () => _userRepository.GetUser(ctx.ChatId).GetAllTargetChats(),
                PageSize: _maxObjPerPage,
                Extras: new[]
                {
                new MenuExtra("➕ Додати чат", "chat_add",
                    _ => new SwitchState(() => _stateFactory.CreateState(StateType.AddChat))),
                },
                OnItem: (ip) =>
                {
                    var chat = (TargetChat)ip;
                    return new NavigateMenu(
                        NextKey: MenuKey.Hashtags,
                        Ctx: ctx with { TargetChatId = chat.Id },
                        StartPage: 0
                    );
                }
            ),

            MenuKey.Hashtags => new MenuDescriptor(
                Title: (ctx) => $"Хэштеги каналу {_userRepository.GetUserByTargetChat(ctx.TargetChatId.Value).GetTargetChat(ctx.TargetChatId.Value).Name}",
                GetItems: () => _userRepository.GetUserByTargetChat(ctx.TargetChatId.Value).GetAllHashtags(ctx.TargetChatId.Value),
                PageSize: _maxObjPerPage,
                Extras: new[]
                {
                new MenuExtra("➕ Додати хэштег", "tag_add",
                    c => new SwitchState(() => _stateFactory.CreateState(StateType.AddHashtag))),
                new MenuExtra("🔙 Назад", "back",
                    _ => new NavigateMenu(MenuKey.TargetChats, ctx with { TargetChatId = null }))
                },
                OnItem: (ip) =>
                {
                    var tag = (Hashtag)ip;
                    return new NavigateMenu(
                        NextKey: MenuKey.HashtagDetails,
                        Ctx: ctx with { HashtagName = tag.Name }
                    );
                }
            ),

            MenuKey.HashtagDetails => new MenuDescriptor(
                Title: ctx =>
        $"Шаблон для хэштегу #{ctx.HashtagName}\n{_userRepository.GetUserByTargetChat(ctx.TargetChatId.Value).GetTargetChat(ctx.TargetChatId.Value).GetHashtagByName(ctx.HashtagName).TemplateText}",
                GetItems: null,
                PageSize: _maxObjPerPage,
                Extras: new[]
                {
                new MenuExtra("Редагувати", Constants.EditTemplateText,
                    c => new SwitchState(() => _stateFactory.CreateState(StateType.EditTemplateText))),
                new MenuExtra("🔙 Назад", "back",
                    _ => new NavigateMenu(MenuKey.Hashtags, ctx with { HashtagName = null }))
                },
                OnItem: null
            ),

            _ => throw new ArgumentOutOfRangeException(nameof(key), key, null)
        };
    }

}
