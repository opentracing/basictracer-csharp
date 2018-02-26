using System;
using System.Collections.Generic;

namespace OpenTracing.BasicTracer
{
    public struct LogData
    {
        public LogData(DateTimeOffset timestamp, IEnumerable<KeyValuePair<string, object>> fields)
        {
            Timestamp = timestamp;
            Fields = fields;
        }

        public DateTimeOffset Timestamp { get; }

        public IEnumerable<KeyValuePair<string, object>> Fields { get; }
    }
}
