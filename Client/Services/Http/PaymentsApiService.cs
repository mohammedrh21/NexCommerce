using Client.Models.Payments;

namespace Client.Services.Http;

public interface IPaymentsApiService
{
    Task<PaymentIntentDto> CreateIntentAsync(CreatePaymentIntentRequest request, CancellationToken ct = default);
    Task<PaymentDto> ConfirmPaymentAsync(ProcessPaymentRequest request, CancellationToken ct = default);
}

public class PaymentsApiService : BaseApiService, IPaymentsApiService
{
    public PaymentsApiService(HttpClient httpClient) : base(httpClient) { }

    public Task<PaymentIntentDto> CreateIntentAsync(CreatePaymentIntentRequest request, CancellationToken ct = default)
        => PostAsync<CreatePaymentIntentRequest, PaymentIntentDto>("api/v1/payments/create-intent", request, ct);

    public Task<PaymentDto> ConfirmPaymentAsync(ProcessPaymentRequest request, CancellationToken ct = default)
        => PostAsync<ProcessPaymentRequest, PaymentDto>("api/v1/payments/confirm", request, ct);
}
