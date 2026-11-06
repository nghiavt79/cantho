# Ghi chú: repo GitHub công khai, nội dung đã push rồi gỡ

Ngày ghi: 05/11/2026. Tài liệu nội bộ, không đưa lên GitHub.

## Quyết định

- Repo `nghiavt79/cantho` là fork **công khai**. Anh Nghĩa quyết định **không tạo repo riêng tư**.
- Vì vậy không đưa lên GitHub: hồ sơ thầu, hợp đồng, công văn, báo cáo pentest, gói triển khai, mật khẩu.
- Code và tài liệu mới hiện chỉ lưu trên máy.

## Đã xảy ra

Ngày 05/11/2026 đã push lên GitHub, trước khi kiểm tra repo công khai hay riêng tư:

| Nhánh | Commit | Nội dung |
|---|---|---|
| `master` | `0bd5182`, `f9c1ede` | Code HSMT, bản sửa lỗi L2, dự thảo phản hồi |
| `feature/bo-sung-chuc-nang-hsmt` | `e1bc2c7` | Code HSMT đến 31/10/2026 |
| `claude/sua-loi-l2-vht2` | `f9c1ede` | Như `master` |
| `claude/api-hop-dong-ky-usb-token-26ca31` | `e0dd8f2` | Phần đầu tích hợp eContract |
| `docs/ho-so-thue-san-2026-2030` | `0f7bd9b` | Công văn, hồ sơ thuê Sàn, có file ghi VTISC |

## Đã gỡ

- Xóa 3 nhánh `claude/sua-loi-l2-vht2`, `claude/api-hop-dong-ky-usb-token-26ca31`, `docs/ho-so-thue-san-2026-2030`.
- Đưa `master` về `fef0530`, `feature/bo-sung-chuc-nang-hsmt` về `4361b43`.
- Đã kiểm tra từ bên ngoài: GitHub chỉ còn 2 nhánh này; các file đã gỡ báo 404 qua đường dẫn nhánh.

## Còn tồn tại trên GitHub

1. Commit `4361b43` (đã có trên GitHub từ trước 05/11/2026) vẫn chứa mật khẩu tài khoản thử, ở 2 file:
   - `TechExchangeApp/Scripts/create-tai-khoan-van-hanh-thu.sql`
   - `docs/Trien_khai_mekongx_25102026.md`
2. `appsettings.json` trên cả 2 nhánh có chuỗi kết nối kèm mật khẩu và `EncryptKeyword`.
3. Các commit `f9c1ede`, `0bd5182`, `e1bc2c7`, `0f7bd9b`, `e0dd8f2` vẫn mở được bằng liên kết trực tiếp. Muốn xóa hẳn: xóa repo hoặc nhờ GitHub Support dọn.

## Việc cần làm

- [ ] Đổi mật khẩu trên mekongx.vn: `ttud_*`, `vht_*`, `gs_theodoi`, `ncc_hotro`; ưu tiên tài khoản quản trị. Báo trước và gửi lại mật khẩu cho đơn vị vận hành thử, giám sát.
- [ ] So mật khẩu cơ sở dữ liệu, `EncryptKeyword` trên máy chủ với `appsettings.json`; nếu giống thì đổi.
- [ ] Chạy gói `deploy-mekongx-2026-11-05.zip` trên máy chủ theo `HUONG_DAN.txt` trong gói.

## Lưu ý khi dùng git

- **Không chạy `git push` trên `master` hoặc `feature/bo-sung-chuc-nang-hsmt` ở máy:** nội dung đã gỡ sẽ quay lại GitHub.
- Bản trên máy:

| Nhánh | Commit trên máy | Trên GitHub |
|---|---|---|
| `master` | `f9c1ede` | `fef0530` |
| `feature/bo-sung-chuc-nang-hsmt` | `e1bc2c7` | `4361b43` |
| `claude/sua-loi-l2-vht2` | `f9c1ede` | Không có |
| `claude/api-hop-dong-ky-usb-token-26ca31` | `e0dd8f2` | Không có |
| `docs/ho-so-thue-san-2026-2030` | `0f7bd9b` | Không có |

- `feature/bo-sung-chuc-nang-hsmt` trên máy chưa có bản sửa lỗi L2; bản sửa ở `master` và `claude/sua-loi-l2-vht2`.
