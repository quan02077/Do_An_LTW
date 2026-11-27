@echo off
rem Bam doi (double-click) de chay BaSau Bags o che do LAN. Logic nam trong chay-lan.ps1 (tu tim IP LAN, tu chon cong).
rem Muon tu mo tuong lua: chuot phai file nay > Run as administrator, hoac them -MoTuongLua o cuoi dong powershell ben duoi.
chcp 65001 >nul
cd /d "%~dp0"
powershell -NoProfile -ExecutionPolicy Bypass -File "%~dp0chay-lan.ps1" %*
echo.
echo Ung dung da dung. Nhan phim bat ky de dong cua so.
pause >nul
