using Microsoft.Extensions.Primitives;

namespace TechExchangeApp.Services
{
    /// <summary>
    /// Tín hiệu vô hiệu hóa cache trang chủ. HomeController gắn change-token này vào mỗi
    /// entry cache (home:index:{lang}:{domain}); gọi Clear() sẽ hủy toàn bộ entry cùng lúc,
    /// bất kể ngôn ngữ hay domain — dùng cho nút "Xóa cache trang chủ" trong CMS.
    /// </summary>
    public interface IHomeCacheSignal
    {
        IChangeToken GetChangeToken();
        void Clear();
    }

    public sealed class HomeCacheSignal : IHomeCacheSignal
    {
        private readonly object _lock = new();
        private CancellationTokenSource _cts = new();

        public IChangeToken GetChangeToken()
        {
            lock (_lock)
            {
                return new CancellationChangeToken(_cts.Token);
            }
        }

        public void Clear()
        {
            CancellationTokenSource old;
            lock (_lock)
            {
                old = _cts;
                _cts = new CancellationTokenSource();
            }
            old.Cancel();
            old.Dispose();
        }
    }
}
