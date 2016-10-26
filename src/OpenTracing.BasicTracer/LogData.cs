using System;
using System.Collections.Generic;

namespace OpenTracing.BasicTracer
{
    public struct LogData
    {
        public LogData(DateTime timestamp, IEnumerable<KeyValuePair<string, object>> fields)
        {
            Timestamp = timestamp;
            Fields = fields;
        }

        public DateTime Timestamp { get; }

        public IEnumerable<KeyValuePair<string, object>> Fields { get; }
    }
}
