using System.Security.Claims;
using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Microsoft.EntityFrameworkCore.Infrastructure;
using TechExchangeApp.Entities;

namespace TechExchangeApp.Data
{
    /// <summary>
    /// Tự ghi lịch sử phiên bản SanPhamCNTB mỗi lần lưu (HSMT A.2.7, A.3.2, A.8.3).
    /// So sánh với phiên bản đã lưu gần nhất (không dựa vào OriginalValues của EF vì nhiều
    /// chỗ dùng Update() trên entity rời, khi đó EF coi mọi cột là "đã sửa").
    /// Sản phẩm sửa lần đầu sau khi bật tính năng: lưu thêm phiên bản 0 = trạng thái cũ trong CSDL.
    /// </summary>
    public partial class AppDbContext
    {
        /// <summary>Cột không tính là thay đổi nội dung (bộ đếm, dấu thời gian, cờ dịch).</summary>
        private static readonly HashSet<string> VersionIgnoredProps = new(StringComparer.Ordinal)
        {
            nameof(SanPhamCNTB.Viewed), nameof(SanPhamCNTB.Rating), nameof(SanPhamCNTB.Modified),
            nameof(SanPhamCNTB.Modifier), nameof(SanPhamCNTB.EnStale), nameof(SanPhamCNTB.SourceHash),
            nameof(SanPhamCNTB.DaBan)
        };

        private bool _writingVersions;

        public override int SaveChanges(bool acceptAllChangesOnSuccess)
            => SaveChangesWithVersionsAsync(acceptAllChangesOnSuccess, CancellationToken.None, sync: true).GetAwaiter().GetResult();

        public override Task<int> SaveChangesAsync(bool acceptAllChangesOnSuccess, CancellationToken cancellationToken = default)
            => SaveChangesWithVersionsAsync(acceptAllChangesOnSuccess, cancellationToken, sync: false);

        private async Task<int> SaveChangesWithVersionsAsync(bool accept, CancellationToken ct, bool sync)
        {
            if (_writingVersions)
                return sync ? base.SaveChanges(accept) : await base.SaveChangesAsync(accept, ct);

            List<(EntityEntry<SanPhamCNTB> Entry, bool IsNew, Dictionary<string, object?>? DbValues)> pending = new();
            try
            {
                foreach (var e in ChangeTracker.Entries<SanPhamCNTB>())
                {
                    if (e.State == EntityState.Added)
                        pending.Add((e, true, null));
                    else if (e.State == EntityState.Modified
                             && e.Properties.Any(p => p.IsModified && !VersionIgnoredProps.Contains(p.Metadata.Name)))
                        pending.Add((e, false, null));
                }

                // Sản phẩm chưa có phiên bản nào: chụp giá trị đang nằm trong CSDL làm phiên bản 0
                for (int i = 0; i < pending.Count; i++)
                {
                    var (entry, isNew, _) = pending[i];
                    if (isNew) continue;
                    var id = entry.Entity.ID;
                    var hasVersion = sync
                        ? SanPhamCNTBVersions.Any(v => v.SanPhamId == id)
                        : await SanPhamCNTBVersions.AnyAsync(v => v.SanPhamId == id, ct);
                    if (hasVersion) continue;
                    var dbValues = sync ? entry.GetDatabaseValues() : await entry.GetDatabaseValuesAsync(ct);
                    if (dbValues != null) pending[i] = (entry, false, ToSnapshot(dbValues));
                }
            }
            catch
            {
                // Ghi lịch sử không được làm hỏng thao tác lưu chính
                pending.Clear();
            }

            var result = sync ? base.SaveChanges(accept) : await base.SaveChangesAsync(accept, ct);
            if (pending.Count == 0) return result;

            try
            {
                _writingVersions = true;
                var (userName, userId) = CurrentUser();
                foreach (var (entry, isNew, dbValues) in pending)
                {
                    var id = entry.Entity.ID;
                    var current = ToSnapshot(entry.CurrentValues);
                    var last = await SanPhamCNTBVersions.Where(v => v.SanPhamId == id)
                        .OrderByDescending(v => v.VersionNo).FirstOrDefaultAsync(ct);
                    var nextNo = (last?.VersionNo ?? -1) + 1;

                    Dictionary<string, object?>? previous = null;
                    if (last != null)
                        previous = JsonSerializer.Deserialize<Dictionary<string, object?>>(last.SnapshotJson);
                    else if (dbValues != null)
                    {
                        SanPhamCNTBVersions.Add(new SanPhamCNTBVersion
                        {
                            SanPhamId = id, VersionNo = 0, Action = ProductVersionActions.Baseline,
                            SnapshotJson = JsonSerializer.Serialize(dbValues), Created = DateTime.Now
                        });
                        previous = dbValues;
                        nextNo = 1;
                    }

                    var changed = isNew || previous == null ? null : Diff(previous, current);
                    if (!isNew && previous != null && changed!.Count == 0) continue;

                    SanPhamCNTBVersions.Add(new SanPhamCNTBVersion
                    {
                        SanPhamId = id,
                        VersionNo = nextNo,
                        Action = isNew ? ProductVersionActions.Create : ProductVersionActions.Update,
                        SnapshotJson = JsonSerializer.Serialize(current),
                        ChangedFields = changed == null ? null : string.Join(",", changed),
                        ChangedBy = userName ?? entry.Entity.Modifier ?? entry.Entity.Creator,
                        ChangedByUserId = userId,
                        Created = DateTime.Now
                    });
                }
                if (sync) base.SaveChanges(); else await base.SaveChangesAsync(ct);
            }
            catch (Exception ex)
            {
                // Không chặn thao tác chính; bỏ các bản ghi phiên bản lỗi khỏi bộ theo dõi
                foreach (var v in ChangeTracker.Entries<SanPhamCNTBVersion>().Where(x => x.State == EntityState.Added).ToList())
                    v.State = EntityState.Detached;
                System.Diagnostics.Trace.TraceWarning("[ProductVersion] Lỗi ghi lịch sử: " + ex.Message);
            }
            finally
            {
                _writingVersions = false;
            }
            return result;
        }

        private static Dictionary<string, object?> ToSnapshot(PropertyValues values)
        {
            var dict = new Dictionary<string, object?>(StringComparer.Ordinal);
            foreach (var p in values.Properties)
            {
                if (VersionIgnoredProps.Contains(p.Name)) continue;
                dict[p.Name] = values[p];
            }
            return dict;
        }

        private static List<string> Diff(Dictionary<string, object?> before, Dictionary<string, object?> after)
        {
            var changed = new List<string>();
            foreach (var (key, value) in after)
            {
                before.TryGetValue(key, out var old);
                if (Normalize(old) != Normalize(value)) changed.Add(key);
            }
            return changed;
        }

        /// <summary>So sánh theo chuỗi JSON để giá trị đọc lại từ JSON (JsonElement) khớp giá trị CLR.</summary>
        private static string Normalize(object? v)
        {
            if (v == null) return "";
            var je = v is JsonElement e ? e : JsonSerializer.SerializeToElement(v);
            return je.ValueKind switch
            {
                JsonValueKind.Null or JsonValueKind.Undefined => "",
                JsonValueKind.String => je.GetString() ?? "",
                _ => je.GetRawText()
            };
        }

        private (string? Name, int? Id) CurrentUser()
        {
            try
            {
                var sp = this.GetService<IDbContextOptions>().FindExtension<CoreOptionsExtension>()?.ApplicationServiceProvider;
                var user = sp?.GetService<IHttpContextAccessor>()?.HttpContext?.User;
                if (user?.Identity?.IsAuthenticated != true) return (null, null);
                int? id = int.TryParse(user.FindFirst(ClaimTypes.NameIdentifier)?.Value, out var n) ? n : null;
                return (user.Identity!.Name, id);
            }
            catch
            {
                return (null, null);
            }
        }
    }
}
