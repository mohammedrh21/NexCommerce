using Microsoft.AspNetCore.Components;
using System.Threading.Tasks;

namespace Client.Components.UI;

public enum ModalSize
{
    Sm,
    Md,
    Lg,
    Xl,
    Fullscreen
}

public partial class AppModal : ComponentBase
{
    [Parameter] public bool IsOpen { get; set; }
    [Parameter] public EventCallback<bool> IsOpenChanged { get; set; }
    [Parameter] public string Title { get; set; } = string.Empty;
    [Parameter] public ModalSize Size { get; set; } = ModalSize.Md;
    [Parameter] public bool CloseOnBackdropClick { get; set; } = true;
    [Parameter] public RenderFragment? ChildContent { get; set; }
    [Parameter] public RenderFragment? Footer { get; set; }
    [Parameter] public EventCallback OnClose { get; set; }

    public async Task CloseAsync()
    {
        IsOpen = false;
        await IsOpenChanged.InvokeAsync(false);
        await OnClose.InvokeAsync();
    }

    protected async Task HandleBackdropClick()
    {
        if (CloseOnBackdropClick)
        {
            await CloseAsync();
        }
    }

    protected string GetModalPanelClasses()
    {
        var sizeClasses = Size switch
        {
            ModalSize.Sm => "sm:max-w-sm w-full",
            ModalSize.Md => "sm:max-w-md w-full",
            ModalSize.Lg => "sm:max-w-lg w-full",
            ModalSize.Xl => "sm:max-w-xl w-full",
            ModalSize.Fullscreen => "w-screen min-h-screen sm:my-0 rounded-none",
            _ => "sm:max-w-md w-full"
        };

        return sizeClasses;
    }
}
