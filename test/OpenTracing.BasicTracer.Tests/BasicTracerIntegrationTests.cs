﻿using System;
using System.Collections.Generic;
using System.Linq;
using OpenTracing.Propagation;
using Xunit;

namespace OpenTracing.BasicTracer.IntegrationTests
{
    public class BasicTracerIntegrationTests
    {
        private Tracer GetTracer(ISpanRecorder recorder = null)
        {
            var spanRecorder = recorder ?? new SimpleMockRecorder();
            var spanContextFactory = new SpanContextFactory();

            return new Tracer(spanContextFactory, spanRecorder);
        }

        [Fact]
        public void DefaultBasicTracer_WhenStartSpanCalled_ReturnsSpan()
        {
            var tracer = GetTracer();

            var span = tracer.BuildSpan("TestOperation").Start();

            Assert.NotNull(span);
        }

        [Fact]
        public void DefaultBasicTracer_WhenSpanInjectedToMemoryCarrier_Work()
        {
            var tracer = GetTracer();

            var span = tracer.BuildSpan("TestOperation").Start();

            var traceId = span.TypedContext().TraceId;
            var spanId = span.TypedContext().SpanId;

            var data = new Dictionary<string, string>();

            tracer.Inject(span.Context, Formats.TextMap, new DictionaryCarrier(data));

            Assert.Equal(traceId.ToString(), data["ot-traceid"]);
            Assert.Equal(spanId.ToString(), data["ot-spanid"]);
        }

        [Fact]
        public void DefaultBasicTracer_WhenJoinBadSpanToMemoryCarrier_Fails()
        {
            var tracer = GetTracer();

            var data = new Dictionary<string, string>();

            var spanContext = tracer.Extract(Formats.TextMap, new DictionaryCarrier(data));

            Assert.Null(spanContext);
        }

        [Fact]
        public void DefaultBasicTracer_WhenJoinValidSpanToMemoryCarrier_Works()
        {
            var tracer = GetTracer();

            var testTraceId = Guid.NewGuid();
            var testSpanId = Guid.NewGuid();

            var data = new Dictionary<string, string>()
            {
                { "ot-traceid", testTraceId.ToString() },
                { "ot-spanid", testSpanId.ToString() },
            };

            var spanContext = (SpanContext)tracer.Extract(Formats.TextMap, new DictionaryCarrier(data));

            Assert.NotNull(spanContext);

            Assert.Equal(testTraceId, spanContext.TraceId);
            Assert.Equal(testSpanId, spanContext.SpanId);
        }

        [Fact]
        public void DefaultBasicTracer_WhenFinishSpan_CallsRecorderWithAllSpanData()
        {
            var recorder = new SimpleMockRecorder();
            var tracer = GetTracer(recorder: recorder);

            var startTimestamp = new DateTimeOffset(2016, 1, 1, 12, 0, 0, TimeSpan.Zero);
            var finishTimestamp = new DateTimeOffset(2016, 1, 1, 12, 0, 5, TimeSpan.Zero);

            var span = tracer.BuildSpan("TestOperation")
                .WithStartTimestamp(startTimestamp)
                .Start()
                .SetTag("tagkey", "TagValue")
                .SetBaggageItem("baggagekey", "BaggageValue");

            span.Finish(finishTimestamp);

            var recordedSpan = recorder.Spans.First();

            Assert.Equal("TestOperation", recordedSpan.OperationName);
            Assert.Equal(startTimestamp, recordedSpan.StartTimestamp);
            Assert.Equal(TimeSpan.FromSeconds(5), recordedSpan.Duration);

            Assert.Equal("BaggageValue", recordedSpan.Context.GetBaggageItem("baggagekey"));
            Assert.Equal("TagValue", recordedSpan.Tags["tagkey"]);

            Assert.NotEqual(Guid.Empty, recordedSpan.Context.TraceId);
            Assert.NotEqual(Guid.Empty, recordedSpan.Context.SpanId);
        }
    }
}