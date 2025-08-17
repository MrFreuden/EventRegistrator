using EventRegistrator.Application.DTOs;
using EventRegistrator.Application.Interfaces;
using EventRegistrator.Domain.Models;

namespace EventRegistrator.Application.States
{
    //public class PaginationState<T> : IState where T : IPagiable
    //{
    //    private readonly Func<IReadOnlyCollection<T>> _getItems;
    //    private readonly int _pageSize;
    //    private readonly Func<T, Task<IState>> _onItemSelected;
    //    private int _page;
    //    private List<(string, string)> _buttonsData;

    //    public PaginationState(
    //        Func<IReadOnlyCollection<T>> getItems,
    //        Func<T, Task<IState>> onItemSelected,
    //        int pageSize = 5,
    //        int startPage = 0)
    //    {
    //        _getItems = getItems ?? throw new ArgumentNullException(nameof(getItems));
    //        _onItemSelected = onItemSelected ?? throw new ArgumentNullException(nameof(onItemSelected));
    //        _pageSize = pageSize;
    //        _page = startPage;
    //    }

    //    public async Task<List<Response>> Execute(MessageDTO message, UserAdmin user)
    //    {
    //        var items = _getItems();
    //        var item = items.FirstOrDefault(x => x.Callback == message.Text);
    //        if (item != null)
    //        {
    //            user.State = new MenuState<T>(item, new MenuService());
    //            return new List<Response> { await user.State.Handle(message, user) };
    //        }



    //        return [];
    //    }

    //    public async Task<Response> Handle(MessageDTO message, UserAdmin user)
    //    {
    //        var items = _getItems();
    //        var buttons = BuildPageButtons(items);
    //        _buttonsData = buttons;

    //        return await Task.FromResult(new Response
    //        {
    //            ChatId = message.ChatId,
    //            Text = "Выберите элемент:",
    //            ButtonData = buttons
    //        });
    //    }

    //    private List<(string, string)> BuildPageButtons(IReadOnlyCollection<T> items)
    //    {
    //        // стандартная логика: делим по страницам
    //        return items
    //            .Skip(_page * 10)
    //            .Take(10)
    //            .Select(i => (i.Name, i.Callback))
    //            .ToList();
    //    }
    //    private Response BuildPage()
    //    {
    //        var items = _getItems().ToList();
    //        var pageItems = items.Skip(_page * _pageSize).Take(_pageSize).ToList();
    //        var buttons = new List<(string, string)>();

    //        foreach (var item in pageItems)
    //        {
    //            buttons.Add((item.Name, item.Callback));
    //        }

    //        var navRow = new List<(string, string)>();
    //        if (_page > 0)
    //            navRow.Add(("⬅️", $"page_{_page - 1}"));
    //        if ((_page + 1) * _pageSize < items.Count)
    //            navRow.Add(("➡️", $"page_{_page + 1}"));
    //        navRow.Add(("🔙 Назад", "back"));

    //        if (navRow.Any())
    //            buttons.AddRange(navRow);

    //        return new Response
    //        {
    //            Text = "Выберите:",
    //            ButtonData = buttons,
    //        };
    //    }
    //}

    
}
