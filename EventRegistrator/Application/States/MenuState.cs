using EventRegistrator.Application.DTOs;
using EventRegistrator.Application.Enums;
using EventRegistrator.Application.Interfaces;
using EventRegistrator.Application.Objects;
using EventRegistrator.Application.Services;
using EventRegistrator.Domain.DTO;
using EventRegistrator.Domain.Interfaces;
using EventRegistrator.Domain.Models;
        

namespace EventRegistrator.Application.States
{
    public class MenuState : BaseState
    {
        private readonly IMenuService _menuService;
        private readonly MenuKey _key;
        private readonly MenuContext _ctx;
        private int _page;
        private const string PagePrefix = "page_";
        public MenuState(
            IMenuService menuService,
            MenuKey key,
            MenuContext ctx,
            IStateManager stateManager,
            IStateFactory stateFactory,
            int startPage = 0)
            : base(stateManager, stateFactory)
        {
            _menuService = menuService;
            _key = key;
            _ctx = ctx;
            _page = startPage;
        }

        public override async Task<Response> Handle(MessageDTO message, UserAdmin user)
        {
            var d = _menuService.Get(_key, _ctx);
            var items = d.GetItems?.Invoke() ?? Array.Empty<IPagiable>();

            var maxPage = Math.Max(0, (int)Math.Ceiling(items.Count / (double)Math.Max(1, d.PageSize)) - 1);
            if (_page > maxPage) _page = maxPage;

            var buttons = BuildButtons(d, items, maxPage);
            var pageCounterText = maxPage < 2 ? "" : $"\nСтр. {_page + 1}/{Math.Max(1, maxPage + 1)}";

            user.CurrentContext = _ctx;
            if (user.LastMessageId == null)
            {
                return new Response
                {
                    ChatId = message.ChatId,
                    Text = d.GetItems is null ? d.Title.Invoke(_ctx) : $"{d.Title.Invoke(_ctx)}" + pageCounterText,
                    ButtonData = new ButtonData(buttons),
                    SaveMessageIdCallback = id => user.LastMessageId = id,
                };
            }

            return new Response
            {
                ChatId = message.ChatId,
                Text = d.GetItems is null ? d.Title.Invoke(_ctx) : $"{d.Title.Invoke(_ctx)}" + pageCounterText,
                ButtonData = new ButtonData(buttons),
                MessageToEditId = user.LastMessageId,
            };
        }

        protected override async Task<StateResult> ProcessInput(MessageDTO message, UserAdmin user)
        {
            var d = _menuService.Get(_key, _ctx);

            if (message.Text.StartsWith(PagePrefix))
            {
                if (int.TryParse(message.Text.AsSpan(PagePrefix.Length), out var p))
                    _page = Math.Max(0, p);

                return new StateResult
                {
                    Responses = [await Handle(message, user)],
                    Data = null,
                    ShouldTransition = false
                };
            }

            var extra = d.Extras.FirstOrDefault(x => x.Callback == message.Text);
            if (extra is not null)
            {
                return await ProcessMenuAction(extra.Action(_ctx), message, user);
            }

            if (d.GetItems is not null && d.OnItem is not null)
            {
                var items = d.GetItems();
                var selected = items.FirstOrDefault(i => i.Callback == message.Text);
                if (selected is not null)
                {
                    return await ProcessMenuAction(d.OnItem(selected), message, user);
                }
            }

            return new StateResult
            {
                Responses = [],
                Data = null,
                ShouldTransition = false
            };
        }

        protected override bool ShouldChangeState(StateResult result)
        {
            return result.ShouldTransition;
        }

        protected override StateType GetNextStateType(StateResult result)
        {
            return result.NextStateType ?? StateType.Default;
        }

        private async Task<StateResult> ProcessMenuAction(MenuAction action, MessageDTO message, UserAdmin user)
        {
            switch (action)
            {
                case NavigateMenu nm:
                    var newMenuState = new MenuState(_menuService, nm.NextKey, nm.Ctx, _stateManager, _stateFactory, nm.StartPage);
                    _stateManager.TransitionToState(user, newMenuState);
                    return new StateResult
                    {
                        Responses = [await newMenuState.Handle(message, user)],
                        Data = null,
                        ShouldTransition = true,
                        NextStateType = StateType.Menu,
                    };

                case SwitchState ss:
                    var newState = ss.Factory();
                    _stateManager.TransitionToState(user, newState);
                    return new StateResult
                    {
                        Responses = [await newState.Handle(message, user)],
                        Data = null,
                        ShouldTransition = true,
                        NextStateType = StateType.Default,
                    };

                case RunCommand rc:
                    await rc.Do();
                    return new StateResult
                    {
                        Responses = [await Handle(message, user)],
                        Data = null,
                        ShouldTransition = false
                    };

                case Noop:
                default:
                    return new StateResult
                    {
                        Responses = [await Handle(message, user)],
                        Data = null,
                        ShouldTransition = false
                    };
            }
        }

        private List<List<Button>> BuildButtons(MenuDescriptor d, IReadOnlyCollection<IPagiable> items, int maxPage)
        {
            var buttons = new List<List<Button>>();

            if (d.GetItems is not null)
            {
                var pageItems = items.Skip(_page * d.PageSize).Take(d.PageSize);
                var pageRow = new List<Button>();
                foreach (var it in pageItems)
                    pageRow.Add(new Button(it.Name, it.Callback));
                buttons.Add(pageRow);

                AddNavigationButtons(buttons, maxPage);
            }

            foreach (var ex in d.Extras)
                buttons.Add(new() { new(ex.Label, ex.Callback) });

            return buttons;
        }

        private void AddNavigationButtons(List<List<Button>> buttons, int maxPage)
        {
            var nav = new List<Button>();
            if (_page > 0) nav.Add(new Button("⬅️", $"{PagePrefix}{_page - 1}"));
            if (_page < maxPage) nav.Add(new Button("➡️", $"{PagePrefix}{_page + 1}"));
            if (nav.Count > 0) buttons.Add(nav);
        }
    }

}
