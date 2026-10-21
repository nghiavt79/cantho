using System.Net;
using System.Security.Cryptography;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Options;
using TechExchangeApp.Data;
using TechExchangeApp.Entities;
using TechExchangeApp.Enums;
using TechExchangeApp.Helpers;

namespace TechExchangeApp.Services
{
    public interface IEventService
    {
        /// <summary>Đưa Contents.Id (kể cả bản EN) về Id bản tiếng Việt — khóa của mọi bảng sự kiện.</summary>
        Task<long> ResolveContentIdAsync(long contentId);
        Task<EventPanelVm?> GetPanelAsync(long contentId, int? userId, string? visitorKey);
        Task RecordViewAsync(long contentId, int? userId, string? visitorKey);
        /// <summary>Ghi lượt nhấp banner/đường dẫn hoặc lượt chia sẻ (A.4.4).</summary>
        Task RecordInteractionAsync(long contentId, byte type, int? userId, string? visitorKey);
        Task<(bool Ok, string? ErrorKey, string? TicketUrl)> RegisterAsync(long contentId, EventRegisterInput input, int? userId, string? ip, bool isEn);
        Task<(bool Active, int Count)> ToggleInterestAsync(long contentId, int? userId, string visitorKey);
        Task<(bool Ok, string? ErrorKey)> SubscribeReminderAsync(long contentId, int? userId, string visitorKey, string? email, string? phone);
        Task<(bool Ok, string? ErrorKey)> BookBoothAsync(long contentId, EventBoothInput input, int? userId);
        Task<EventTicketVm?> GetTicketAsync(string code);
        string TicketUrl(string code, bool isEn);
        Task<string> EventUrlAsync(long contentId);
    }

    public class EventRegisterInput
    {
        public string? FullName { get; set; }
        public string? Email { get; set; }
        public string? Phone { get; set; }
        public string? Organization { get; set; }
        public string? Note { get; set; }
    }

    public class EventBoothInput
    {
        public int BoothTypeId { get; set; }
        public string? Organization { get; set; }
        public string? ContactName { get; set; }
        public string? Email { get; set; }
        public string? Phone { get; set; }
        public int Quantity { get; set; } = 1;
        public string? Note { get; set; }
    }

    public class EventPanelVm
    {
        public long ContentId { get; set; }
        public EventInfo Info { get; set; } = null!;
        public int RegisteredCount { get; set; }
        public int InterestCount { get; set; }
        public int RemindCount { get; set; }
        public bool IsInterested { get; set; }
        public bool IsReminderSubscribed { get; set; }
        public bool RegistrationOpen { get; set; }
        public string? RegistrationClosedReasonKey { get; set; }
        public EventRegistration? MyRegistration { get; set; }
        public List<EventBoothAvailability> Booths { get; set; } = new();
        public string? PrefillName { get; set; }
        public string? PrefillEmail { get; set; }
        public string? PrefillPhone { get; set; }
    }

    public class EventBoothAvailability
    {
        public EventBoothType Booth { get; set; } = null!;
        public int Remaining { get; set; }
    }

    public class EventTicketVm
    {
        public EventRegistration Registration { get; set; } = null!;
        public string EventTitle { get; set; } = "";
        public EventInfo? Info { get; set; }
        public string QrDataUri { get; set; } = "";
        public string EventUrl { get; set; } = "";
    }

    /// <summary>Nghiệp vụ sự kiện phía người dùng (HSMT A.4.1, A.4.3, A.4.4, A.4.5, B.1.6).</summary>
    public class EventService : IEventService
    {
        private const int MaxRegistrationsPerIpPer10Min = 5;

        private readonly AppDbContext _db;
        private readonly IMemoryCache _cache;
        private readonly string _mainDomain;
        private readonly ILogger<EventService> _logger;

        public EventService(AppDbContext db, IMemoryCache cache, IOptions<AppSettings> appSettings, ILogger<EventService> logger)
        {
            _db = db;
            _cache = cache;
            _mainDomain = (appSettings.Value.MainDomain ?? "").TrimEnd('/') + "/";
            _logger = logger;
        }

        public async Task<long> ResolveContentIdAsync(long contentId)
        {
            var c = await _db.Contents.AsNoTracking().Where(x => x.Id == contentId)
                .Select(x => new { x.LanguageId, x.OriginalId }).FirstOrDefaultAsync();
            return c != null && c.LanguageId == LangHelper.En && c.OriginalId.HasValue ? c.OriginalId.Value : contentId;
        }

        public async Task<EventPanelVm?> GetPanelAsync(long contentId, int? userId, string? visitorKey)
        {
            var info = await _db.EventInfos.AsNoTracking().FirstOrDefaultAsync(e => e.ContentId == contentId);
            if (info == null) return null;

            var vm = new EventPanelVm { ContentId = contentId, Info = info };
            vm.RegisteredCount = await _db.EventRegistrations.CountAsync(r => r.ContentId == contentId && r.Status != EventRegistrationStatuses.Cancelled);
            var counts = await _db.EventInteractions.Where(i => i.ContentId == contentId && i.InteractionType != EventInteractionTypes.View)
                .GroupBy(i => i.InteractionType).Select(g => new { g.Key, N = g.Count() }).ToListAsync();
            vm.InterestCount = counts.FirstOrDefault(c => c.Key == EventInteractionTypes.Interest)?.N ?? 0;
            vm.RemindCount = counts.FirstOrDefault(c => c.Key == EventInteractionTypes.Remind)?.N ?? 0;

            var mine = _db.EventInteractions.Where(i => i.ContentId == contentId &&
                ((userId != null && i.UserId == userId) || (userId == null && visitorKey != null && i.VisitorKey == visitorKey)));
            vm.IsInterested = await mine.AnyAsync(i => i.InteractionType == EventInteractionTypes.Interest);
            vm.IsReminderSubscribed = await mine.AnyAsync(i => i.InteractionType == EventInteractionTypes.Remind);

            if (userId.HasValue)
            {
                var u = await _db.Users.AsNoTracking().Where(x => x.Id == userId).Select(x => new { x.FullName, x.Email, x.PhoneNumber }).FirstOrDefaultAsync();
                vm.PrefillName = u?.FullName; vm.PrefillEmail = u?.Email; vm.PrefillPhone = u?.PhoneNumber;
                vm.MyRegistration = await _db.EventRegistrations.AsNoTracking()
                    .FirstOrDefaultAsync(r => r.ContentId == contentId && r.Status != EventRegistrationStatuses.Cancelled
                        && (r.UserId == userId || (u != null && u.Email != null && r.Email == u.Email)));
            }

            (vm.RegistrationOpen, vm.RegistrationClosedReasonKey) = RegistrationState(info, vm.RegisteredCount);

            var booths = await _db.EventBoothTypes.AsNoTracking().Where(b => b.ContentId == contentId && b.IsActive)
                .OrderBy(b => b.SortOrder).ThenBy(b => b.Id).ToListAsync();
            var used = await _db.EventBoothBookings.Where(b => b.ContentId == contentId &&
                    (b.Status == EventBoothBookingStatuses.Pending || b.Status == EventBoothBookingStatuses.Approved))
                .GroupBy(b => b.BoothTypeId).Select(g => new { g.Key, N = g.Sum(x => x.Quantity) }).ToDictionaryAsync(x => x.Key, x => x.N);
            vm.Booths = booths.Select(b => new EventBoothAvailability { Booth = b, Remaining = Math.Max(0, b.Quantity - (used.TryGetValue(b.Id, out var n) ? n : 0)) }).ToList();
            return vm;
        }

        private static (bool Open, string? ReasonKey) RegistrationState(EventInfo info, int registered)
        {
            var now = DateTime.Now;
            if (!info.RegistrationEnabled) return (false, "event.reg.closed");
            if ((info.EndAt ?? info.StartAt) < now) return (false, "event.reg.ended");
            if (info.RegistrationDeadline.HasValue && info.RegistrationDeadline.Value < now) return (false, "event.reg.deadlinePassed");
            if (info.MaxAttendees.HasValue && registered >= info.MaxAttendees.Value) return (false, "event.reg.full");
            return (true, null);
        }

        public async Task RecordViewAsync(long contentId, int? userId, string? visitorKey)
        {
            if (!await _db.EventInfos.AnyAsync(e => e.ContentId == contentId)) return;
            _db.EventInteractions.Add(new EventInteraction
            {
                ContentId = contentId, InteractionType = EventInteractionTypes.View, UserId = userId, VisitorKey = visitorKey, Created = DateTime.Now
            });
            await _db.SaveChangesAsync();
        }

        public async Task RecordInteractionAsync(long contentId, byte type, int? userId, string? visitorKey)
        {
            if (type != EventInteractionTypes.Click && type != EventInteractionTypes.Share) return;
            if (!await _db.EventInfos.AnyAsync(e => e.ContentId == contentId)) return;
            _db.EventInteractions.Add(new EventInteraction
            {
                ContentId = contentId, InteractionType = type, UserId = userId, VisitorKey = visitorKey, Created = DateTime.Now
            });
            await _db.SaveChangesAsync();
        }

        public async Task<(bool Ok, string? ErrorKey, string? TicketUrl)> RegisterAsync(long contentId, EventRegisterInput input, int? userId, string? ip, bool isEn)
        {
            var info = await _db.EventInfos.AsNoTracking().FirstOrDefaultAsync(e => e.ContentId == contentId);
            if (info == null) return (false, "event.err.notFound", null);

            var name = (input.FullName ?? "").Trim();
            var email = (input.Email ?? "").Trim().ToLowerInvariant();
            var phone = (input.Phone ?? "").Trim();
            if (name.Length == 0 || name.Length > 200) return (false, "event.err.name", null);
            if (!IsEmail(email)) return (false, "event.err.email", null);
            if (phone.Length > 30) return (false, "event.err.phone", null);

            var registered = await _db.EventRegistrations.CountAsync(r => r.ContentId == contentId && r.Status != EventRegistrationStatuses.Cancelled);
            var (open, reason) = RegistrationState(info, registered);
            if (!open) return (false, reason, null);

            if (!userId.HasValue && !string.IsNullOrEmpty(ip))
            {
                var key = "event-reg-ip:" + ip;
                var n = _cache.GetOrCreate(key, e => { e.AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(10); return 0; });
                if (n >= MaxRegistrationsPerIpPer10Min) return (false, "event.err.tooMany", null);
                _cache.Set(key, n + 1, TimeSpan.FromMinutes(10));
            }

            var existing = await _db.EventRegistrations.FirstOrDefaultAsync(r => r.ContentId == contentId && r.Email == email);
            if (existing != null && existing.Status != EventRegistrationStatuses.Cancelled)
                return (false, "event.err.alreadyRegistered", TicketUrl(existing.CheckInCode, isEn));

            var reg = existing ?? new EventRegistration { ContentId = contentId, Email = email, CheckInCode = NewCode() };
            reg.UserId = userId;
            reg.FullName = name;
            reg.Phone = string.IsNullOrEmpty(phone) ? null : phone;
            reg.Organization = Trim(input.Organization, 300);
            reg.Note = Trim(input.Note, 1000);
            reg.Status = EventRegistrationStatuses.Registered;
            reg.Created = DateTime.Now;
            reg.IpAddress = ip;
            if (existing == null) _db.EventRegistrations.Add(reg);
            await _db.SaveChangesAsync();

            // Email xác nhận kèm link vé (QR) — qua hàng đợi thông báo
            var title = await _db.Contents.AsNoTracking().Where(c => c.Id == contentId).Select(c => c.Title).FirstOrDefaultAsync() ?? "";
            var ticketUrl = TicketUrl(reg.CheckInCode, isEn);
            var safeTitle = WebUtility.HtmlEncode(title);
            var body = isEn
                ? $"<p>Dear {WebUtility.HtmlEncode(name)},</p><p>Your registration for <b>{safeTitle}</b> is confirmed.</p>" +
                  $"<p>Time: {info.StartAt:dd/MM/yyyy HH:mm}{(string.IsNullOrEmpty(info.Location) ? "" : "<br/>Venue: " + WebUtility.HtmlEncode(info.Location))}</p>" +
                  $"<p>Check-in code: <b>{reg.CheckInCode}</b><br/><a href=\"{WebUtility.HtmlEncode(ticketUrl)}\">Open your ticket (QR code)</a> and show it at the entrance.</p>"
                : $"<p>Kính gửi {WebUtility.HtmlEncode(name)},</p><p>Anh/chị đã đăng ký tham gia <b>{safeTitle}</b> thành công.</p>" +
                  $"<p>Thời gian: {info.StartAt:dd/MM/yyyy HH:mm}{(string.IsNullOrEmpty(info.Location) ? "" : "<br/>Địa điểm: " + WebUtility.HtmlEncode(info.Location))}</p>" +
                  $"<p>Mã điểm danh: <b>{reg.CheckInCode}</b><br/><a href=\"{WebUtility.HtmlEncode(ticketUrl)}\">Mở vé (mã QR)</a> và xuất trình khi đến sự kiện.</p>";
            _db.Notifications.Add(new Notification
            {
                UserId = userId?.ToString(), Target = email, Title = (isEn ? "Registration confirmed: " : "Xác nhận đăng ký: ") + title,
                Content = body, Channel = (int)NotificationChannel.Email, Status = (int)NotificationStatus.Pending,
                CreatedDate = DateTime.UtcNow, Url = ticketUrl
            });
            await _db.SaveChangesAsync();
            return (true, null, ticketUrl);
        }

        public async Task<(bool Active, int Count)> ToggleInterestAsync(long contentId, int? userId, string visitorKey)
        {
            var row = await _db.EventInteractions.FirstOrDefaultAsync(i => i.ContentId == contentId && i.InteractionType == EventInteractionTypes.Interest &&
                ((userId != null && i.UserId == userId) || (userId == null && i.VisitorKey == visitorKey)));
            if (row == null)
                _db.EventInteractions.Add(new EventInteraction { ContentId = contentId, InteractionType = EventInteractionTypes.Interest, UserId = userId, VisitorKey = visitorKey });
            else
                _db.EventInteractions.Remove(row);
            await _db.SaveChangesAsync();
            var count = await _db.EventInteractions.CountAsync(i => i.ContentId == contentId && i.InteractionType == EventInteractionTypes.Interest);
            return (row == null, count);
        }

        public async Task<(bool Ok, string? ErrorKey)> SubscribeReminderAsync(long contentId, int? userId, string visitorKey, string? email, string? phone)
        {
            var info = await _db.EventInfos.AsNoTracking().FirstOrDefaultAsync(e => e.ContentId == contentId);
            if (info == null) return (false, "event.err.notFound");
            if (info.StartAt < DateTime.Now) return (false, "event.reg.ended");

            email = (email ?? "").Trim().ToLowerInvariant();
            phone = (phone ?? "").Trim();
            if (userId.HasValue && email.Length == 0)
            {
                var u = await _db.Users.AsNoTracking().Where(x => x.Id == userId).Select(x => new { x.Email, x.PhoneNumber }).FirstOrDefaultAsync();
                email = (u?.Email ?? "").ToLowerInvariant();
                if (phone.Length == 0) phone = u?.PhoneNumber ?? "";
            }
            if (!IsEmail(email)) return (false, "event.err.email");

            var exists = await _db.EventInteractions.AnyAsync(i => i.ContentId == contentId && i.InteractionType == EventInteractionTypes.Remind &&
                ((userId != null && i.UserId == userId) || i.Email == email));
            if (exists) return (true, null);

            _db.EventInteractions.Add(new EventInteraction
            {
                ContentId = contentId, InteractionType = EventInteractionTypes.Remind, UserId = userId, VisitorKey = visitorKey,
                Email = email, Phone = phone.Length == 0 ? null : (phone.Length > 30 ? phone[..30] : phone)
            });
            await _db.SaveChangesAsync();
            return (true, null);
        }

        public async Task<(bool Ok, string? ErrorKey)> BookBoothAsync(long contentId, EventBoothInput input, int? userId)
        {
            var booth = await _db.EventBoothTypes.AsNoTracking().FirstOrDefaultAsync(b => b.Id == input.BoothTypeId && b.ContentId == contentId && b.IsActive);
            if (booth == null) return (false, "event.err.notFound");
            var info = await _db.EventInfos.AsNoTracking().FirstOrDefaultAsync(e => e.ContentId == contentId);
            if (info == null || info.StartAt < DateTime.Now) return (false, "event.reg.ended");

            var org = (input.Organization ?? "").Trim();
            var contact = (input.ContactName ?? "").Trim();
            var email = (input.Email ?? "").Trim().ToLowerInvariant();
            var phone = (input.Phone ?? "").Trim();
            if (org.Length == 0 || org.Length > 300 || contact.Length == 0 || contact.Length > 200) return (false, "event.err.name");
            if (!IsEmail(email)) return (false, "event.err.email");
            if (phone.Length == 0 || phone.Length > 30) return (false, "event.err.phone");
            if (input.Quantity < 1) input.Quantity = 1;

            var used = await _db.EventBoothBookings.Where(b => b.BoothTypeId == booth.Id &&
                    (b.Status == EventBoothBookingStatuses.Pending || b.Status == EventBoothBookingStatuses.Approved)).SumAsync(b => (int?)b.Quantity) ?? 0;
            if (used + input.Quantity > booth.Quantity) return (false, "event.err.boothFull");

            _db.EventBoothBookings.Add(new EventBoothBooking
            {
                BoothTypeId = booth.Id, ContentId = contentId, UserId = userId, Organization = org, ContactName = contact,
                Email = email, Phone = phone, Quantity = input.Quantity, Note = Trim(input.Note, 1000),
                Status = EventBoothBookingStatuses.Pending, Created = DateTime.Now
            });
            await _db.SaveChangesAsync();
            return (true, null);
        }

        public async Task<EventTicketVm?> GetTicketAsync(string code)
        {
            if (string.IsNullOrWhiteSpace(code) || code.Length > 32) return null;
            var reg = await _db.EventRegistrations.AsNoTracking().FirstOrDefaultAsync(r => r.CheckInCode == code);
            if (reg == null) return null;
            var title = await _db.Contents.AsNoTracking().Where(c => c.Id == reg.ContentId).Select(c => c.Title).FirstOrDefaultAsync() ?? "";
            var info = await _db.EventInfos.AsNoTracking().FirstOrDefaultAsync(e => e.ContentId == reg.ContentId);
            // QR mở trang điểm danh trong CMS — cán bộ quét bằng điện thoại đã đăng nhập CMS
            var checkInUrl = $"{_mainDomain}cms/EventsAdmin/CheckIn?code={code}";
            return new EventTicketVm
            {
                Registration = reg, EventTitle = title, Info = info,
                QrDataUri = QrCodeHelper.ToPngDataUri(checkInUrl), EventUrl = await EventUrlAsync(reg.ContentId)
            };
        }

        public string TicketUrl(string code, bool isEn) => $"{_mainDomain}{(isEn ? "en/event/ticket" : "su-kien/ve")}/{code}";

        public async Task<string> EventUrlAsync(long contentId)
        {
            var c = await _db.Contents.AsNoTracking().Where(x => x.Id == contentId)
                .Select(x => new { x.Id, x.MenuId, x.QueryString }).FirstOrDefaultAsync();
            if (c == null) return _mainDomain;
            return $"{_mainDomain}{(c.MenuId == 44 ? "tin-tuc-su-kien" : c.MenuId?.ToString())}/{c.QueryString}-{c.Id}";
        }

        private static string NewCode() => Convert.ToHexString(RandomNumberGenerator.GetBytes(10)); // 20 ký tự hex

        private static string? Trim(string? s, int max)
        {
            s = s?.Trim();
            if (string.IsNullOrEmpty(s)) return null;
            return s.Length > max ? s[..max] : s;
        }

        private static bool IsEmail(string s)
            => s.Length is > 3 and <= 256 && System.Text.RegularExpressions.Regex.IsMatch(s, @"^[^@\s]+@[^@\s]+\.[^@\s]+$");
    }
}
