using System.Net;
using Microsoft.EntityFrameworkCore;
using TechExchangeApp.Data;
using TechExchangeApp.Entities;
using TechExchangeApp.Enums;
using TechExchangeApp.Services;

namespace TechExchangeApp.BackgroundServices
{
    /// <summary>
    /// Nhắc lịch sự kiện (HSMT A.4.5, A.8.4). Mỗi 5 phút: với sự kiện sắp diễn ra, khi đã qua mốc
    /// "trước giờ bắt đầu N phút" (EventInfos.ReminderOffsetsMinutes) thì đưa thông báo vào hàng đợi cho
    /// người đã đăng ký tham gia và người bấm "Nhắc lịch". Email có UserId sẽ hiện thêm ở chuông thông báo web.
    /// EventReminderLogs chống gửi trùng; mốc đã qua quá 2 giờ (ví dụ máy chủ tắt) thì bỏ, không gửi muộn.
    /// </summary>
    public class EventReminderBackgroundService : BackgroundService
    {
        private static readonly TimeSpan Interval = TimeSpan.FromMinutes(5);
        private static readonly TimeSpan MaxLateness = TimeSpan.FromHours(2);

        private readonly IServiceScopeFactory _scopeFactory;
        private readonly ILogger<EventReminderBackgroundService> _logger;

        public EventReminderBackgroundService(IServiceScopeFactory scopeFactory, ILogger<EventReminderBackgroundService> logger)
        {
            _scopeFactory = scopeFactory;
            _logger = logger;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    using var scope = _scopeFactory.CreateScope();
                    var sent = await RunOnceAsync(scope.ServiceProvider, DateTime.Now, stoppingToken);
                    if (sent > 0) _logger.LogInformation("[EventReminder] Đã đưa {N} nhắc lịch vào hàng đợi", sent);
                }
                catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested) { break; }
                catch (Exception ex)
                {
                    try { _logger.LogWarning(ex, "[EventReminder] Lỗi, thử lại ở chu kỳ sau."); } catch { }
                }

                try { await Task.Delay(Interval, stoppingToken); }
                catch (OperationCanceledException) { break; }
            }
        }

        /// <summary>Một lượt quét — tách riêng để CMS gọi "Gửi nhắc ngay" và để kiểm thử.</summary>
        public static async Task<int> RunOnceAsync(IServiceProvider sp, DateTime now, CancellationToken ct = default)
        {
            var db = sp.GetRequiredService<AppDbContext>();
            var events = sp.GetRequiredService<IEventService>();

            var upcoming = await db.EventInfos.AsNoTracking()
                .Where(e => e.StartAt > now && e.StartAt <= now.AddDays(31))
                .ToListAsync(ct);

            int queued = 0;
            foreach (var info in upcoming)
            {
                var dueOffsets = info.ReminderOffsets()
                    .Where(o => now >= info.StartAt.AddMinutes(-o) && now - info.StartAt.AddMinutes(-o) <= MaxLateness)
                    .ToList();
                if (dueOffsets.Count == 0) continue;

                var title = await db.Contents.AsNoTracking().Where(c => c.Id == info.ContentId).Select(c => c.Title).FirstOrDefaultAsync(ct) ?? "";
                var url = await events.EventUrlAsync(info.ContentId);

                var recipients = new List<(string Key, int? UserId, string? Email, string? Phone, string Name)>();
                recipients.AddRange((await db.EventRegistrations.AsNoTracking()
                        .Where(r => r.ContentId == info.ContentId && r.Status != EventRegistrationStatuses.Cancelled)
                        .Select(r => new { r.Id, r.UserId, r.Email, r.Phone, r.FullName }).ToListAsync(ct))
                    .Select(r => ("reg:" + r.Id, r.UserId, (string?)r.Email, r.Phone, r.FullName)));
                var registeredEmails = recipients.Select(r => r.Email?.ToLowerInvariant()).ToHashSet();
                recipients.AddRange((await db.EventInteractions.AsNoTracking()
                        .Where(i => i.ContentId == info.ContentId && i.InteractionType == EventInteractionTypes.Remind)
                        .Select(i => new { i.Id, i.UserId, i.Email, i.Phone }).ToListAsync(ct))
                    .Where(i => !registeredEmails.Contains(i.Email?.ToLowerInvariant()))
                    .Select(i => ("sub:" + i.Id, i.UserId, i.Email, i.Phone, "")));

                foreach (var offset in dueOffsets)
                {
                    var sentKeys = (await db.EventReminderLogs.AsNoTracking()
                        .Where(l => l.ContentId == info.ContentId && l.OffsetMinutes == offset)
                        .Select(l => l.RecipientKey).ToListAsync(ct)).ToHashSet();

                    foreach (var r in recipients.Where(r => !sentKeys.Contains(r.Key)))
                    {
                        var subject = $"Nhắc lịch: {title} – {info.StartAt:dd/MM/yyyy HH:mm}";
                        var body = $"<p>Kính gửi {WebUtility.HtmlEncode(string.IsNullOrEmpty(r.Name) ? "anh/chị" : r.Name)},</p>" +
                                   $"<p>Sự kiện <b>{WebUtility.HtmlEncode(title)}</b> sẽ bắt đầu lúc <b>{info.StartAt:HH:mm dd/MM/yyyy}</b>" +
                                   (string.IsNullOrEmpty(info.Location) ? "" : $" tại {WebUtility.HtmlEncode(info.Location)}") + ".</p>" +
                                   (string.IsNullOrEmpty(info.LivestreamUrl) ? "" : $"<p>Xem trực tuyến: <a href=\"{WebUtility.HtmlEncode(info.LivestreamUrl)}\">{WebUtility.HtmlEncode(info.LivestreamUrl)}</a></p>") +
                                   $"<p><a href=\"{WebUtility.HtmlEncode(url)}\">Xem chi tiết sự kiện</a></p>";

                        if ((info.ReminderChannels & EventReminderChannels.Email) != 0 && !string.IsNullOrEmpty(r.Email))
                            db.Notifications.Add(new Notification
                            {
                                UserId = r.UserId?.ToString(), Target = r.Email, Title = subject, Content = body, Url = url,
                                Channel = (int)NotificationChannel.Email, Status = (int)NotificationStatus.Pending, CreatedDate = DateTime.UtcNow
                            });
                        if ((info.ReminderChannels & EventReminderChannels.Sms) != 0 && !string.IsNullOrEmpty(r.Phone))
                            db.Notifications.Add(new Notification
                            {
                                Target = r.Phone, Title = subject,
                                Content = $"Nhac lich: {title} luc {info.StartAt:HH:mm dd/MM/yyyy}. Chi tiet: {url}",
                                Channel = (int)NotificationChannel.Sms, Status = (int)NotificationStatus.Pending, CreatedDate = DateTime.UtcNow
                            });

                        db.EventReminderLogs.Add(new EventReminderLog { ContentId = info.ContentId, RecipientKey = r.Key, OffsetMinutes = offset, Created = DateTime.Now });
                        queued++;
                    }
                    await db.SaveChangesAsync(ct);
                }
            }
            return queued;
        }
    }
}
