@echo off
chcp 936 >nul
setlocal EnableExtensions EnableDelayedExpansion
title ReverseNumbers - One Key Start

set "DOTNET_CLI_TELEMETRY_OPTOUT=1"
set "DOTNET_NOLOGO=1"
set "NPM_CONFIG_FUND=false"

cd /d "%~dp0"
set "ROOT=%CD%"
set "FRONTEND=%ROOT%\frontend"
set "BACKEND=%ROOT%\backend\ReverseNumbers.Api"
set "NODE_DIR=%ProgramFiles%\nodejs"
set "DOTNET_DIR=%ProgramFiles%\dotnet"

echo ============================================================
echo            Reverse Numbers  -  Env Check and Start
echo ============================================================
echo.

if not exist "%FRONTEND%\package.json" (
  echo [ERROR] 未找到前端目录：%FRONTEND%
  pause
  exit /b 1
)
if not exist "%BACKEND%\ReverseNumbers.Api.csproj" (
  echo [ERROR] 未找到后端目录：%BACKEND%
  pause
  exit /b 1
)

:: ================= 1. Node.js =================
echo [1/4] 检测 Node.js 运行环境 ...
call :ensure_node
if errorlevel 1 (
  echo.
  echo [FAILED] Node.js 环境不可用，脚本终止。
  pause
  exit /b 1
)
echo.

:: ================= 2. .NET 8 SDK =================
echo [2/4] 检测 .NET 8 SDK 运行环境 ...
call :ensure_dotnet
if errorlevel 1 (
  echo.
  echo [FAILED] .NET 8 SDK 环境不可用，脚本终止。
  pause
  exit /b 1
)
echo.

:: 仅检测模式：start.bat check
if /i "%~1"=="check" (
  echo [OK] 环境检测通过（仅检测模式，未启动服务）。
  exit /b 0
)

:: ================= 3. 前端依赖 =================
echo [3/4] 检查前端依赖 ...
if exist "%FRONTEND%\node_modules" (
  echo     node_modules 已存在，跳过安装。
) else (
  echo     未找到依赖，正在执行 npm install，请稍候 ...
  pushd "%FRONTEND%"
  call npm install
  set "NPM_RC=!errorlevel!"
  popd
  if not "!NPM_RC!"=="0" (
    echo [FAILED] 前端依赖安装失败，请检查网络或 npm 配置。
    pause
    exit /b 1
  )
  echo     前端依赖安装完成。
)
echo.

:: ================= 4. 启动服务 =================
echo [4/4] 启动服务 ...
call :port_listening 5247
if errorlevel 1 (
  echo     启动后端 API  http://127.0.0.1:5247 ...
  start "Backend API (5247)" /D "%BACKEND%" cmd /k dotnet run --launch-profile http
) else (
  echo     后端 API (5247) 已在运行，跳过启动。
)

call :port_listening 5173
if errorlevel 1 (
  echo     启动前端 Vite  http://127.0.0.1:5173 ...
  start "Frontend Vite (5173)" /D "%FRONTEND%" cmd /k npm run dev
) else (
  echo     前端 Vite (5173) 已在运行，跳过启动。
)
echo.

echo 等待服务就绪（首次构建可能较慢）...
call :wait_port 5247 180
if errorlevel 1 (
  echo [WARN] 后端 180 秒内未就绪，请查看 "Backend API (5247)" 窗口日志。
)
call :wait_port 5173 120
if errorlevel 1 (
  echo [WARN] 前端 120 秒内未就绪，请查看 "Frontend Vite (5173)" 窗口日志。
  pause
  exit /b 1
)

echo.
echo ============================================================
echo   启动完成：
echo     前端页面 : http://127.0.0.1:5173/
echo     后端接口 : http://127.0.0.1:5247/
echo     接口文档 : http://127.0.0.1:5247/swagger
echo ============================================================
start "" "http://127.0.0.1:5173/"
echo 浏览器已打开，服务在各自的窗口中继续运行。
echo 按任意键关闭本启动窗口 ...
pause >nul
exit /b 0


:: ============================================================
::                       子过程 / Subroutines
:: ============================================================

:ensure_node
where node >nul 2>nul
if not errorlevel 1 goto :node_check_version
if exist "%NODE_DIR%\node.exe" set "PATH=%NODE_DIR%;%PATH%"
where node >nul 2>nul
if not errorlevel 1 goto :node_check_version

echo     未检测到 Node.js，尝试通过 winget 自动安装 ...
where winget >nul 2>nul
if errorlevel 1 goto :node_no_winget
winget install --id OpenJS.NodeJS.LTS -e --silent --accept-package-agreements --accept-source-agreements
if exist "%NODE_DIR%\node.exe" set "PATH=%NODE_DIR%;%PATH%"
where node >nul 2>nul
if not errorlevel 1 goto :node_check_version
echo     [ERROR] winget 安装 Node.js 未成功。
exit /b 1

:node_no_winget
echo     [ERROR] 未找到 winget，无法自动安装 Node.js。
echo     请手动安装 LTS 版本后重试：https://nodejs.org/zh-cn/download
exit /b 1

:node_check_version
for /f "delims=" %%v in ('node -v') do set "NODE_VER=%%v"
for /f "tokens=1 delims=." %%a in ("!NODE_VER:v=!") do set "NODE_MAJOR=%%a"
echo     Node.js 已就绪：!NODE_VER!
if !NODE_MAJOR! LSS 18 (
  echo     [WARN] Vite 5 需要 Node.js 18 及以上，当前为 !NODE_VER!，建议升级。
)
exit /b 0


:ensure_dotnet
if exist "%DOTNET_DIR%\dotnet.exe" set "PATH=%DOTNET_DIR%;%PATH%"
where dotnet >nul 2>nul
if errorlevel 1 goto :dotnet_install
dotnet --list-sdks 2>nul | findstr /r "^[89]. ^1[0-9]." >nul
if not errorlevel 1 goto :dotnet_ok
echo     已有 .NET 运行时，但缺少 .NET 8 SDK，尝试自动安装 ...
goto :dotnet_install

:dotnet_ok
for /f "delims=" %%v in ('dotnet --version 2^>nul') do set "DOTNET_VER=%%v"
echo     .NET SDK 已就绪：!DOTNET_VER!
exit /b 0

:dotnet_install
echo     未检测到 .NET 8 SDK，尝试通过 winget 自动安装 ...
where winget >nul 2>nul
if errorlevel 1 goto :dotnet_fallback
winget install --id Microsoft.DotNet.SDK.8 -e --silent --accept-package-agreements --accept-source-agreements
if exist "%DOTNET_DIR%\dotnet.exe" set "PATH=%DOTNET_DIR%;%PATH%"
dotnet --list-sdks 2>nul | findstr /r "^[89]. ^1[0-9]." >nul
if not errorlevel 1 goto :dotnet_ok
echo     winget 安装未生效，改用官方脚本 ...
goto :dotnet_fallback

:dotnet_fallback
powershell -NoProfile -ExecutionPolicy Bypass -Command "& { $u='https://dot.net/v1/dotnet-install.ps1'; $p=Join-Path $env:TEMP 'dotnet-install.ps1'; Invoke-WebRequest $u -OutFile $p -UseBasicParsing; & $p -Channel 8.0 }"
set "PATH=%LOCALAPPDATA%\Microsoft\dotnet;%PATH%"
dotnet --list-sdks 2>nul | findstr /r "^[89]. ^1[0-9]." >nul
if not errorlevel 1 goto :dotnet_ok
echo     [ERROR] 自动安装 .NET 8 SDK 失败。
echo     请手动安装后重试：https://dotnet.microsoft.com/zh-cn/download/dotnet/8.0
exit /b 1


:: 判断端口是否已在 IPv4 回环/全部网卡上监听（返回 0 = 已监听）
:port_listening
netstat -ano | findstr /c:"127.0.0.1:%~1 " >nul
if not errorlevel 1 exit /b 0
netstat -ano | findstr /c:"0.0.0.0:%~1 " >nul
if not errorlevel 1 exit /b 0
exit /b 1


:: 等待端口就绪，参数1=端口 参数2=最大秒数
:wait_port
set "WP_PORT=%~1"
set "WP_TRIES=%~2"
for /l %%i in (1,1,%WP_TRIES%) do (
  call :port_listening %WP_PORT%
  if not errorlevel 1 exit /b 0
  timeout /t 1 /nobreak >nul
)
exit /b 1
