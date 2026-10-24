using System.Collections.Concurrent;
using System.Text;
using System.Threading.Channels;

namespace TechExchangeApp.Infrastructure.Logging
{
    /// <summary>Cấu hình ghi log hệ thống ra file (appsettings: FileLogging).</summary>
    public class FileLoggerOptions
    {
        public bool Enabled { get; set; } = true;
        /// <summary>Thư mục log; đường dẫn tương đối tính từ thư mục ứng dụng.</summary>
        public string Path { get; set; } = "logs";
        /// <summary>Số ngày giữ file log; file cũ hơn tự xóa.</summary>
        public int RetentionDays { get; set; } = 180;
    }

    /// <summary>
    /// Ghi log hệ thống ra file theo ngày (logs/app-yyyyMMdd.log) để cung cấp cho đơn vị giám sát.
    /// Ghi bất đồng bộ qua hàng đợi, không chặn request; lỗi ghi file không làm dừng ứng dụng.
    /// Mức log theo cấu hình "Logging" chung, có thể chỉnh riêng tại "Logging:File:LogLevel".
    /// </summary>
    [ProviderAlias("File")]
    public sealed class FileLoggerProvider : ILoggerProvider
    {
        private readonly string _dir;
        private readonly int _retentionDays;
        private readonly Channel<(DateTime Time, string Line)> _queue =
            Channel.CreateBounded<(DateTime, string)>(new BoundedChannelOptions(10000) { FullMode = BoundedChannelFullMode.DropOldest });
        private readonly ConcurrentDictionary<string, FileLogger> _loggers = new();
        private readonly Task _writer;

        public FileLoggerProvider(string contentRoot, FileLoggerOptions options)
        {
            _dir = System.IO.Path.IsPathRooted(options.Path) ? options.Path : System.IO.Path.Combine(contentRoot, options.Path);
            _retentionDays = Math.Max(1, options.RetentionDays);
            _writer = Task.Run(WriteLoopAsync);
        }

        public ILogger CreateLogger(string categoryName) =>
            _loggers.GetOrAdd(categoryName, name => new FileLogger(name, this));

        internal void Enqueue(DateTime time, string line) => _queue.Writer.TryWrite((time, line));

        private async Task WriteLoopAsync()
        {
            StreamWriter? writer = null;
            DateTime currentDay = default;
            try
            {
                while (await _queue.Reader.WaitToReadAsync())
                {
                    while (_queue.Reader.TryRead(out var item))
                    {
                        try
                        {
                            if (writer == null || item.Time.Date != currentDay)
                            {
                                writer?.Dispose();
                                currentDay = item.Time.Date;
                                Directory.CreateDirectory(_dir);
                                writer = new StreamWriter(
                                    new FileStream(System.IO.Path.Combine(_dir, $"app-{currentDay:yyyyMMdd}.log"),
                                        FileMode.Append, FileAccess.Write, FileShare.ReadWrite),
                                    new UTF8Encoding(false));
                                DeleteOldFiles();
                            }
                            writer.WriteLine(item.Line);
                        }
                        catch
                        {
                            // Không ghi được (thiếu quyền, đầy đĩa...) thì bỏ qua dòng này
                            writer?.Dispose();
                            writer = null;
                        }
                    }
                    try { writer?.Flush(); } catch { writer = null; }
                }
            }
            finally
            {
                writer?.Dispose();
            }
        }

        private void DeleteOldFiles()
        {
            try
            {
                var cutoff = DateTime.Now.Date.AddDays(-_retentionDays);
                foreach (var f in Directory.EnumerateFiles(_dir, "app-*.log"))
                    if (File.GetLastWriteTime(f) < cutoff) File.Delete(f);
            }
            catch { }
        }

        public void Dispose()
        {
            _queue.Writer.TryComplete();
            try { _writer.Wait(TimeSpan.FromSeconds(5)); } catch { }
        }

        private sealed class FileLogger : ILogger
        {
            private readonly string _category;
            private readonly FileLoggerProvider _provider;

            public FileLogger(string category, FileLoggerProvider provider)
            {
                _category = category;
                _provider = provider;
            }

            public IDisposable? BeginScope<TState>(TState state) where TState : notnull => null;

            public bool IsEnabled(LogLevel logLevel) => logLevel != LogLevel.None;

            public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception? exception, Func<TState, Exception?, string> formatter)
            {
                if (!IsEnabled(logLevel)) return;
                var now = DateTime.Now;
                var sb = new StringBuilder()
                    .Append(now.ToString("yyyy-MM-dd HH:mm:ss.fff")).Append(" [")
                    .Append(LevelText(logLevel)).Append("] ")
                    .Append(_category).Append(": ")
                    .Append(formatter(state, exception));
                if (exception != null) sb.AppendLine().Append(exception);
                _provider.Enqueue(now, sb.ToString());
            }

            private static string LevelText(LogLevel level) => level switch
            {
                LogLevel.Trace => "TRC",
                LogLevel.Debug => "DBG",
                LogLevel.Information => "INF",
                LogLevel.Warning => "WRN",
                LogLevel.Error => "ERR",
                LogLevel.Critical => "CRT",
                _ => level.ToString()
            };
        }
    }
}
