using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Client.Components.UI;

public enum ButtonVariant
{
    Primary,
    Secondary,
    Danger,
    Ghost,
    Link
}

public enum ButtonSize
{
    Sm,
    Md,
    Lg
}

public partial class AppButton : ComponentBase
{
    [Parameter] public string Type { get; set; } = "button";
    [Parameter] public ButtonVariant Variant { get; set; } = ButtonVariant.Primary;
    [Parameter] public ButtonSize Size { get; set; } = ButtonSize.Md;
    [Parameter] public bool IsLoading { get; set; }
    [Parameter] public bool Disabled { get; set; }
    [Parameter] public EventCallback<MouseEventArgs> OnClick { get; set; }
    [Parameter] public RenderFragment? ChildContent { get; set; }
    [Parameter] public string Class { get; set; } = string.Empty;
    [Parameter(CaptureUnmatchedValues = true)] public IDictionary<string, object>? AdditionalAttributes { get; set; }

    protected async Task HandleClick(MouseEventArgs e)
    {
        if (!Disabled && !IsLoading && OnClick.HasDelegate)
        {
            await OnClick.InvokeAsync(e);
        }
    }

    protected string GetButtonClasses()
    {
        var baseClasses = "inline-flex items-center justify-center font-medium shadow-sm transition-all duration-200 focus:outline-none focus:ring-2 focus:ring-offset-2 disabled:opacity-50 disabled:cursor-not-allowed select-none";
        
        var variantClasses = Variant switch
        {
            ButtonVariant.Primary => "bg-indigo-600 hover:bg-indigo-500 text-white border border-transparent focus:ring-indigo-500 dark:focus:ring-offset-zinc-900",
            ButtonVariant.Secondary => "bg-white dark:bg-zinc-800 text-zinc-900 dark:text-zinc-100 border border-zinc-300 dark:border-zinc-700 hover:bg-zinc-50 dark:hover:bg-zinc-700 focus:ring-indigo-500 dark:focus:ring-offset-zinc-900",
            ButtonVariant.Danger => "bg-red-600 hover:bg-red-500 text-white border border-transparent focus:ring-red-500 dark:focus:ring-offset-zinc-900",
            ButtonVariant.Ghost => "bg-transparent text-zinc-700 dark:text-zinc-300 border border-transparent hover:bg-zinc-100 dark:hover:bg-zinc-850 shadow-none focus:ring-zinc-500",
            ButtonVariant.Link => "bg-transparent text-indigo-600 dark:text-indigo-400 border border-transparent hover:underline shadow-none focus:ring-indigo-500 p-0",
            _ => ""
        };

        var sizeClasses = Size switch
        {
            ButtonSize.Sm => "px-3 py-1.5 text-xs rounded-md",
            ButtonSize.Md => "px-4 py-2 text-sm rounded-md",
            ButtonSize.Lg => "px-5 py-2.5 text-base rounded-md",
            _ => ""
        };

        if (Variant == ButtonVariant.Link)
        {
            // Link has no padding or border shadow
            sizeClasses = "text-sm";
        }

        return $"{baseClasses} {variantClasses} {sizeClasses} {Class}".Trim();
    }
}
