using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace FBP.Logging
{
    public static class LoggerFactoryExtensions
    {
        public static ILoggerFactory AddFbpLogger(this ILoggerFactory loggerFactory, LogLevel logLevel, string path, string fileName, int maxNumberOfFiles, long maxFileSize)
        {
            FbpLoggingProvider f = new FbpLoggingProvider(logLevel, path, fileName, maxNumberOfFiles, maxFileSize);
            loggerFactory.AddProvider(f);
            return loggerFactory;
        }
    }
}
