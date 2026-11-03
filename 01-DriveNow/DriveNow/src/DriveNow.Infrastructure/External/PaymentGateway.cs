using DriveNow.Application.Abstractions;

namespace DriveNow.Infrastructure.External;

public class PaymentGateway : IPaymentGateway
{
    private readonly IDelayProvider _delayProvider;
    private readonly IRandomProvider _randomProvider;
    private readonly IReferenceGenerator _referenceGenerator;
    private readonly IConsoleWriter _consoleWriter;

    public PaymentGateway(
        IDelayProvider delayProvider,
        IRandomProvider randomProvider,
        IReferenceGenerator referenceGenerator,
        IConsoleWriter consoleWriter)
    {
        _delayProvider = delayProvider;
        _randomProvider = randomProvider;
        _referenceGenerator = referenceGenerator;
        _consoleWriter = consoleWriter;
    }

    public string Charge(int customerId, decimal amount, string description)
    {
        PauseAndMaybeFail();

        _consoleWriter.WriteLine(
            $"[Payment] Charged {amount:N2} EGP to customer #{customerId}");

        return "PAY-" + _referenceGenerator.Generate();
    }

    public void Refund(int customerId, decimal amount, string description)
    {
        PauseAndMaybeFail();

        _consoleWriter.WriteLine(
            $"[Payment] Refunded {amount:N2} EGP to customer #{customerId}");
    }

    private void PauseAndMaybeFail()
    {
        _delayProvider.Delay(1500);

        if (_randomProvider.Next(10) == 0)
        {
            throw new PaymentFailedException("Gateway timeout");
        }
    }
}