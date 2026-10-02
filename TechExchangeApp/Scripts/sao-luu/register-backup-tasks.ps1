<#
.SYNOPSIS
    Đăng ký 2 tác vụ Windows Task Scheduler cho sao lưu tự động (chạy với quyền Administrator trên máy chủ):
      - TechExchange-Backup-Full : hằng ngày 01:00 (CSDL Full + đồng bộ uploads)
      - TechExchange-Backup-Diff : mỗi 6 giờ (07:00, 13:00, 19:00) sao lưu vi sai
    Tác vụ chạy bằng tài khoản SYSTEM; tài khoản này phải có quyền sysadmin hoặc db_backupoperator trên SQL Server
    (mặc định NT AUTHORITY\SYSTEM không có — cấp quyền trước, hoặc đổi -RunAsUser).

.EXAMPLE
    .\register-backup-tasks.ps1 -BackupRoot "E:\Backup\TechExchange" -UploadsPath "D:\Web\TechExchangeApp\wwwroot\uploads"
#>
param(
    [Parameter(Mandatory = $true)]
    [string]$BackupRoot,
    [Parameter(Mandatory = $true)]
    [string]$UploadsPath,
    [string]$SqlServer = 'localhost',
    [string]$Database = 'TechExchangeNew',
    [string]$RunAsUser = 'SYSTEM'
)

$script = Join-Path $PSScriptRoot 'backup-techexchange.ps1'
$common = "-NoProfile -ExecutionPolicy Bypass -File `"$script`" -SqlServer `"$SqlServer`" -Database `"$Database`" -BackupRoot `"$BackupRoot`""

$fullAction = New-ScheduledTaskAction -Execute 'powershell.exe' -Argument "$common -Mode Full -UploadsPath `"$UploadsPath`""
$fullTrigger = New-ScheduledTaskTrigger -Daily -At '01:00'
Register-ScheduledTask -TaskName 'TechExchange-Backup-Full' -Action $fullAction -Trigger $fullTrigger `
    -User $RunAsUser -RunLevel Highest -Force -Description 'Sao lưu Full CSDL + uploads Sàn GDCN' | Out-Null

$diffAction = New-ScheduledTaskAction -Execute 'powershell.exe' -Argument "$common -Mode Diff"
$diffTriggers = @('07:00', '13:00', '19:00') | ForEach-Object { New-ScheduledTaskTrigger -Daily -At $_ }
Register-ScheduledTask -TaskName 'TechExchange-Backup-Diff' -Action $diffAction -Trigger $diffTriggers `
    -User $RunAsUser -RunLevel Highest -Force -Description 'Sao lưu vi sai CSDL Sàn GDCN' | Out-Null

Get-ScheduledTask -TaskName 'TechExchange-Backup-*' | Select-Object TaskName, State
