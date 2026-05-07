namespace AnimalHaus.Shared.Utils;

public static class RetryPolicy
{
    public static T Execute<T>(Func<T> action, int maxAttempts = 3, int backoffMs = 50)
    {
        ArgumentNullException.ThrowIfNull(action);

        Exception? lastException = null;
        for (var attempt = 1; attempt <= maxAttempts; attempt++)
        {
            try
            {
                return action();
            }
            catch (Exception ex) when (attempt < maxAttempts)
            {
                lastException = ex;
                Thread.Sleep(backoffMs * attempt);
            }
        }

        throw lastException ?? new InvalidOperationException("Retry policy exhausted.");
    }
}
