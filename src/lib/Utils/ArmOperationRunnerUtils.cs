using Azure;
using Azure.ResourceManager;
using Microsoft.Extensions.Logging;

namespace lib.Utils;

public static class ArmOperationExtensions
{
    public static async Task<TResult> TriggerAndWaitAsync<TResult>(this Task<ArmOperation<TResult>?> operation,
        bool configureWait = false, ILogger? logger = null)
    {
        try
        {
            logger?.LogDebug("Running ARM operation");

            var updater = await operation.ConfigureAwait(false);

            if (updater == null)
                throw new ApplicationException("Unable to get ARM operation updater");

            var runner = await updater.WaitForCompletionAsync().ConfigureAwait(configureWait);
            logger?.LogDebug("ARM operation completed");
            logger?.LogDebug("ARM operation result: {@result}", runner.GetRawResponse().Status);
            return runner.Value;
        }
        catch (RequestFailedException ex)
        {
            logger?.LogError(ex, "ARM operation failed");
            throw;
        }
    }

    public static async Task<TResult> TriggerAndWaitAsync<TResult>(this Task<ArmOperation<TResult>?> operation,
        string preMessage,
        string successMessage, string failMessage, bool configureWait = false,
        TResult? whenNull = default, ILogger? logger = null)
    {
        try
        {
            logger?.LogDebug(preMessage);
            var updater = await operation.ConfigureAwait(false);
                
            if (updater == null)
                throw new ApplicationException("Unable to get ARM operation updater");

            var runner = await updater.WaitForCompletionAsync().ConfigureAwait(false);
            logger?.LogDebug(successMessage);
            logger?.LogDebug("ARM operation result: {@result}", runner.GetRawResponse().Status);
            return runner.Value;
        }
        catch (RequestFailedException ex)
        {
            logger?.LogError(ex, failMessage);
            if (whenNull != null)
                return whenNull;
            throw;
        }
    }
}