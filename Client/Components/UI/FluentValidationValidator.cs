using FluentValidation;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Forms;
using System;
using System.Linq;

namespace Client.Components.UI;

public class FluentValidationValidator : ComponentBase, IDisposable
{
    private EditContext? _editContext;

    [CascadingParameter]
    private EditContext? CascadedEditContext { get; set; }

    [Parameter]
    public IValidator? Validator { get; set; }

    [Parameter]
    public Type? ValidatorType { get; set; }

    [Inject]
    private IServiceProvider ServiceProvider { get; set; } = default!;

    protected override void OnInitialized()
    {
        if (CascadedEditContext == null)
        {
            throw new InvalidOperationException($"{nameof(FluentValidationValidator)} requires a cascading parameter of type {nameof(EditContext)}.");
        }

        _editContext = CascadedEditContext;
        _editContext.OnValidationRequested += OnValidationRequested;
        _editContext.OnFieldChanged += OnFieldChanged;
    }

    private void OnValidationRequested(object? sender, ValidationRequestedEventArgs e)
    {
        Validate();
    }

    private void OnFieldChanged(object? sender, FieldChangedEventArgs e)
    {
        ValidateField(e.FieldIdentifier);
    }

    private void Validate()
    {
        if (_editContext == null) return;

        var validator = GetValidator();
        if (validator == null) return;

        var messages = new ValidationMessageStore(_editContext);
        messages.Clear();

        var context = new ValidationContext<object>(_editContext.Model);
        var result = validator.Validate(context);

        foreach (var error in result.Errors)
        {
            messages.Add(_editContext.Field(error.PropertyName), error.ErrorMessage);
        }

        _editContext.NotifyValidationStateChanged();
    }

    private void ValidateField(FieldIdentifier fieldIdentifier)
    {
        if (_editContext == null) return;

        var validator = GetValidator();
        if (validator == null) return;

        var messages = new ValidationMessageStore(_editContext);
        messages.Clear(fieldIdentifier);

        var context = new ValidationContext<object>(_editContext.Model);
        var result = validator.Validate(context);

        var fieldErrors = result.Errors.Where(x => x.PropertyName == fieldIdentifier.FieldName);
        foreach (var error in fieldErrors)
        {
            messages.Add(fieldIdentifier, error.ErrorMessage);
        }

        _editContext.NotifyValidationStateChanged();
    }

    private IValidator? GetValidator()
    {
        if (Validator != null) return Validator;
        if (ValidatorType != null)
        {
            return ServiceProvider.GetService(ValidatorType) as IValidator;
        }

        // Try to resolve generic validator for the model
        var modelType = _editContext?.Model.GetType();
        if (modelType != null)
        {
            var validatorType = typeof(IValidator<>).MakeGenericType(modelType);
            return ServiceProvider.GetService(validatorType) as IValidator;
        }

        return null;
    }

    public void Dispose()
    {
        if (_editContext != null)
        {
            _editContext.OnValidationRequested -= OnValidationRequested;
            _editContext.OnFieldChanged -= OnFieldChanged;
        }
    }
}
