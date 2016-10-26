using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace FBP.Logging
{
    public class FbpLoggingProvider : ILoggerProvider
    {
        private LogLevel _logLevel;
        private string _path;
        private string _fileName;
        private int _maxNumberOfFiles;
        private long _maxFileSize;

        public FbpLoggingProvider(LogLevel logLevel, string path, string fileName, int maxNumberOfFiles, long maxFileSize)
        {
            _logLevel = logLevel;
            _path = path;
            _fileName = fileName;
            _maxNumberOfFiles = maxNumberOfFiles;
            _maxFileSize = maxFileSize;
        }

        public ILogger CreateLogger(string categoryName)
        {
            return new FbpLogger(_logLevel, _path, _fileName, _maxNumberOfFiles, _maxFileSize);
        }

        public void Dispose()
        {
        }

    }
}
