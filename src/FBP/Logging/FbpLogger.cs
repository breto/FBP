using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace FBP.Logging
{
    public class FbpLogger : ILogger
    {
        private LogLevel _logLevel;
        private string _path;
        private string _fileName;
        private int _maxNumberOfFiles;
        private long _maxFileSize;

        public FbpLogger(LogLevel logLevel, string path, string fileName, int maxNumberOfFiles, long maxFileSize)
        {
            _logLevel = logLevel;
            _path = path;
            _fileName = fileName;
            _maxNumberOfFiles = maxNumberOfFiles;
            _maxFileSize = maxFileSize;
        }
        public IDisposable BeginScope<TState>(TState state)
        {
            return null;
        }

        public bool IsEnabled(LogLevel logLevel)
        {
            return _logLevel >= logLevel;
        }

        public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception exception, Func<TState, Exception, string> formatter)
        {
            if (logLevel.CompareTo(_logLevel) >=0 && state != null)
            {
                lock (_fileName)
                {

                    if (File.Exists(_path + _fileName))
                    {
                        FileInfo fi = new FileInfo(_path + _fileName);

                        //check if next log statement will make it larger than max allowed size

                        if (fi.Length + state.ToString().Length > _maxFileSize)
                        {
                            DirectoryInfo di = new DirectoryInfo(@_path);
                            FileInfo[] files = di.GetFiles(_fileName + "*");
                            Array.Reverse(files);
                            int currentFileCount = files.Count();
                            //if current file count is equal or greater than max allowed, remove all extra files
                            if (currentFileCount >= _maxNumberOfFiles)
                            {
                                for (int i = 0; i < currentFileCount + 1 - _maxNumberOfFiles; i++)
                                {
                                    File.Delete(_path + files[i].Name);
                                }
                            }
                            // rename all the remaining and create the new one
                            di = new DirectoryInfo(@_path);
                            files = di.GetFiles(_fileName + "*");
                            Array.Reverse(files);
                            for (int i = 0; i < files.Length; i++)
                            {
                                int fileNumberExt = -1;
                                if (!Int32.TryParse(Path.GetExtension(files[i].Name).Replace(".", ""), out fileNumberExt))
                                {
                                    fileNumberExt = -1;
                                }

                                if (fileNumberExt == -1)
                                {
                                    File.Move(_path + files[i].Name, _path + files[i].Name + ".0");
                                }
                                else
                                {
                                    File.Move(_path + files[i].Name, _path + Path.GetFileNameWithoutExtension(files[i].Name) + "." + (fileNumberExt + 1));
                                }

                            }
                        }

                    }
                    // Create a file to write to.
                    using (StreamWriter sw = File.AppendText(_path + _fileName))
                    {
                        sw.WriteLine(state.ToString());
                        if(exception != null)
                        {
                            sw.WriteLine(exception.StackTrace);
                        }
                    }
                }
            }
        }
    }
}
