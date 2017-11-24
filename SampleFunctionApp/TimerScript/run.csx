using System;

public static void Run(TimerInfo myTimer, TraceWriter log)
{
    log.Info($"Keep Warm Timer trigger function executed at: {DateTime.Now}");
}