<#
.SYNOPSIS
    Chạy ứng dụng BaSau Bags ở chế độ LAN để thiết bị khác cùng mạng (điện thoại, laptop) truy cập được.

.DESCRIPTION
    Không ghi cứng địa chỉ hay cổng: mỗi lần chạy script tự
      1. tìm địa chỉ IPv4 mạng LAN thật của máy (bỏ qua card ảo như Hyper-V, WSL, VPN, VirtualBox),
      2. tìm cổng còn trống (bắt đầu từ -Port, mặc định 5226),
      3. kiểm tra tường lửa Windows đã cho phép cổng đó chưa,
      4. chạy ứng dụng lắng nghe mọi địa chỉ (0.0.0.0) và in ra đường dẫn để mở từ thiết bị khác.

.PARAMETER Port
    Cổng muốn bắt đầu thử. Nếu đang bị chiếm sẽ thử cổng kế tiếp (tối đa 50 cổng).

.PARAMETER Ip
    Chỉ định IP LAN thủ công khi máy có nhiều card mạng và script chọn chưa đúng.

.PARAMETER MoTuongLua
    Tự tạo luật tường lửa cho cổng đang dùng (cần chạy PowerShell bằng Run as administrator).
    Luật chỉ áp dụng cho mạng Private và tự xóa khi thoát (Ctrl+C).

.EXAMPLE
    .\chay-lan.ps1
    .\chay-lan.ps1 -Port 8080
    .\chay-lan.ps1 -MoTuongLua
#>
[CmdletBinding()]
param(
    [int]$Port = 5226,
    [string]$Ip,
    [switch]$MoTuongLua
)

$ErrorActionPreference = 'Stop'
$duAn = Join-Path $PSScriptRoot 'DoAnLTW\DoAnLTW'
if (-not (Test-Path (Join-Path $duAn 'DoAnLTW.csproj'))) {
    throw "Không tìm thấy DoAnLTW.csproj trong '$duAn'. Hãy để file chay-lan.ps1 ở thư mục gốc của repo."
}

# ---------- 1. Tìm IP LAN ----------
function Get-LanIp {
    # card ảo thường gặp: không phải mạng mà thiết bị khác nhìn thấy
    $ao = 'vEthernet|Hyper-V|WSL|VirtualBox|VMware|Loopback|Bluetooth|TAP|VPN|Tailscale|ZeroTier|Docker'
    $cauHinh = Get-NetIPConfiguration | Where-Object {
        $_.IPv4Address -and $_.IPv4DefaultGateway -and $_.NetAdapter.Status -eq 'Up' -and
        $_.InterfaceAlias -notmatch $ao -and $_.InterfaceDescription -notmatch $ao
    }
    # có cổng mặc định = đang ra mạng thật; ưu tiên Wi-Fi/Ethernet có metric thấp nhất
    $cauHinh | Sort-Object { $_.IPv4Address[0].PrefixOrigin -ne 'Dhcp' }, { $_.NetIPv4Interface.InterfaceMetric } |
        ForEach-Object { $_.IPv4Address[0].IPAddress } | Select-Object -First 1
}

if (-not $Ip) { $Ip = Get-LanIp }
if (-not $Ip) {
    throw "Không tìm thấy mạng LAN (máy chưa nối Wi-Fi/Ethernet?). Nối mạng rồi chạy lại, hoặc dùng -Ip <địa chỉ>."
}

# ---------- 2. Tìm cổng trống ----------
function Test-CongTrong([int]$p) {
    # cổng đang có tiến trình nghe (kể cả chỉ nghe localhost) thì coi là bận: Windows vẫn cho bind 0.0.0.0 chồng lên 127.0.0.1
    if (Get-NetTCPConnection -LocalPort $p -State Listen -ErrorAction SilentlyContinue) { return $false }
    $l = $null
    try {
        $l = [System.Net.Sockets.TcpListener]::new([System.Net.IPAddress]::Any, $p)
        $l.Start()
        return $true
    } catch { return $false } finally { if ($l) { $l.Stop() } }
}

$cong = $null
foreach ($p in $Port..($Port + 49)) {
    if (Test-CongTrong $p) { $cong = $p; break }
}
if (-not $cong) { throw "Không tìm được cổng trống trong khoảng $Port-$($Port + 49)." }
if ($cong -ne $Port) { Write-Host "Cổng $Port đang bận, dùng cổng $cong." -ForegroundColor Yellow }

# ---------- 3. Tường lửa ----------
$tenLuat = "BaSau LAN demo $cong"
$laAdmin = ([Security.Principal.WindowsPrincipal][Security.Principal.WindowsIdentity]::GetCurrent()).IsInRole(
    [Security.Principal.WindowsBuiltInRole]::Administrator)
$daTaoLuat = $false

function Test-TuongLuaChoPhep([int]$p) {
    # có luật Allow Inbound TCP đang bật cho cổng này (hoặc cho dotnet) thì coi như đã mở
    $luat = Get-NetFirewallRule -Direction Inbound -Action Allow -Enabled True -ErrorAction SilentlyContinue
    foreach ($r in $luat) {
        $pf = $r | Get-NetFirewallPortFilter
        if ($pf.Protocol -eq 'TCP' -and ($pf.LocalPort -eq "$p" -or $pf.LocalPort -eq 'Any')) {
            $af = $r | Get-NetFirewallApplicationFilter
            if ($af.Program -eq 'Any' -or $af.Program -like '*dotnet*') { return $true }
        }
    }
    return $false
}

if ($MoTuongLua) {
    if (-not $laAdmin) {
        Write-Host "-MoTuongLua cần quyền administrator. Mở PowerShell bằng 'Run as administrator' rồi chạy lại." -ForegroundColor Red
        exit 1
    }
    New-NetFirewallRule -DisplayName $tenLuat -Direction Inbound -Protocol TCP -LocalPort $cong -Action Allow -Profile Private | Out-Null
    $daTaoLuat = $true
    Write-Host "Đã mở cổng $cong trên tường lửa (chỉ mạng Private). Luật tự xóa khi thoát." -ForegroundColor Green
}
elseif (-not (Test-TuongLuaChoPhep $cong)) {
    Write-Host ""
    Write-Host "Tường lửa có thể đang chặn cổng $cong, thiết bị khác sẽ không vào được." -ForegroundColor Yellow
    Write-Host "Cách 1: chạy lại script này bằng PowerShell 'Run as administrator' kèm -MoTuongLua"
    Write-Host "Cách 2: tự mở cổng (cần quyền administrator):"
    Write-Host "  New-NetFirewallRule -DisplayName '$tenLuat' -Direction Inbound -Protocol TCP -LocalPort $cong -Action Allow -Profile Private"
    Write-Host ""
}

# ---------- 4. Chạy ứng dụng ----------
$env:ASPNETCORE_ENVIRONMENT = 'Development'
# máy dùng SDK mới hơn runtime của dự án (.NET 8) thì cho phép chạy bằng runtime mới hơn
$env:DOTNET_ROLL_FORWARD = 'Major'

Write-Host ""
Write-Host "================ BaSau Bags - chế độ LAN ================" -ForegroundColor Cyan
Write-Host "  Trên máy này : http://localhost:$cong"
Write-Host "  Từ thiết bị khác cùng mạng : http://${Ip}:$cong" -ForegroundColor Green
Write-Host "  Dừng: nhấn Ctrl+C"
Write-Host "==========================================================" -ForegroundColor Cyan
Write-Host ""

Push-Location $duAn
try {
    dotnet run --no-launch-profile --urls "http://0.0.0.0:$cong"
}
finally {
    Pop-Location
    if ($daTaoLuat) {
        Remove-NetFirewallRule -DisplayName $tenLuat -ErrorAction SilentlyContinue
        Write-Host "Đã xóa luật tường lửa '$tenLuat'." -ForegroundColor Green
    }
}
