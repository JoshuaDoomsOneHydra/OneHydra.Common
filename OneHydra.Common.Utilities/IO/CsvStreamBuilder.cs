using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace OneHydra.Common.Utilities.IO
{
    public class CsvStreamBuilder
    {
        private string _header;
        private readonly IList<string> _records;

        public CsvStreamBuilder()
        {
            _records = new List<string>();
        }

        public Stream Build()
        {
            var data = String.Join("\r\n", _records);

            if (!string.IsNullOrEmpty(_header))
            {
                data = string.Concat(_header, "\r\n", data);
            }

            return new MemoryStream(Encoding.UTF8.GetBytes(data));
        }

        public CsvStreamBuilder WithRecord(params object[] cells)
        {
            _records.Add(String.Join(",", cells));
            return this;
        }

        public CsvStreamBuilder WithHeader(params object[] headerCells)
        {
            _header = String.Join(",", headerCells);
            return this;
        }

        public CsvStreamBuilder WithComment(string commentLine)
        {
            _records.Add(commentLine);
            return this;
        }
    }
}
