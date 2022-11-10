﻿using System.Diagnostics;
using Cassandra;
using CecoChat.Otel;

namespace CecoChat.Data.History.Telemetry;

internal interface IHistoryTelemetry
{
    Activity StartAddDataMessage(ISession session, long messageID);

    Activity StartGetHistory(ISession session, long userID);

    Activity StartSetReaction(ISession session, long reactorID);

    Activity StartUnsetReaction(ISession session, long reactorID);

    void Stop(Activity activity, bool operationSuccess);
}

internal sealed class HistoryTelemetry : IHistoryTelemetry
{
    private readonly ITelemetry _telemetry;

    public HistoryTelemetry(ITelemetry telemetry)
    {
        _telemetry = telemetry;
    }

    public Activity StartAddDataMessage(ISession session, long messageID)
    {
        Activity activity = _telemetry.Start(
            HistoryInstrumentation.Operations.AddDataMessage,
            HistoryInstrumentation.ActivitySource,
            ActivityKind.Client,
            Activity.Current?.Context);

        if (activity.IsAllDataRequested)
        {
            Enrich(OtelInstrumentation.Values.DbOperationBatchWrite, session, activity);
            activity.SetTag("message.id", messageID);
        }

        return activity;
    }

    public Activity StartGetHistory(ISession session, long userID)
    {
        Activity activity = _telemetry.Start(
            HistoryInstrumentation.Operations.GetHistory,
            HistoryInstrumentation.ActivitySource,
            ActivityKind.Client,
            Activity.Current?.Context);

        if (activity.IsAllDataRequested)
        {
            Enrich(OtelInstrumentation.Values.DbOperationOneRead, session, activity);
            activity.SetTag("user.id", userID);
        }

        return activity;
    }

    public Activity StartSetReaction(ISession session, long reactorID)
    {
        Activity activity = _telemetry.Start(
            HistoryInstrumentation.Operations.SetReaction,
            HistoryInstrumentation.ActivitySource,
            ActivityKind.Client,
            Activity.Current?.Context);

        if (activity.IsAllDataRequested)
        {
            Enrich(OtelInstrumentation.Values.DbOperationOneWrite, session, activity);
            activity.SetTag("reaction.reactor_id", reactorID);
        }

        return activity;
    }

    public Activity StartUnsetReaction(ISession session, long reactorID)
    {
        Activity activity = _telemetry.Start(
            HistoryInstrumentation.Operations.UnsetReaction,
            HistoryInstrumentation.ActivitySource,
            ActivityKind.Client,
            Activity.Current?.Context);

        if (activity.IsAllDataRequested)
        {
            Enrich(OtelInstrumentation.Values.DbOperationOneWrite, session, activity);
            activity.SetTag("reaction.reactor_id", reactorID);
        }

        return activity;
    }

    public void Stop(Activity activity, bool operationSuccess)
    {
        _telemetry.Stop(activity, operationSuccess);
    }

    private static void Enrich(string operation, ISession session, Activity activity)
    {
        activity.SetTag(OtelInstrumentation.Keys.DbOperation, operation);
        activity.SetTag(OtelInstrumentation.Keys.DbSystem, OtelInstrumentation.Values.DbSystemCassandra);
        activity.SetTag(OtelInstrumentation.Keys.DbName, session.Keyspace);
        activity.SetTag(OtelInstrumentation.Keys.DbSessionName, session.SessionName);
    }
}