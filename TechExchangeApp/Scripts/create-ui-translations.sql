-- ============================================================================
-- i18n UI strings: bảng UiTranslations + UiTranslationHistory + seed từ ui.json.
-- Idempotent: chạy lại an toàn (tạo bảng nếu chưa có; seed chỉ chèn key chưa tồn tại).
-- ============================================================================
SET NOCOUNT ON;

IF OBJECT_ID('dbo.UiTranslations','U') IS NULL
BEGIN
    CREATE TABLE dbo.UiTranslations
    (
        Id         INT IDENTITY(1,1) NOT NULL,
        [Key]      NVARCHAR(250) NOT NULL,
        Vi         NVARCHAR(MAX) NULL,
        En         NVARCHAR(MAX) NULL,
        Note       NVARCHAR(500) NULL,
        AllowHtml  BIT NOT NULL CONSTRAINT DF_UiTranslations_AllowHtml DEFAULT (0),
        IsActive   BIT NOT NULL CONSTRAINT DF_UiTranslations_IsActive DEFAULT (1),
        RowVersion ROWVERSION NOT NULL,
        Created    DATETIME2 NOT NULL CONSTRAINT DF_UiTranslations_Created DEFAULT (SYSDATETIME()),
        Creator    NVARCHAR(255) NULL,
        Modified   DATETIME2 NULL,
        Modifier   NVARCHAR(255) NULL,
        CONSTRAINT PK_UiTranslations PRIMARY KEY (Id),
        CONSTRAINT UQ_UiTranslations_Key UNIQUE ([Key])
    );
END
GO

IF OBJECT_ID('dbo.UiTranslationHistory','U') IS NULL
BEGIN
    CREATE TABLE dbo.UiTranslationHistory
    (
        Id            INT IDENTITY(1,1) NOT NULL,
        TranslationId INT NOT NULL,
        Vi            NVARCHAR(MAX) NULL,
        En            NVARCHAR(MAX) NULL,
        AllowHtml     BIT NOT NULL,
        ChangedBy     NVARCHAR(255) NULL,
        ChangedAt     DATETIME2 NOT NULL CONSTRAINT DF_UiTranslationHistory_ChangedAt DEFAULT (SYSDATETIME()),
        CONSTRAINT PK_UiTranslationHistory PRIMARY KEY (Id),
        CONSTRAINT FK_UiTranslationHistory_UiTranslations
            FOREIGN KEY (TranslationId) REFERENCES dbo.UiTranslations(Id)
    );
END
GO

-- Seed 44 key từ ui.json (chỉ chèn key chưa có).
IF NOT EXISTS (SELECT 1 FROM dbo.UiTranslations WHERE [Key]=N'common.unknown') INSERT INTO dbo.UiTranslations([Key],Vi,En,Creator) VALUES (N'common.unknown',N'Chưa xác định',N'Unknown','ui-json-seed');
IF NOT EXISTS (SELECT 1 FROM dbo.UiTranslations WHERE [Key]=N'common.consultant') INSERT INTO dbo.UiTranslations([Key],Vi,En,Creator) VALUES (N'common.consultant',N'Tư vấn',N'Consultant','ui-json-seed');
IF NOT EXISTS (SELECT 1 FROM dbo.UiTranslations WHERE [Key]=N'common.supplier') INSERT INTO dbo.UiTranslations([Key],Vi,En,Creator) VALUES (N'common.supplier',N'Nhà cung ứng',N'Supplier','ui-json-seed');
IF NOT EXISTS (SELECT 1 FROM dbo.UiTranslations WHERE [Key]=N'common.buyer') INSERT INTO dbo.UiTranslations([Key],Vi,En,Creator) VALUES (N'common.buyer',N'Bên mua',N'Buyer','ui-json-seed');
IF NOT EXISTS (SELECT 1 FROM dbo.UiTranslations WHERE [Key]=N'common.actions') INSERT INTO dbo.UiTranslations([Key],Vi,En,Creator) VALUES (N'common.actions',N'Thao tác',N'Actions','ui-json-seed');
IF NOT EXISTS (SELECT 1 FROM dbo.UiTranslations WHERE [Key]=N'common.score') INSERT INTO dbo.UiTranslations([Key],Vi,En,Creator) VALUES (N'common.score',N'Điểm',N'Score','ui-json-seed');
IF NOT EXISTS (SELECT 1 FROM dbo.UiTranslations WHERE [Key]=N'proposal.review.consultantComments') INSERT INTO dbo.UiTranslations([Key],Vi,En,Creator) VALUES (N'proposal.review.consultantComments',N'Nhận xét của tư vấn',N'Consultant comments','ui-json-seed');
IF NOT EXISTS (SELECT 1 FROM dbo.UiTranslations WHERE [Key]=N'proposal.review.noComment') INSERT INTO dbo.UiTranslations([Key],Vi,En,Creator) VALUES (N'proposal.review.noComment',N'Không có nhận xét',N'No comments','ui-json-seed');
IF NOT EXISTS (SELECT 1 FROM dbo.UiTranslations WHERE [Key]=N'proposal.review.cardTitle') INSERT INTO dbo.UiTranslations([Key],Vi,En,Creator) VALUES (N'proposal.review.cardTitle',N'Hồ sơ báo giá',N'Proposal','ui-json-seed');
IF NOT EXISTS (SELECT 1 FROM dbo.UiTranslations WHERE [Key]=N'proposal.review.copyCode') INSERT INTO dbo.UiTranslations([Key],Vi,En,Creator) VALUES (N'proposal.review.copyCode',N'Sao chép mã',N'Copy code','ui-json-seed');
IF NOT EXISTS (SELECT 1 FROM dbo.UiTranslations WHERE [Key]=N'proposal.review.downloadTechnical') INSERT INTO dbo.UiTranslations([Key],Vi,En,Creator) VALUES (N'proposal.review.downloadTechnical',N'Tải giải pháp kỹ thuật',N'Download technical proposal','ui-json-seed');
IF NOT EXISTS (SELECT 1 FROM dbo.UiTranslations WHERE [Key]=N'proposal.review.downloadCapability') INSERT INTO dbo.UiTranslations([Key],Vi,En,Creator) VALUES (N'proposal.review.downloadCapability',N'Tải hồ sơ năng lực',N'Download capability profile','ui-json-seed');
IF NOT EXISTS (SELECT 1 FROM dbo.UiTranslations WHERE [Key]=N'proposal.review.yourReviewConsultant') INSERT INTO dbo.UiTranslations([Key],Vi,En,Creator) VALUES (N'proposal.review.yourReviewConsultant',N'ĐÁNH GIÁ CỦA BẠN (Tư vấn)',N'YOUR REVIEW (Consultant)','ui-json-seed');
IF NOT EXISTS (SELECT 1 FROM dbo.UiTranslations WHERE [Key]=N'proposal.review.technicalScore') INSERT INTO dbo.UiTranslations([Key],Vi,En,Creator) VALUES (N'proposal.review.technicalScore',N'Điểm kỹ thuật',N'Technical score','ui-json-seed');
IF NOT EXISTS (SELECT 1 FROM dbo.UiTranslations WHERE [Key]=N'proposal.review.priceScore') INSERT INTO dbo.UiTranslations([Key],Vi,En,Creator) VALUES (N'proposal.review.priceScore',N'Điểm giá',N'Price score','ui-json-seed');
IF NOT EXISTS (SELECT 1 FROM dbo.UiTranslations WHERE [Key]=N'proposal.review.timelineScore') INSERT INTO dbo.UiTranslations([Key],Vi,En,Creator) VALUES (N'proposal.review.timelineScore',N'Điểm tiến độ',N'Timeline score','ui-json-seed');
IF NOT EXISTS (SELECT 1 FROM dbo.UiTranslations WHERE [Key]=N'proposal.review.overallScore') INSERT INTO dbo.UiTranslations([Key],Vi,En,Creator) VALUES (N'proposal.review.overallScore',N'Điểm tổng thể',N'Overall score','ui-json-seed');
IF NOT EXISTS (SELECT 1 FROM dbo.UiTranslations WHERE [Key]=N'proposal.review.commentLabel') INSERT INTO dbo.UiTranslations([Key],Vi,En,Creator) VALUES (N'proposal.review.commentLabel',N'Nhận xét',N'Comment','ui-json-seed');
IF NOT EXISTS (SELECT 1 FROM dbo.UiTranslations WHERE [Key]=N'proposal.review.alreadyScored') INSERT INTO dbo.UiTranslations([Key],Vi,En,Creator) VALUES (N'proposal.review.alreadyScored',N'Bạn đã chấm điểm hồ sơ báo giá này',N'You have scored this proposal','ui-json-seed');
IF NOT EXISTS (SELECT 1 FROM dbo.UiTranslations WHERE [Key]=N'contract.status.draft') INSERT INTO dbo.UiTranslations([Key],Vi,En,Creator) VALUES (N'contract.status.draft',N'Bản nháp',N'Draft','ui-json-seed');
IF NOT EXISTS (SELECT 1 FROM dbo.UiTranslations WHERE [Key]=N'contract.status.reviewing') INSERT INTO dbo.UiTranslations([Key],Vi,En,Creator) VALUES (N'contract.status.reviewing',N'Đang rà soát',N'Under review','ui-json-seed');
IF NOT EXISTS (SELECT 1 FROM dbo.UiTranslations WHERE [Key]=N'contract.status.revised') INSERT INTO dbo.UiTranslations([Key],Vi,En,Creator) VALUES (N'contract.status.revised',N'Đã sửa',N'Revised','ui-json-seed');
IF NOT EXISTS (SELECT 1 FROM dbo.UiTranslations WHERE [Key]=N'contract.status.readyToSign') INSERT INTO dbo.UiTranslations([Key],Vi,En,Creator) VALUES (N'contract.status.readyToSign',N'Sẵn sàng ký',N'Ready to sign','ui-json-seed');
IF NOT EXISTS (SELECT 1 FROM dbo.UiTranslations WHERE [Key]=N'contract.status.signing') INSERT INTO dbo.UiTranslations([Key],Vi,En,Creator) VALUES (N'contract.status.signing',N'Đang ký',N'Signing','ui-json-seed');
IF NOT EXISTS (SELECT 1 FROM dbo.UiTranslations WHERE [Key]=N'contract.status.signed') INSERT INTO dbo.UiTranslations([Key],Vi,En,Creator) VALUES (N'contract.status.signed',N'Đã ký đầy đủ',N'Fully signed','ui-json-seed');
IF NOT EXISTS (SELECT 1 FROM dbo.UiTranslations WHERE [Key]=N'contract.status.archived') INSERT INTO dbo.UiTranslations([Key],Vi,En,Creator) VALUES (N'contract.status.archived',N'Lưu trữ',N'Archived','ui-json-seed');
IF NOT EXISTS (SELECT 1 FROM dbo.UiTranslations WHERE [Key]=N'contract.action.finalizeReadyToSign') INSERT INTO dbo.UiTranslations([Key],Vi,En,Creator) VALUES (N'contract.action.finalizeReadyToSign',N'Chốt hợp đồng - sẵn sàng ký',N'Finalize contract - ready to sign','ui-json-seed');
IF NOT EXISTS (SELECT 1 FROM dbo.UiTranslations WHERE [Key]=N'contract.message.mustCompleteStep6') INSERT INTO dbo.UiTranslations([Key],Vi,En,Creator) VALUES (N'contract.message.mustCompleteStep6',N'Vui lòng hoàn thành Bước 6 (tất cả 3 bên đã duyệt và chốt sẵn sàng ký).',N'Please complete Step 6 (all 3 parties approved and finalized as ready to sign).','ui-json-seed');
IF NOT EXISTS (SELECT 1 FROM dbo.UiTranslations WHERE [Key]=N'contract.message.notReadyToSign') INSERT INTO dbo.UiTranslations([Key],Vi,En,Creator) VALUES (N'contract.message.notReadyToSign',N'Hợp đồng chưa ở trạng thái sẵn sàng ký.',N'The contract is not ready to sign.','ui-json-seed');
IF NOT EXISTS (SELECT 1 FROM dbo.UiTranslations WHERE [Key]=N'legalReview.comment.empty') INSERT INTO dbo.UiTranslations([Key],Vi,En,Creator) VALUES (N'legalReview.comment.empty',N'Nội dung nhận xét không được để trống.',N'Comment content cannot be empty.','ui-json-seed');
IF NOT EXISTS (SELECT 1 FROM dbo.UiTranslations WHERE [Key]=N'legalReview.comment.added') INSERT INTO dbo.UiTranslations([Key],Vi,En,Creator) VALUES (N'legalReview.comment.added',N'Nhận xét đã được thêm.',N'Comment added.','ui-json-seed');
IF NOT EXISTS (SELECT 1 FROM dbo.UiTranslations WHERE [Key]=N'legalReview.comment.resolved') INSERT INTO dbo.UiTranslations([Key],Vi,En,Creator) VALUES (N'legalReview.comment.resolved',N'Nhận xét đã được đánh dấu đã xử lý.',N'Comment marked as resolved.','ui-json-seed');
IF NOT EXISTS (SELECT 1 FROM dbo.UiTranslations WHERE [Key]=N'legalReview.comment.notFound') INSERT INTO dbo.UiTranslations([Key],Vi,En,Creator) VALUES (N'legalReview.comment.notFound',N'Không tìm thấy nhận xét.',N'Comment not found.','ui-json-seed');
IF NOT EXISTS (SELECT 1 FROM dbo.UiTranslations WHERE [Key]=N'common.home') INSERT INTO dbo.UiTranslations([Key],Vi,En,Creator) VALUES (N'common.home',N'Trang chủ',N'Home','ui-json-seed');
IF NOT EXISTS (SELECT 1 FROM dbo.UiTranslations WHERE [Key]=N'news.breadcrumb') INSERT INTO dbo.UiTranslations([Key],Vi,En,Creator) VALUES (N'news.breadcrumb',N'Tin tức',N'News','ui-json-seed');
IF NOT EXISTS (SELECT 1 FROM dbo.UiTranslations WHERE [Key]=N'news.title') INSERT INTO dbo.UiTranslations([Key],Vi,En,Creator) VALUES (N'news.title',N'Tin tức - Sự kiện',N'News & Events','ui-json-seed');
IF NOT EXISTS (SELECT 1 FROM dbo.UiTranslations WHERE [Key]=N'news.desc') INSERT INTO dbo.UiTranslations([Key],Vi,En,Creator) VALUES (N'news.desc',N'Cập nhật các hoạt động, chương trình, hội thảo và thông tin khoa học công nghệ mới nhất.',N'Latest activities, programs, seminars and science & technology updates.','ui-json-seed');
IF NOT EXISTS (SELECT 1 FROM dbo.UiTranslations WHERE [Key]=N'news.search') INSERT INTO dbo.UiTranslations([Key],Vi,En,Creator) VALUES (N'news.search',N'Tìm kiếm tin tức',N'Search news','ui-json-seed');
IF NOT EXISTS (SELECT 1 FROM dbo.UiTranslations WHERE [Key]=N'news.searchPlaceholder') INSERT INTO dbo.UiTranslations([Key],Vi,En,Creator) VALUES (N'news.searchPlaceholder',N'Nhập từ khóa tìm kiếm...',N'Enter keywords...','ui-json-seed');
IF NOT EXISTS (SELECT 1 FROM dbo.UiTranslations WHERE [Key]=N'common.search') INSERT INTO dbo.UiTranslations([Key],Vi,En,Creator) VALUES (N'common.search',N'Tìm kiếm',N'Search','ui-json-seed');
IF NOT EXISTS (SELECT 1 FROM dbo.UiTranslations WHERE [Key]=N'news.categories') INSERT INTO dbo.UiTranslations([Key],Vi,En,Creator) VALUES (N'news.categories',N'Danh mục tin',N'News categories','ui-json-seed');
IF NOT EXISTS (SELECT 1 FROM dbo.UiTranslations WHERE [Key]=N'news.latest') INSERT INTO dbo.UiTranslations([Key],Vi,En,Creator) VALUES (N'news.latest',N'Tin mới nhất',N'Latest news','ui-json-seed');
IF NOT EXISTS (SELECT 1 FROM dbo.UiTranslations WHERE [Key]=N'news.archive') INSERT INTO dbo.UiTranslations([Key],Vi,En,Creator) VALUES (N'news.archive',N'Lưu trữ',N'Archive','ui-json-seed');
IF NOT EXISTS (SELECT 1 FROM dbo.UiTranslations WHERE [Key]=N'news.year') INSERT INTO dbo.UiTranslations([Key],Vi,En,Creator) VALUES (N'news.year',N'Năm',N'Year','ui-json-seed');

SELECT COUNT(*) AS TotalKeys FROM dbo.UiTranslations;