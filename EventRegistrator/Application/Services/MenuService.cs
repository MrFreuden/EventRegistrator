using EventRegistrator.Application.Interfaces;
using EventRegistrator.Application.Objects;
using EventRegistrator.Application.Objects.Enums;
using EventRegistrator.Application.States;
using EventRegistrator.Domain;
using EventRegistrator.Domain.Models;

namespace EventRegistrator.Application.Services
{
    public class MenuService : IMenuService
    {
        private readonly IUserRepository _userRepository;
        private readonly UserAdmin _userAdmin;
        private readonly IStateFactory _stateFactory;
        private const int _maxObjPerPage = 3;
        public MenuService(IUserRepository userRepository)
        {
            _userRepository = userRepository;
        }

        public MenuDescriptor Get(MenuKey key, MenuContext ctx) => key switch
        {
            MenuKey.TargetChats => new MenuDescriptor(
                Title: (ctx) => "Выберите чат",
                GetItems: () => _userRepository.GetUser(ctx.ChatId).GetAllTargetChats(),
                PageSize: _maxObjPerPage,
                Extras: new[]
                {
                new MenuExtra("➕ Добавить чат", "chat_add",
                    _ => new SwitchState(() => new AddChatState())),
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
                Title: (ctx) => $"Хэштеги чата {_userRepository.GetUserByTargetChat(ctx.TargetChatId.Value).GetTargetChat(ctx.TargetChatId.Value).Name}",
                GetItems: () => _userRepository.GetUserByTargetChat(ctx.TargetChatId.Value).GetAllHashtags(ctx.TargetChatId.Value),
                PageSize: _maxObjPerPage,
                Extras: new[]
                {
                new MenuExtra("➕ Добавить хэштег", "tag_add",
                    c => new SwitchState(() => new AddHashtagState(c.TargetChatId!.Value))),
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
        $"Шаблон для хэштегу {ctx.HashtagName}\n{_userRepository.GetUserByTargetChat(ctx.TargetChatId.Value).GetTargetChat(ctx.TargetChatId.Value).GetHashtagByName(ctx.HashtagName).TemplateText}",
                GetItems: null,
                PageSize: _maxObjPerPage,
                Extras: new[]
                {
                new MenuExtra("Редагувати", Constants.EditTemplateText,
                    c => new SwitchState(() => new EditTemplateTextState())),
                new MenuExtra("🔙 Назад", "back",
                    _ => new NavigateMenu(MenuKey.Hashtags, ctx with { HashtagName = null }))
                },
                OnItem: null
            ),

            _ => throw new ArgumentOutOfRangeException(nameof(key), key, null)
        };
    }

}
