﻿using OpenTelemetry;

namespace CecoChat.Otel;

public sealed class JaegerOptions
{
    public string AgentHost { get; set; }

    public int AgentPort { get; set; }

    public ExportProcessorType ExportProcessorType { get; set; }

    public int BatchExportScheduledDelayMillis { get; set; }
}