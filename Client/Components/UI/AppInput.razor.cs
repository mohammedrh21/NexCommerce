using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Forms;
using System;
using System.Collections.Generic;
using System.Linq.Expressions;

namespace Client.Components.UI;

public partial class AppInput : InputBase<string>
{
    [Parameter] public string? Id { get; set; } = $"input-{Guid.NewGuid():N}";
    [Parameter] public string? Label { get; set; }
    [Parameter] public string Type { get; set; } = "text";
    [Parameter] public string? Placeholder { get; set; }
    [Parameter] public bool Disabled { get; set; }
    [Parameter] public RenderFragment? Icon { get; set; }
    [Parameter] public RenderFragment? RightIcon { get; set; }
    [Parameter] public string Class { get; set; } = string.Empty;
    [Parameter] public Expression<Func<string>>? ValidationExpression { get; set; }

    protected override void OnInitialized()
    {
        base.OnInitialized();
        ValidationExpression ??= ValueExpression;
    }

    protected void HandleInput(ChangeEventArgs e)
    {
        CurrentValueAsString = e.Value?.ToString();
    }

    protected override bool TryParseValueFromString(string? value, out string result, out string validationErrorMessage)
    {
        result = value ?? string.Empty;
        validationErrorMessage = string.Empty;
        return true;
    }

    protected string GetInputClasses()
    {
        var baseClasses = "block w-full rounded-md border-0 py-2 text-zinc-900 dark:text-zinc-100 bg-white dark:bg-zinc-800 shadow-sm ring-1 ring-inset ring-zinc-300 dark:ring-zinc-700 placeholder:text-zinc-400 focus:ring-2 focus:ring-inset focus:ring-indigo-600 sm:text-sm sm:leading-6 disabled:opacity-50 disabled:cursor-not-allowed transition-all duration-200";

        var paddingClasses = "";
        if (Icon != null)
            paddingClasses += " pl-10";
        if (RightIcon != null)
            paddingClasses += " pr-10";

        var validationClass = "";
        if (EditContext != null && FieldIdentifier.Model != null)
        {
            var isValid = !EditContext.GetValidationMessages(FieldIdentifier).Any();
            var isModified = EditContext.IsModified(FieldIdentifier);
            if (isModified && !isValid)
                validationClass = " ring-red-300 dark:ring-red-800 focus:ring-red-500";
        }

        return $"{baseClasses}{paddingClasses}{validationClass}".Trim();
    }
}
