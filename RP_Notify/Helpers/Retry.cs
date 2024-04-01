using System;
using System.Collections.Generic;
using System.Threading;

namespace RP_Notify.Helpers
{
    public static class Retry
    {
        public static void Do(
            Action action,
            int retryIntervalMillisecs = 100,
            int maxAttemptCount = 20)
        {
            Do<object>(() =>
            {
                action();
                return null;
            }, retryIntervalMillisecs, maxAttemptCount);
        }

        public static void Do(
            Action action,
            TimeSpan retryInterval,
            int maxAttemptCount = 3)
        {
            Do<object>(() =>
            {
                action();
                return null;
            }, retryInterval, maxAttemptCount);
        }

        public static T Do<T>(
            Func<T> action,
            int retryIntervalMillisecs = 100,
            int maxAttemptCount = 20)
        {
            var exceptions = new List<Exception>();

            for (int attempted = 0; attempted < maxAttemptCount; attempted++)
            {
                try
                {
                    if (attempted > 0)
                    {
                        Thread.Sleep(TimeSpan.FromMilliseconds(retryIntervalMillisecs));
                    }
                    return action();
                }
                catch (Exception ex)
                {
                    exceptions.Add(ex);
                }
            }
            throw new AggregateException(exceptions);
        }

        public static T Do<T>(
            Func<T> action,
            TimeSpan retryInterval,
            int maxAttemptCount = 3)
        {
            var exceptions = new List<Exception>();

            for (int attempted = 0; attempted < maxAttemptCount; attempted++)
            {
                try
                {
                    if (attempted > 0)
                    {
                        Thread.Sleep(retryInterval);
                    }
                    return action();
                }
                catch (Exception ex)
                {
                    exceptions.Add(ex);
                }
            }
            throw new AggregateException(exceptions);
        }
    }

}
