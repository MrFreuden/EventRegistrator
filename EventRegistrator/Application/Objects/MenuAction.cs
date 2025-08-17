using EventRegistrator.Application.Interfaces;
using EventRegistrator.Application.Objects.Enums;
using EventRegistrator.Domain;

namespace EventRegistrator.Application.Objects
{
    public abstract record MenuAction;
    public record MenuContext(
        long ChatId,
        long? TargetChatId = null,
        string? HashtagName = null);

    public record NavigateMenu(MenuKey NextKey, MenuContext Ctx, int StartPage = 0) : MenuAction;
    public record SwitchState(Func<IState> Factory) : MenuAction;
    public record RunCommand(Func<Task> Do) : MenuAction;
    public record Noop(string? Reason = null) : MenuAction;
    public record MenuExtra(string Label, string Callback, Func<MenuContext, MenuAction> Action);

    public record MenuDescriptor(
        Func<MenuContext, string> Title,
        Func<IReadOnlyCollection<IPagiable>>? GetItems,
        int PageSize,
        IReadOnlyList<MenuExtra> Extras,
        Func<IPagiable, MenuAction>? OnItem);
}
