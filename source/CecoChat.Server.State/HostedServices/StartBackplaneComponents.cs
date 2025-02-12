using CecoChat.Server.State.Backplane;
using CecoChat.Threading;

namespace CecoChat.Server.State.HostedServices;

public sealed class StartBackplaneComponents : IHostedService, IDisposable
{
    private readonly ILogger _logger;
    private readonly IStateConsumer _stateConsumer;
    private readonly CancellationToken _appStoppingCt;
    private readonly ReceiversConsumerHealthCheck _receiversConsumerHealthCheck;
    private readonly SendersConsumerHealthCheck _sendersConsumerHealthCheck;
    private CancellationTokenSource? _stoppedCts;
    private DedicatedThreadTaskScheduler? _receiverMessagesTaskScheduler;
    private DedicatedThreadTaskScheduler? _senderMessagesTaskScheduler;

    public StartBackplaneComponents(
        ILogger<StartBackplaneComponents> logger,
        IStateConsumer stateConsumer,
        ReceiversConsumerHealthCheck receiversConsumerHealthCheck,
        SendersConsumerHealthCheck sendersConsumerHealthCheck,
        IHostApplicationLifetime applicationLifetime)
    {
        _logger = logger;
        _stateConsumer = stateConsumer;
        _receiversConsumerHealthCheck = receiversConsumerHealthCheck;
        _sendersConsumerHealthCheck = sendersConsumerHealthCheck;

        _appStoppingCt = applicationLifetime.ApplicationStopping;
    }

    public void Dispose()
    {
        _senderMessagesTaskScheduler?.Dispose();
        _receiverMessagesTaskScheduler?.Dispose();
        _stoppedCts?.Dispose();
        _stateConsumer.Dispose();
    }

    public Task StartAsync(CancellationToken cancellationToken)
    {
        _stoppedCts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken, _appStoppingCt);
        _stateConsumer.Prepare();

        _receiverMessagesTaskScheduler = new DedicatedThreadTaskScheduler();
        Task.Factory.StartNew(() =>
        {
            try
            {
                _receiversConsumerHealthCheck.IsReady = true;
                _stateConsumer.StartConsumingReceiverMessages(_stoppedCts.Token);
            }
            catch (Exception exception)
            {
                _logger.LogCritical(exception, "Failure in consumer {ConsumerId}", _stateConsumer.ReceiverConsumerId);
            }
            finally
            {
                _receiversConsumerHealthCheck.IsReady = false;
            }
        }, _stoppedCts.Token, TaskCreationOptions.LongRunning, _receiverMessagesTaskScheduler);

        _senderMessagesTaskScheduler = new DedicatedThreadTaskScheduler();
        Task.Factory.StartNew(() =>
        {
            try
            {
                _sendersConsumerHealthCheck.IsReady = true;
                _stateConsumer.StartConsumingSenderMessages(_stoppedCts.Token);
            }
            catch (Exception exception)
            {
                _logger.LogCritical(exception, "Failure in consumer {ConsumerId}", _stateConsumer.SenderConsumerId);
            }
            finally
            {
                _sendersConsumerHealthCheck.IsReady = false;
            }
        }, _stoppedCts.Token, TaskCreationOptions.LongRunning, _senderMessagesTaskScheduler);

        return Task.CompletedTask;
    }

    public Task StopAsync(CancellationToken cancellationToken)
    {
        return Task.CompletedTask;
    }
}
