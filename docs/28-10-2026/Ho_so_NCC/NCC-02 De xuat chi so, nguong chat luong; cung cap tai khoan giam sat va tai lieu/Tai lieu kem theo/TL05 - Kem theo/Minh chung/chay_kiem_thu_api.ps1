<#
  Chạy các ca kiểm thử API có đăng nhập (Kịch bản kiểm thử chi tiết, mục IV) trên https://mekongx.vn
  và ghi nhật ký minh chứng MC-KT-xx-<ngày>-<tài khoản>.txt cùng thư mục với script.

  Cách lấy Cookie: đăng nhập tài khoản kiểm thử trên trình duyệt › F12 › Network › bấm một yêu cầu tới mekongx.vn
  › Request Headers › sao chép toàn bộ giá trị dòng "Cookie".

  Ví dụ (Windows PowerShell 5.1):
    powershell -ExecutionPolicy Bypass -File .\chay_kiem_thu_api.ps1 -TaiKhoan vht_buyer01 -Cookie "<cookie>" `
        -ProductId 35393 -ProjectId <mã hồ sơ HS-KNDA> -CookieKhac "<cookie của vht_seller02>"
  -CookieKhac (không bắt buộc): tài khoản KHÔNG phải thành viên cuộc trò chuyện / hồ sơ, dùng cho ca không có quyền.
  Các ca ghi dữ liệu tạo dữ liệu thật trên hệ thống (tin nhắn, đánh giá, yêu cầu hỗ trợ): chỉ dùng tài khoản và sản phẩm kiểm thử.
#>
param(
    [Parameter(Mandatory = $true)][string]$TaiKhoan,
    [Parameter(Mandatory = $true)][string]$Cookie,
    [Parameter(Mandatory = $true)][int]$ProductId,
    [Parameter(Mandatory = $true)][int]$ProjectId,
    [string]$CookieKhac = '',
    [string]$BaseUrl = 'https://mekongx.vn'
)
$ErrorActionPreference = 'Stop'
[Net.ServicePointManager]::SecurityProtocol = [Net.SecurityProtocolType]::Tls12
Add-Type -AssemblyName System.Net.Http
$ngay = Get-Date -Format 'yyyyMMdd'
$dir = Split-Path -Parent $MyInvocation.MyCommand.Path
$tongHop = @()

function New-Client([string]$cookie) {
    $h = New-Object System.Net.Http.HttpClientHandler
    $h.UseCookies = $false; $h.AllowAutoRedirect = $false
    $c = New-Object System.Net.Http.HttpClient($h)
    if ($cookie) { $c.DefaultRequestHeaders.TryAddWithoutValidation('Cookie', $cookie) | Out-Null }
    return $c
}
function Get-Token($client) {
    $html = $client.GetStringAsync("$BaseUrl/").Result
    $m = [regex]::Match($html, 'name="__RequestVerificationToken"\s+content="([^"]+)"')
    if (-not $m.Success) { throw 'Không lấy được mã chống giả mạo: kiểm tra Cookie còn hiệu lực (đã đăng nhập).' }
    return $m.Groups[1].Value
}
function Invoke-Ca([string]$ma, [string]$moTa, $client, [string]$method, [string]$path, $body, [string]$token, [int[]]$kyVong, [string]$kieu = 'json') {
    $req = New-Object System.Net.Http.HttpRequestMessage([System.Net.Http.HttpMethod]::new($method), "$BaseUrl$path")
    if ($token) { $req.Headers.TryAddWithoutValidation('RequestVerificationToken', $token) | Out-Null }
    $bodyText = ''
    if ($kieu -eq 'json' -and $body -ne $null) {
        $bodyText = ($body | ConvertTo-Json -Compress)
        $req.Content = New-Object System.Net.Http.StringContent($bodyText, [Text.Encoding]::UTF8, 'application/json')
    } elseif ($kieu -eq 'multipart') {
        $mp = New-Object System.Net.Http.MultipartFormDataContent
        $bytes = [Text.Encoding]::ASCII.GetBytes("%PDF-1.4`n1 0 obj<<>>endobj`ntrailer<<>>`n%%EOF")
        $fc = New-Object System.Net.Http.ByteArrayContent(,$bytes)
        $fc.Headers.ContentType = [System.Net.Http.Headers.MediaTypeHeaderValue]::Parse('application/pdf')
        $mp.Add($fc, 'file', "kiem-thu-$ngay.pdf"); $mp.Add((New-Object System.Net.Http.StringContent('Tệp kiểm thử')), 'message')
        $req.Content = $mp; $bodyText = "multipart: file=kiem-thu-$ngay.pdf, message=Tệp kiểm thử"
    }
    $res = $client.SendAsync($req).Result
    $code = [int]$res.StatusCode
    $text = $res.Content.ReadAsStringAsync().Result
    $loc = if ($res.Headers.Location) { "Location: $($res.Headers.Location)" } else { '' }
    $khop = if ($kyVong -contains $code) { 'Khớp kết quả mong đợi' } else { 'KHÔNG khớp kết quả mong đợi' }
    $tep = Join-Path $dir "MC-$ma-$ngay-$TaiKhoan.txt"
    @(
        "Mã minh chứng: MC-$ma-$ngay-$TaiKhoan", "Ca kiểm thử: $ma – $moTa",
        "Thời điểm: $(Get-Date -Format 'dd/MM/yyyy HH:mm:ss')", "Tài khoản: $TaiKhoan", "Môi trường: $BaseUrl",
        '------------------------------------------------------------',
        "Yêu cầu: $method $path", "Mã chống giả mạo: $(if ($token) { 'có gửi' } else { 'không gửi' })", "Thân: $bodyText", '',
        "Kết quả: HTTP $code $loc", "Nội dung trả về (tối đa 1.500 ký tự):", $text.Substring(0, [Math]::Min(1500, $text.Length)), '',
        "Mong đợi: HTTP $($kyVong -join ' hoặc ') → $khop"
    ) | Set-Content -Path $tep -Encoding UTF8
    $script:tongHop += [pscustomobject]@{ Ca = $ma; MoTa = $moTa; HTTP = $code; MongDoi = ($kyVong -join '/'); DanhGia = $khop; MinhChung = "MC-$ma-$ngay-$TaiKhoan" }
    return $text
}

$c = New-Client $Cookie
$t = Get-Token $c
Write-Host "Đã lấy mã chống giả mạo cho $TaiKhoan. Bắt đầu chạy..."

# Trợ lý AI
Invoke-Ca 'KT-01' 'Lấy nút gợi ý AI' $c GET '/api/ai-chat/suggestions' $null '' @(200) | Out-Null
Invoke-Ca 'KT-02' 'Lịch sử AI với sessionKey ngẫu nhiên' $c GET "/api/ai-chat/history?sessionKey=$([guid]::NewGuid().ToString('N'))" $null '' @(200) | Out-Null
$r3 = Invoke-Ca 'KT-03' 'Gửi câu hỏi AI hợp lệ (ghi thành công)' $c POST '/api/ai-chat/message' @{ message = 'Tìm công nghệ xử lý nước thải' } $t @(200)
Invoke-Ca 'KT-04a' 'Gửi câu hỏi AI không có mã chống giả mạo' $c POST '/api/ai-chat/message' @{ message = 'test' } '' @(400) | Out-Null
Invoke-Ca 'KT-04b' 'Gửi câu hỏi AI rỗng' $c POST '/api/ai-chat/message' @{ message = '' } $t @(400) | Out-Null
Invoke-Ca 'KT-05' 'Gửi góp ý AI không có mã chống giả mạo' $c POST '/api/ai-chat/submit-feedback' @{ fullName = 'Kiểm thử'; message = 'test' } '' @(400) | Out-Null
Invoke-Ca 'KT-31' 'Gửi góp ý AI hợp lệ (ghi thành công)' $c POST '/api/ai-chat/submit-feedback' @{ fullName = "Kiểm thử $TaiKhoan"; email = 'kiemthu@example.com'; phone = '0900000000'; message = "Ca KT-31 ngày $ngay" } $t @(200) | Out-Null
$sk = ''; try { $sk = ($r3 | ConvertFrom-Json).sessionKey } catch {}
if ($sk) { Invoke-Ca 'KT-25' 'Đọc lịch sử AI bằng sessionKey của KT-03 khi ẩn danh' (New-Client '') GET "/api/ai-chat/history?sessionKey=$sk" $null '' @(200) | Out-Null }

# Tin nhắn
Invoke-Ca 'KT-08' 'Mở cuộc trò chuyện không có mã chống giả mạo (lỗi L-02)' $c POST '/api/chat/start' @{ productId = $ProductId } '' @(400) | Out-Null
$r32 = Invoke-Ca 'KT-32' 'Mở cuộc trò chuyện với nhà cung cấp của sản phẩm (ghi thành công)' $c POST '/api/chat/start' @{ productId = $ProductId } $t @(200)
$chatId = 0; try { $o = $r32 | ConvertFrom-Json; $chatId = [long]($(if ($o.conversationId) { $o.conversationId } elseif ($o.id) { $o.id } else { 0 })) } catch {}
Invoke-Ca 'KT-09' 'Gửi tin nhắn không có mã chống giả mạo (lỗi L-02)' $c POST "/api/chat/$chatId/send" @{ message = 'test' } '' @(400) | Out-Null
Invoke-Ca 'KT-33' 'Gửi tin nhắn trong cuộc trò chuyện (ghi thành công)' $c POST "/api/chat/$chatId/send" @{ message = "Tin nhắn kiểm thử KT-33 $ngay" } $t @(200) | Out-Null
Invoke-Ca 'KT-11' 'Gửi tệp không có mã chống giả mạo' $c POST "/api/chat/$chatId/upload" $null '' @(400) 'multipart' | Out-Null
Invoke-Ca 'KT-34' 'Gửi tệp PDF trong cuộc trò chuyện (ghi thành công)' $c POST "/api/chat/$chatId/upload" $null $t @(200) 'multipart' | Out-Null
Invoke-Ca 'KT-12' 'Đếm tin nhắn chưa đọc' $c GET '/api/chat/unread-count' $null '' @(200) | Out-Null
Invoke-Ca 'KT-13' 'Đọc tin nhắn của cuộc trò chuyện (thành viên)' $c GET "/api/chat/$chatId/messages" $null '' @(200) | Out-Null
Invoke-Ca 'KT-15' 'Đọc tin nhắn cuộc trò chuyện số 1 (không phải thành viên)' $c GET '/api/chat/1/messages' $null '' @(404) | Out-Null
Invoke-Ca 'KT-40' 'Tạo phòng họp từ cuộc trò chuyện (ghi thành công)' $c POST "/api/chat/$chatId/video-call" $null $t @(200) | Out-Null
Invoke-Ca 'KT-27' 'Tạo phòng họp cho cuộc trò chuyện không tồn tại' $c POST '/api/chat/0/video-call' $null $t @(404) | Out-Null

# Đánh giá, lượt xem
$sum1 = Invoke-Ca 'KT-16' 'Số liệu tổng hợp sản phẩm (trước khi ghi)' $c GET "/api/entity/SanPhamCNTB/$ProductId/summary" $null '' @(200)
Invoke-Ca 'KT-17' 'Lưu đánh giá 0 sao không có mã chống giả mạo (lỗi L-02)' $c POST "/api/entity/SanPhamCNTB/$ProductId/rating/save" @{ stars = 0 } '' @(400) | Out-Null
Invoke-Ca 'KT-35' 'Lưu đánh giá 5 sao (ghi thành công)' $c POST "/api/entity/SanPhamCNTB/$ProductId/rating/save" @{ stars = 5; title = 'Kiểm thử'; comment = "KT-35 $ngay" } $t @(200) | Out-Null
Invoke-Ca 'KT-19' 'Tăng lượt xem với loại đối tượng sai' $c POST '/api/entity/KhongHopLe/1/view/increase' $null '' @(400) | Out-Null
Invoke-Ca 'KT-36' 'Tăng lượt xem sản phẩm (ghi thành công)' $c POST "/api/entity/SanPhamCNTB/$ProductId/view/increase" $null '' @(200) | Out-Null
Invoke-Ca 'KT-16b' 'Số liệu tổng hợp sản phẩm (sau khi ghi: lượt xem +1, có đánh giá của tài khoản)' $c GET "/api/entity/SanPhamCNTB/$ProductId/summary" $null '' @(200) | Out-Null
Invoke-Ca 'KT-20' 'Danh sách đánh giá sản phẩm' $c GET "/api/entity/SanPhamCNTB/$ProductId/ratings?take=5" $null '' @(200) | Out-Null
Invoke-Ca 'KT-21' 'Số liệu trang chủ' $c GET '/api/home/analytics' $null '' @(200) | Out-Null

# Thông báo
$r22 = Invoke-Ca 'KT-22' 'Thông báo mới nhất (đã đăng nhập)' $c GET '/api/notifications/latest?take=5' $null '' @(200)
Invoke-Ca 'KT-23' 'Đếm thông báo chưa đọc (trước khi ghi)' $c GET '/api/notifications/unread-count' $null '' @(200) | Out-Null
Invoke-Ca 'KT-24' 'Đánh dấu đã đọc không có mã chống giả mạo (lỗi L-02)' $c POST '/api/notifications/mark-read/0' $null '' @(400) | Out-Null
$nid = 0; try { $nid = (($r22 | ConvertFrom-Json) | Where-Object { -not $_.isRead } | Select-Object -First 1).id } catch {}
if ($nid) { Invoke-Ca 'KT-37' "Đánh dấu thông báo $nid đã đọc (ghi thành công)" $c POST "/api/notifications/mark-read/$nid" $null $t @(200) | Out-Null }
Invoke-Ca 'KT-38' 'Đánh dấu tất cả đã đọc – API-18 (ghi thành công)' $c POST '/api/notifications/mark-all-read' $null $t @(200) | Out-Null
Invoke-Ca 'KT-23b' 'Đếm thông báo chưa đọc (sau khi ghi: bằng 0)' $c GET '/api/notifications/unread-count' $null '' @(200) | Out-Null

# Yêu cầu Trung tâm tư vấn
Invoke-Ca 'KT-26' 'Yêu cầu hỗ trợ với hồ sơ không hợp lệ' $c POST '/api/support/start' @{ projectId = 0; requestType = 1; message = 'test' } $t @(400) | Out-Null
Invoke-Ca 'KT-39' "Yêu cầu hỗ trợ cho hồ sơ $ProjectId (ghi thành công)" $c POST '/api/support/start' @{ projectId = $ProjectId; requestType = 1; stepNumber = 1; subject = 'Kiểm thử KT-39'; description = "Ca KT-39 $ngay"; message = "Ca KT-39 $ngay" } $t @(200) | Out-Null

# Ca không có quyền (tài khoản không phải thành viên)
if ($CookieKhac) {
    $k = New-Client $CookieKhac; $tk = Get-Token $k
    Invoke-Ca 'KT-41' "Tài khoản khác đọc cuộc trò chuyện $chatId" $k GET "/api/chat/$chatId/messages" $null '' @(403, 404) | Out-Null
    Invoke-Ca 'KT-42' "Tài khoản khác gửi tin vào cuộc trò chuyện $chatId" $k POST "/api/chat/$chatId/send" @{ message = 'không có quyền' } $tk @(400, 403, 404) | Out-Null
    Invoke-Ca 'KT-43' "Tài khoản khác gửi yêu cầu hỗ trợ cho hồ sơ $ProjectId" $k POST '/api/support/start' @{ projectId = $ProjectId; requestType = 1; message = 'không có quyền' } $tk @(403, 404) | Out-Null
}

$csv = Join-Path $dir "TongHop-KT-$ngay-$TaiKhoan.csv"
$tongHop | Export-Csv -Path $csv -NoTypeInformation -Encoding UTF8
$tongHop | Format-Table -AutoSize
Write-Host "Đã ghi $($tongHop.Count) tệp minh chứng và bảng tổng hợp: $csv"
