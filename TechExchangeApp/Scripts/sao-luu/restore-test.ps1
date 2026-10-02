<#
.SYNOPSIS
    Chạy thử phục hồi (HSMT 2.2, 2.10): khôi phục bản Full mới nhất (+ bản Diff mới nhất sau nó)
    vào một CSDL tạm, đếm số bản ghi các bảng chính rồi xóa CSDL tạm. Kết quả dùng làm biên bản.

.EXAMPLE
    .\restore-test.ps1 -BackupRoot "E:\Backup\TechExchange"
#>
param(
    [string]$SqlServer = 'localhost',
    [string]$Database = 'TechExchangeNew',
    [Parameter(Mandatory = $true)]
    [string]$BackupRoot,
    [string]$TestDatabase = 'TechExchangeNew_RestoreTest',
    [switch]$KeepDatabase
)

$ErrorActionPreference = 'Stop'

function Invoke-Sql([string]$query) {
    $out = & sqlcmd -S $SqlServer -E -b -W -s '|' -Q $query 2>&1
    if ($LASTEXITCODE -ne 0) { throw "sqlcmd lỗi ($LASTEXITCODE): $($out -join ' ')" }
    return $out
}

$dbDir = Join-Path $BackupRoot 'db'
$full = Get-ChildItem $dbDir -Filter "$Database`_FULL_*.bak" | Sort-Object LastWriteTime -Descending | Select-Object -First 1
if (-not $full) { throw "Không có bản Full trong $dbDir" }
$diff = Get-ChildItem $dbDir -Filter "$Database`_DIFF_*.bak" |
        Where-Object { $_.LastWriteTime -gt $full.LastWriteTime } |
        Sort-Object LastWriteTime -Descending | Select-Object -First 1

$started = Get-Date
Write-Output "Bản Full : $($full.FullName)"
Write-Output "Bản Diff : $(if ($diff) { $diff.FullName } else { '(không có)' })"

# Đổi tên tệp dữ liệu/log sang tên CSDL tạm, đặt cùng thư mục dữ liệu mặc định của máy chủ
$dataDir = (Invoke-Sql "SET NOCOUNT ON; SELECT CAST(SERVERPROPERTY('InstanceDefaultDataPath') AS NVARCHAR(400))" | Select-Object -Skip 2 -First 1).Trim()
$files = Invoke-Sql "SET NOCOUNT ON; RESTORE FILELISTONLY FROM DISK = N'$($full.FullName)'" | Select-Object -Skip 2
$moves = @()
$i = 0
foreach ($line in $files) {
    $cols = $line -split '\|'
    if ($cols.Count -lt 3) { continue }
    $ext = if ($cols[2].Trim() -eq 'L') { 'ldf' } else { 'mdf' }
    $moves += "MOVE N'$($cols[0].Trim())' TO N'$dataDir$TestDatabase`_$i.$ext'"
    $i++
}
$moveSql = $moves -join ', '
$recovery = if ($diff) { 'NORECOVERY' } else { 'RECOVERY' }

Invoke-Sql "IF DB_ID(N'$TestDatabase') IS NOT NULL BEGIN ALTER DATABASE [$TestDatabase] SET SINGLE_USER WITH ROLLBACK IMMEDIATE; DROP DATABASE [$TestDatabase]; END" | Out-Null
Invoke-Sql "RESTORE DATABASE [$TestDatabase] FROM DISK = N'$($full.FullName)' WITH $moveSql, $recovery, CHECKSUM" | Out-Null
if ($diff) {
    Invoke-Sql "RESTORE DATABASE [$TestDatabase] FROM DISK = N'$($diff.FullName)' WITH RECOVERY, CHECKSUM" | Out-Null
}

Write-Output ""
Write-Output "Số bản ghi sau phục hồi (so với CSDL đang chạy):"
$tables = 'Users', 'SanPhamCNTB', 'NhaCungUng', 'Contents', 'Projects'
foreach ($t in $tables) {
    $r = Invoke-Sql "SET NOCOUNT ON; SELECT (SELECT COUNT(*) FROM [$TestDatabase].dbo.[$t]), (SELECT COUNT(*) FROM [$Database].dbo.[$t])" | Select-Object -Skip 2 -First 1
    $c = $r -split '\|'
    Write-Output ("  {0,-14} phục hồi: {1,8}   đang chạy: {2,8}" -f $t, $c[0].Trim(), $c[1].Trim())
}
$check = Invoke-Sql "DBCC CHECKDB([$TestDatabase]) WITH NO_INFOMSGS"
Write-Output "DBCC CHECKDB: không có lỗi"

if (-not $KeepDatabase) {
    Invoke-Sql "ALTER DATABASE [$TestDatabase] SET SINGLE_USER WITH ROLLBACK IMMEDIATE; DROP DATABASE [$TestDatabase];" | Out-Null
    Write-Output "Đã xóa CSDL tạm $TestDatabase"
}
Write-Output ("Thời gian phục hồi: {0:N0} giây" -f ((Get-Date) - $started).TotalSeconds)
