using System;
using System.Collections.Generic;

namespace OpenTracing.BasicTracer
{
    public class Span : ISpan
    {
        private readonly ISpanRecorder _spanRecorder;

        private readonly SpanContext _context;

        public ISpanContext Context => _context;

        public string OperationName { get; private set; }
        public DateTime StartTimestamp { get; }
        public DateTime? FinishTimestamp { get; private set; }

        public IDictionary<string, object> Tags { get; } = new Dictionary<string, object>();
        public IList<LogData> Logs { get; } = new List<LogData>();

        internal Span(
            ISpanRecorder spanRecorder,
            SpanContext context,
            string operationName,
            DateTime startTimestamp,
            IDictionary<string, object> tags)
        {
            if (spanRecorder == null)
            {
                throw new ArgumentNullException(nameof(spanRecorder));
            }

            if (context == null)
            {
                throw new ArgumentNullException(nameof(context));
            }

            if (string.IsNullOrWhiteSpace(operationName))
            {
                throw new ArgumentNullException(operationName);
            }

            _spanRecorder = spanRecorder;

            _context = context;
            OperationName = operationName.Trim();
            StartTimestamp = startTimestamp;

            if (tags != null)
            {
                foreach (var tag in tags)
                {
                    Tags.Add(tag);
                }
            }
        }

        public virtual ISpan SetOperationName(string operationName)
        {
            if (string.IsNullOrWhiteSpace(operationName))
            {
                throw new ArgumentNullException(nameof(operationName));
            }

            OperationName = operationName;
            return this;
        }

        public virtual ISpan SetTag(string key, bool value)
        {
            if (string.IsNullOrWhiteSpace(key))
            {
                throw new ArgumentNullException(nameof(key));
            }

            Tags[key] = value;
            return this;
        }

        public virtual ISpan SetTag(string key, double value)
        {
            if (string.IsNullOrWhiteSpace(key))
            {
                throw new ArgumentNullException(nameof(key));
            }

            Tags[key] = value;
            return this;
        }

        public virtual ISpan SetTag(string key, int value)
        {
            if (string.IsNullOrWhiteSpace(key))
            {
                throw new ArgumentNullException(nameof(key));
            }

            Tags[key] = value;
            return this;
        }

        public virtual ISpan SetTag(string key, string value)
        {
            if (string.IsNullOrWhiteSpace(key))
            {
                throw new ArgumentNullException(nameof(key));
            }

            Tags[key] = value;
            return this;
        }

        public ISpan Log(IEnumerable<KeyValuePair<string, object>> fields)
        {
            return Log(DateTime.UtcNow, fields);
        }

        public ISpan Log(DateTime timestamp, IEnumerable<KeyValuePair<string, object>> fields)
        {
            Logs.Add(new LogData(timestamp, fields));
            return this;
        }

        public ISpan Log(string eventName)
        {
            return Log(DateTime.UtcNow, eventName);
        }

        public ISpan Log(DateTime timestamp, string eventName)
        {
            return Log(timestamp, new Dictionary<string, object> { { "event", eventName }});
        }

        public ISpan SetBaggageItem(string key, string value)
        {
            _context.SetBaggageItem(key, value);
            return this;
        }

        public string GetBaggageItem(string key)
        {
            return _context.GetBaggageItem(key);
        }

        public void Finish()
        {
            Finish(DateTime.UtcNow);
        }

        public void Finish(DateTime finishTimestamp)
        {
            if (FinishTimestamp.HasValue)
                return;

            FinishTimestamp = finishTimestamp;
            OnFinished();
        }

        public void Dispose()
        {
            Finish();
        }

        protected void OnFinished()
        {
            var spanData = new SpanData()
            {
                Context = this.TypedContext(),
                OperationName = OperationName,
                StartTimestamp = StartTimestamp,
                Duration = FinishTimestamp.Value - StartTimestamp,
                Tags = Tags,
                LogData = Logs,
            };

            _spanRecorder.RecordSpan(spanData);
        }
    }
}