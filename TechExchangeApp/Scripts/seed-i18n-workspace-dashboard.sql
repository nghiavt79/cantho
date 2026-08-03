-- Seed i18n đợt 1 cho workspace: Dashboard, MyProjects, _AuthTopbar, _Layout (dropdown thông báo).
-- LƯU Ý: chạy bằng  sqlcmd ... -f 65001 -i <file>  (file UTF-8 no-BOM, thiếu cờ này dữ liệu vào DB bị mojibake).
-- Giá trị cột Vi đặt ĐÚNG BẰNG chuỗi cứng đang hiển thị => bản tiếng Việt không đổi một ký tự nào.
SET NOCOUNT ON;
DECLARE @s TABLE(k NVARCHAR(250), vi NVARCHAR(700), en NVARCHAR(700));
INSERT INTO @s VALUES
 -- Tên 6 bước HIỂN THỊ trên Dashboard (bước 6 "Kiểm tra pháp lý" bị gộp vào bước 5,
 -- các bước sau dồn số lại — xem DashboardService.BuildDisplaySteps).
 (N'dash.step.1',            N'Yêu cầu chuyển giao công nghệ',                    N'Technology transfer request'),
 (N'dash.step.2',            N'Thỏa thuận bảo mật (NDA)',                         N'Non-disclosure agreement (NDA)'),
 (N'dash.step.3',            N'Yêu cầu báo giá (RFQ)',                            N'Request for quotation (RFQ)'),
 (N'dash.step.4',            N'Nộp hồ sơ đề xuất',                                N'Proposal submission'),
 (N'dash.step.5',            N'Đàm phán thương mại / Kiểm tra pháp lý hợp đồng',  N'Commercial negotiation / Contract legal review'),
 (N'dash.step.6',            N'Ký hợp đồng',                                      N'Contract signing'),
 -- Dashboard
 (N'dash.greeting',          N'Xin chào,',                                        N'Hello,'),
 (N'dash.stat.total',        N'Tổng dự án',                                       N'Total projects'),
 (N'dash.stat.inProgress',   N'Đang xử lý',                                       N'In progress'),
 (N'dash.stat.waitingForMe', N'Chờ tôi xử lý',                                    N'Waiting for me'),
 (N'dash.stat.completed',    N'Đã hoàn thành',                                    N'Completed'),
 (N'dash.col.code',          N'Mã dự án',                                         N'Project code'),
 (N'dash.col.name',          N'Tên dự án',                                        N'Project name'),
 (N'dash.col.role',          N'Vai trò',                                          N'Role'),
 (N'dash.col.currentStep',   N'Bước hiện tại',                                    N'Current step'),
 (N'dash.col.status',        N'Trạng thái',                                       N'Status'),
 (N'dash.col.progress',      N'Tiến độ',                                          N'Progress'),
 (N'dash.col.workflow',      N'Workflow',                                         N'Workflow'),
 (N'dash.stepPrefix',        N'Bước',                                             N'Step'),
 (N'dash.stepsSuffix',       N'bước',                                             N'steps'),
 (N'dash.status.completed',  N'Hoàn thành',                                       N'Completed'),
 (N'dash.status.inProgress', N'Đang xử lý',                                       N'In progress'),
 (N'dash.status.notStarted', N'Chưa bắt đầu',                                     N'Not started'),
 (N'dash.empty.title',       N'Chưa có dự án nào',                                N'No projects yet'),
 (N'dash.empty.text',        N'Bạn chưa tham gia dự án nào. Bắt đầu bằng cách đăng yêu cầu chuyển giao công nghệ đầu tiên.', N'You have not joined any project yet. Start by posting your first technology transfer request.'),
 (N'dash.postRequest',       N'Đăng yêu cầu chuyển giao công nghệ',               N'Post a technology transfer request'),
 -- MyProjects
 (N'myproj.listHeading',     N'Danh sách hồ sơ giao dịch',                        N'Transaction records'),
 (N'myproj.totalPrefix',     N'Tổng:',                                            N'Total:'),
 (N'myproj.recordsSuffix',   N'hồ sơ',                                            N'records'),
 (N'myproj.empty.title',     N'Chưa có hồ sơ giao dịch nào',                      N'No transaction records yet'),
 (N'myproj.empty.text',      N'Bạn chưa tham gia hồ sơ giao dịch/chuyển giao nào. Bắt đầu bằng cách đăng yêu cầu chuyển giao công nghệ đầu tiên.', N'You have not joined any transaction or transfer record yet. Start by posting your first technology transfer request.'),
 (N'myproj.col.code',        N'Mã hồ sơ',                                         N'Record code'),
 (N'myproj.col.name',        N'Tên hồ sơ',                                        N'Record name'),
 (N'myproj.view',            N'Xem',                                              N'View'),
 (N'myproj.summary.total',   N'Tổng hồ sơ',                                       N'Total records'),
 (N'myproj.summary.active',  N'Hồ sơ đang xử lý',                                 N'Records in progress'),
 (N'myproj.summary.incomplete', N'Hồ sơ chưa hoàn thành',                         N'Incomplete records'),
 -- _AuthTopbar
 (N'topbar.searchPh',        N'Tìm công nghệ, thiết bị, chuyên gia, dự án...',    N'Search technologies, equipment, experts, projects...'),
 (N'topbar.backToSite',      N'Về trang chủ',                                     N'Back to homepage'),
 -- _Layout
 (N'layout.siteName',        N'Sàn Giao dịch Công nghệ Thành phố Cần Thơ',        N'Can Tho City Technology Exchange');
INSERT INTO dbo.UiTranslations([Key],Vi,En,Creator)
SELECT s.k,s.vi,s.en,'i18n-page-seed' FROM @s s WHERE NOT EXISTS(SELECT 1 FROM dbo.UiTranslations t WHERE t.[Key]=s.k);
SELECT @@ROWCOUNT AS Inserted, (SELECT COUNT(*) FROM dbo.UiTranslations) AS Total;
