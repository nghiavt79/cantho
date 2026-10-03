# Đặc tả cơ sở dữ liệu – phần bổ sung theo HSMT (Đợt 1–5)

Ngày lập: 03/10/2026. Sinh tự động từ CSDL `TechExchangeNew`.

**Phạm vi:** các bảng và cột thêm mới khi bổ sung chức năng theo Chương V. Các bảng có sẵn của hệ thống không nằm trong tài liệu này.

**Giao diện tích hợp (API):** tài liệu tự sinh dạng OpenAPI tại `/swagger` (cần tài khoản quản trị), bản JSON tại `/swagger/v1/swagger.json`.

**Script tạo bảng:** `TechExchangeApp/Scripts/dot1-…sql` đến `dot5-…sql` (chạy theo thứ tự, `sqlcmd -f 65001 -I`).

## 1. Cột thêm vào bảng có sẵn

| Bảng | Cột | Mục đích |
|---|---|---|
| `Users` | IsLocked, LockedReason, LockedAt, LockedBy | Khóa tài khoản (A.5.1) — Đợt 1 |
| `Users` | ConsentAt, ConsentVersion | Đồng ý xử lý dữ liệu cá nhân (2.11) — Đợt 5 |
| `SanPhamCNTB` | DisplayMode | Hình thức trưng bày 1/2/3 (A.3.1, B.1.4) — Đợt 1 |
| `ChatConversations` | IsFlagged, FlagReason, FlaggedAt, FlaggedBy | Gắn cờ hội thoại vi phạm (A.5.3) — Đợt 2 |
| `NhaCungUng` | TierId, TierAssignedAt, TierAssignedBy | Hạng nhà cung cấp (A.2.2) — Đợt 4 |

## 2. Bảng mới

### Đợt 1

#### 2.1. `PasswordResetTokens`

Token đặt lại mật khẩu (A.1.1) — chỉ lưu SHA-256, hết hạn 30 phút, dùng 1 lần

| Cột | Kiểu | Cho phép rỗng | Ghi chú |
|---|---|---|---|
| `Id` | int | Không | Khóa chính, Tự tăng |
| `UserId` | int | Không |  |
| `TokenHash` | varchar(64) | Không |  |
| `ExpiresAt` | datetime | Không |  |
| `UsedAt` | datetime | Có |  |
| `Created` | datetime | Không |  |
| `RequestIp` | varchar(64) | Có |  |

#### 2.2. `Faqs`

Câu hỏi thường gặp (A.7.3)

| Cột | Kiểu | Cho phép rỗng | Ghi chú |
|---|---|---|---|
| `Id` | int | Không | Khóa chính, Tự tăng |
| `GroupName` | nvarchar(200) | Không |  |
| `Question` | nvarchar(1000) | Không |  |
| `Answer` | nvarchar(MAX) | Không |  |
| `SortOrder` | int | Không |  |
| `IsActive` | bit | Không |  |
| `LanguageId` | int | Không |  |
| `Created` | datetime | Không |  |
| `Creator` | nvarchar(100) | Có |  |
| `Modified` | datetime | Có |  |
| `Modifier` | nvarchar(100) | Có |  |

#### 2.3. `ProductFavorites`

Sản phẩm người dùng đánh dấu Quan tâm (B.3.3)

| Cột | Kiểu | Cho phép rỗng | Ghi chú |
|---|---|---|---|
| `Id` | int | Không | Khóa chính, Tự tăng |
| `UserId` | int | Không |  |
| `SanPhamId` | int | Không |  |
| `Created` | datetime | Không |  |

### Đợt 2

#### 2.4. `SanPhamCNTBVersions`

Lịch sử phiên bản nội dung sản phẩm (A.2.7, A.3.2, A.8.3)

| Cột | Kiểu | Cho phép rỗng | Ghi chú |
|---|---|---|---|
| `Id` | int | Không | Khóa chính, Tự tăng |
| `SanPhamId` | int | Không |  |
| `VersionNo` | int | Không |  |
| `Action` | tinyint | Không |  |
| `SnapshotJson` | nvarchar(MAX) | Không |  |
| `ChangedFields` | nvarchar(2000) | Có |  |
| `ChangedBy` | nvarchar(256) | Có |  |
| `ChangedByUserId` | int | Có |  |
| `Created` | datetime | Không |  |

#### 2.5. `ProductWarnings`

Cảnh báo sản phẩm trùng lặp / quá hạn (A.8.2)

| Cột | Kiểu | Cho phép rỗng | Ghi chú |
|---|---|---|---|
| `Id` | int | Không | Khóa chính, Tự tăng |
| `SanPhamId` | int | Không |  |
| `WarningType` | tinyint | Không |  |
| `GroupKey` | nvarchar(450) | Có |  |
| `Detail` | nvarchar(1000) | Có |  |
| `DetectedAt` | datetime | Không |  |
| `LastSeenAt` | datetime | Không |  |
| `Status` | tinyint | Không |  |
| `ResolvedAt` | datetime | Có |  |
| `ResolvedBy` | nvarchar(256) | Có |  |
| `Note` | nvarchar(500) | Có |  |

#### 2.6. `UserComments`

Bình luận của người dùng (B.3.5, A.7.1, A.5.4)

| Cột | Kiểu | Cho phép rỗng | Ghi chú |
|---|---|---|---|
| `Id` | int | Không | Khóa chính, Tự tăng |
| `TargetType` | tinyint | Không |  |
| `TargetId` | int | Không |  |
| `UserId` | int | Không |  |
| `ParentId` | int | Có |  |
| `Content` | nvarchar(2000) | Không |  |
| `StatusId` | tinyint | Không |  |
| `Created` | datetime | Không |  |
| `Modified` | datetime | Có |  |
| `ModifiedBy` | nvarchar(256) | Có |  |
| `IpAddress` | varchar(64) | Có |  |

### Đợt 3

#### 2.7. `EventInfos`

Thông tin sự kiện gắn với tin bài (A.4.1, C.6)

| Cột | Kiểu | Cho phép rỗng | Ghi chú |
|---|---|---|---|
| `Id` | int | Không | Khóa chính, Tự tăng |
| `ContentId` | bigint | Không |  |
| `StartAt` | datetime | Không |  |
| `EndAt` | datetime | Có |  |
| `Location` | nvarchar(500) | Có |  |
| `LivestreamUrl` | nvarchar(1000) | Có |  |
| `RegistrationEnabled` | bit | Không |  |
| `RegistrationDeadline` | datetime | Có |  |
| `MaxAttendees` | int | Có |  |
| `ReminderOffsetsMinutes` | varchar(100) | Không |  |
| `ReminderChannels` | tinyint | Không |  |
| `Created` | datetime | Không |  |
| `Modified` | datetime | Có |  |
| `Modifier` | nvarchar(256) | Có |  |

#### 2.8. `EventRegistrations`

Đăng ký tham gia, điểm danh (A.4.1, B.1.6)

| Cột | Kiểu | Cho phép rỗng | Ghi chú |
|---|---|---|---|
| `Id` | int | Không | Khóa chính, Tự tăng |
| `ContentId` | bigint | Không |  |
| `UserId` | int | Có |  |
| `FullName` | nvarchar(200) | Không |  |
| `Email` | nvarchar(256) | Không |  |
| `Phone` | varchar(30) | Có |  |
| `Organization` | nvarchar(300) | Có |  |
| `Note` | nvarchar(1000) | Có |  |
| `Status` | tinyint | Không |  |
| `CheckInCode` | varchar(32) | Không |  |
| `Created` | datetime | Không |  |
| `CheckedInAt` | datetime | Có |  |
| `CheckedInBy` | nvarchar(256) | Có |  |
| `IpAddress` | varchar(64) | Có |  |

#### 2.9. `EventInteractions`

Lượt xem / quan tâm / đăng ký nhắc lịch sự kiện (A.4.4, A.4.5)

| Cột | Kiểu | Cho phép rỗng | Ghi chú |
|---|---|---|---|
| `Id` | bigint | Không | Khóa chính, Tự tăng |
| `ContentId` | bigint | Không |  |
| `InteractionType` | tinyint | Không |  |
| `UserId` | int | Có |  |
| `VisitorKey` | varchar(64) | Có |  |
| `Email` | nvarchar(256) | Có |  |
| `Phone` | varchar(30) | Có |  |
| `Created` | datetime | Không |  |

#### 2.10. `EventReminderLogs`

Nhật ký đã gửi nhắc lịch, chống gửi trùng (A.4.5)

| Cột | Kiểu | Cho phép rỗng | Ghi chú |
|---|---|---|---|
| `Id` | int | Không | Khóa chính, Tự tăng |
| `ContentId` | bigint | Không |  |
| `RecipientKey` | nvarchar(300) | Không |  |
| `OffsetMinutes` | int | Không |  |
| `Created` | datetime | Không |  |

#### 2.11. `EventBoothTypes`

Loại gian hàng của sự kiện (A.4.3)

| Cột | Kiểu | Cho phép rỗng | Ghi chú |
|---|---|---|---|
| `Id` | int | Không | Khóa chính, Tự tăng |
| `ContentId` | bigint | Không |  |
| `Name` | nvarchar(200) | Không |  |
| `AreaM2` | decimal(8,2) | Có |  |
| `Position` | nvarchar(200) | Có |  |
| `Price` | decimal(18,0) | Không |  |
| `Quantity` | int | Không |  |
| `Services` | nvarchar(1000) | Có |  |
| `SortOrder` | int | Không |  |
| `IsActive` | bit | Không |  |

#### 2.12. `EventBoothBookings`

Đăng ký thuê gian hàng sự kiện (A.4.3)

| Cột | Kiểu | Cho phép rỗng | Ghi chú |
|---|---|---|---|
| `Id` | int | Không | Khóa chính, Tự tăng |
| `BoothTypeId` | int | Không |  |
| `ContentId` | bigint | Không |  |
| `UserId` | int | Có |  |
| `Organization` | nvarchar(300) | Không |  |
| `ContactName` | nvarchar(200) | Không |  |
| `Email` | nvarchar(256) | Không |  |
| `Phone` | varchar(30) | Không |  |
| `Quantity` | int | Không |  |
| `Note` | nvarchar(1000) | Có |  |
| `Status` | tinyint | Không |  |
| `Created` | datetime | Không |  |
| `ReviewedBy` | nvarchar(256) | Có |  |
| `ReviewedAt` | datetime | Có |  |
| `ReviewNote` | nvarchar(500) | Có |  |

### Đợt 4

#### 2.13. `SupplierTiers`

Hạng nhà cung cấp và tiêu chí (A.2.2, A.2.4)

| Cột | Kiểu | Cho phép rỗng | Ghi chú |
|---|---|---|---|
| `Id` | int | Không | Khóa chính, Tự tăng |
| `Name` | nvarchar(100) | Không |  |
| `Rank` | int | Không |  |
| `BadgeColor` | varchar(20) | Không |  |
| `MinProducts` | int | Không |  |
| `MinContacts` | int | Không |  |
| `MinViews` | int | Không |  |
| `RequireVerified` | bit | Không |  |
| `Description` | nvarchar(500) | Có |  |
| `IsActive` | bit | Không |  |

#### 2.14. `StorePackages`

Gói gian hàng theo số sản phẩm (A.2.3)

| Cột | Kiểu | Cho phép rỗng | Ghi chú |
|---|---|---|---|
| `Id` | int | Không | Khóa chính, Tự tăng |
| `Name` | nvarchar(200) | Không |  |
| `MaxProducts` | int | Không |  |
| `Price` | decimal(18,0) | Không |  |
| `DurationDays` | int | Không |  |
| `Description` | nvarchar(500) | Có |  |
| `SortOrder` | int | Không |  |
| `IsActive` | bit | Không |  |

#### 2.15. `SupplierSubscriptions`

Gói gian hàng đã cấp cho nhà cung cấp (A.2.3)

| Cột | Kiểu | Cho phép rỗng | Ghi chú |
|---|---|---|---|
| `Id` | int | Không | Khóa chính, Tự tăng |
| `CungUngId` | int | Không |  |
| `PackageId` | int | Không |  |
| `StartDate` | date | Không |  |
| `EndDate` | date | Không |  |
| `MaxProducts` | int | Không |  |
| `Price` | decimal(18,0) | Không |  |
| `Status` | tinyint | Không |  |
| `Note` | nvarchar(500) | Có |  |
| `Created` | datetime | Không |  |
| `CreatedBy` | nvarchar(256) | Có |  |

#### 2.16. `CommissionRates`

Tỷ lệ hoa hồng theo loại giao dịch (A.6.1)

| Cột | Kiểu | Cho phép rỗng | Ghi chú |
|---|---|---|---|
| `Id` | int | Không | Khóa chính, Tự tăng |
| `Code` | varchar(50) | Không |  |
| `Name` | nvarchar(200) | Không |  |
| `RatePercent` | decimal(5,2) | Không |  |
| `IsActive` | bit | Không |  |

#### 2.17. `FinanceEntries`

Sổ thu chi (A.6.1)

| Cột | Kiểu | Cho phép rỗng | Ghi chú |
|---|---|---|---|
| `Id` | int | Không | Khóa chính, Tự tăng |
| `EntryDate` | date | Không |  |
| `Direction` | tinyint | Không |  |
| `Category` | varchar(50) | Không |  |
| `Amount` | decimal(18,0) | Không |  |
| `CungUngId` | int | Có |  |
| `RefType` | varchar(50) | Có |  |
| `RefId` | int | Có |  |
| `BaseAmount` | decimal(18,0) | Có |  |
| `RatePercent` | decimal(5,2) | Có |  |
| `Description` | nvarchar(500) | Có |  |
| `Created` | datetime | Không |  |
| `CreatedBy` | nvarchar(256) | Có |  |

#### 2.18. `Showrooms`

Phòng trưng bày (A.3.3)

| Cột | Kiểu | Cho phép rỗng | Ghi chú |
|---|---|---|---|
| `Id` | int | Không | Khóa chính, Tự tăng |
| `Name` | nvarchar(200) | Không |  |
| `Location` | nvarchar(300) | Có |  |
| `Description` | nvarchar(500) | Có |  |
| `SortOrder` | int | Không |  |
| `IsActive` | bit | Không |  |

#### 2.19. `ShowroomSlots`

Vị trí trưng bày (A.3.4)

| Cột | Kiểu | Cho phép rỗng | Ghi chú |
|---|---|---|---|
| `Id` | int | Không | Khóa chính, Tự tăng |
| `ShowroomId` | int | Không |  |
| `Code` | nvarchar(50) | Không |  |
| `Description` | nvarchar(300) | Có |  |
| `SanPhamId` | int | Có |  |
| `AssignedAt` | datetime | Có |  |
| `AssignedBy` | nvarchar(256) | Có |  |
| `IsActive` | bit | Không |  |

#### 2.20. `DynamicForms`

Biểu mẫu động (A.7.2, B.1.6)

| Cột | Kiểu | Cho phép rỗng | Ghi chú |
|---|---|---|---|
| `Id` | int | Không | Khóa chính, Tự tăng |
| `Title` | nvarchar(300) | Không |  |
| `Slug` | varchar(200) | Không |  |
| `Description` | nvarchar(1000) | Có |  |
| `FieldsJson` | nvarchar(MAX) | Không |  |
| `SubmitText` | nvarchar(100) | Có |  |
| `SuccessMessage` | nvarchar(500) | Có |  |
| `RedirectUrl` | nvarchar(500) | Có |  |
| `NotifyEmails` | nvarchar(500) | Có |  |
| `SendConfirmation` | bit | Không |  |
| `LanguageId` | int | Không |  |
| `IsActive` | bit | Không |  |
| `Created` | datetime | Không |  |
| `Modified` | datetime | Có |  |
| `Modifier` | nvarchar(256) | Có |  |

#### 2.21. `DynamicFormSubmissions`

Dữ liệu gửi qua biểu mẫu động

| Cột | Kiểu | Cho phép rỗng | Ghi chú |
|---|---|---|---|
| `Id` | int | Không | Khóa chính, Tự tăng |
| `FormId` | int | Không |  |
| `DataJson` | nvarchar(MAX) | Không |  |
| `Email` | nvarchar(256) | Có |  |
| `UserId` | int | Có |  |
| `IpAddress` | varchar(64) | Có |  |
| `Created` | datetime | Không |  |
