@echo off
chcp 65001 > nul
echo ===================================================
echo     DANG KHOI DONG HE THONG BASAU BAGS STORE
echo ===================================================
echo.
echo Dang mo trinh duyet den: http://localhost:5226
start http://localhost:5226
echo Dang chay may chu ASP.NET Core MVC...
echo Nhan Ctrl + C de dung may chu khi khong su dung.
echo.
dotnet run --project DoAnLTW\DoAnLTW\DoAnLTW.csproj
pause

