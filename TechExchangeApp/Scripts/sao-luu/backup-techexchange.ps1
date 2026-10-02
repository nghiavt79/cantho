<#
.SYNOPSIS
    Sao lưu CSDL và thư mục tệp tải lên của Sàn giao dịch công nghệ (HSMT 2.2, 2.10).

.DESCRIPTION
    -Mode Full : sao lưu toàn bộ CSDL + đồng bộ thư mục uploads sang thư mục sao lưu (robocopy /MIR).
    -Mode Diff : sao lưu vi sai CSDL (chỉ phần thay đổi từ lần Full gần nhất).
    Mỗi bản sao lưu được kiểm tra bằng RESTORE VERIFYONLY. Bản cũ hơn -RetentionDays bị xóa.
    Trả mã thoát 1 khi lỗi để Task Scheduler / công cụ giám sát ghi nhận thất bại.

.EXAMPLE
    .\backup-techexchange.ps1 -Mode Full -BackupRoot "E:\Backup\TechExchange" -UploadsPath "D:\Web\TechExchangeApp\wwwroot\uploads"
#>
param(
    [ValidateSet('Full', 'Diff')]
    [string]$Mode = 'Full',
    [string]$SqlServer = 'localhost',
    [string]$Database = 'TechExchangeNew',
    [Parameter(Mandatory = $true)]
    [string]$BackupRoot,
    [string]$UploadsPath = '',
    [int]$RetentionDays = 30
)

$ErrorActionPreference = 'Stop'
$stamp  = Get-Date -Format 'yyyyMMdd_HHmm'
$dbDir  = Join-Path $BackupRoot 'db'
$logDir = Join-Path $BackupRoot 'logs'
New-Item -ItemType Directory -Force -Path $dbDir, $logDir | Out-Null
$logFile = Join-Path $logDir "backup_$stamp`_$Mode.log"

function Write-Log([string]$msg) {
    $line = "$(Get-Date -Format 'yyyy-MM-dd HH:mm:ss') $msg"
    Write-Output $line
    Add-Content -Path $logFile -Value $line -Encoding UTF8
}

function Invoke-Sql([string]$query) {
    # -b: thoát mã lỗi khi T-SQL lỗi; -E: Windows auth (tài khoản chạy tác vụ phải có quyền BACKUP)
    $out = & sqlcmd -S $SqlServer -E -b -W -h -1 -Q $query 2>&1
    if ($LASTEXITCODE -ne 0) { throw "sqlcmd lỗi ($LASTEXITCODE): $($out -join ' ')" }
    return $out
}

try {
    Write-Log "Bắt đầu sao lưu $Mode — $SqlServer/$Database"

    # Bản Express không hỗ trợ nén sao lưu
    $engineEdition = (Invoke-Sql "SET NOCOUNT ON; SELECT CAST(SERVERPROPERTY('EngineEdition') AS INT)" | Select-Object -First 1).Trim()
    $compress = if ($engineEdition -eq '4') { '' } else { ', COMPRESSION' }

    $suffix  = if ($Mode -eq 'Full') { 'FULL' } else { 'DIFF' }
    $bakFile = Join-Path $dbDir "$Database`_$suffix`_$stamp.bak"
    $diffOpt = if ($Mode -eq 'Diff') { 'DIFFERENTIAL, ' } else { '' }

    Invoke-Sql "BACKUP DATABASE [$Database] TO DISK = N'$bakFile' WITH $diffOpt INIT, CHECKSUM$compress, NAME = N'$Database $suffix $stamp'" | Out-Null
    Invoke-Sql "RESTORE VERIFYONLY FROM DISK = N'$bakFile' WITH CHECKSUM" | Out-Null
    $sizeMb = [math]::Round((Get-Item $bakFile).Length / 1MB, 1)
    Write-Log "Đã sao lưu và kiểm tra: $bakFile ($sizeMb MB)"

    if ($Mode -eq 'Full' -and $UploadsPath) {
        if (-not (Test-Path $UploadsPath)) { throw "Không thấy thư mục uploads: $UploadsPath" }
        $mirror = Join-Path $BackupRoot 'uploads-mirror'
        # /MIR đồng bộ (chỉ chép tệp thay đổi), /R:2 /W:5 thử lại, /NP /NFL /NDL log gọn
        & robocopy $UploadsPath $mirror /MIR /R:2 /W:5 /NP /NFL /NDL /LOG+:"$logFile" | Out-Null
        # robocopy: mã < 8 là thành công
        if ($LASTEXITCODE -ge 8) { throw "robocopy lỗi, mã $LASTEXITCODE" }
        Write-Log "Đã đồng bộ uploads sang $mirror"
    }

    $cutoff = (Get-Date).AddDays(-$RetentionDays)
    $old = Get-ChildItem $dbDir -Filter '*.bak' | Where-Object { $_.LastWriteTime -lt $cutoff }
    foreach ($f in $old) { Remove-Item $f.FullName -Force; Write-Log "Xóa bản cũ: $($f.Name)" }
    Get-ChildItem $logDir -Filter '*.log' | Where-Object { $_.LastWriteTime -lt $cutoff } | Remove-Item -Force

    Write-Log "Hoàn tất."
    exit 0
}
catch {
    Write-Log "LỖI: $($_.Exception.Message)"
    exit 1
}
