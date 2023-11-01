@echo off
:start0
cd /d %~dp0..\..
set pt=%cd%
cd /d %~dp0

set pt=%pt%\PowerToys
title PT Source Cleaner - [new: Newest Version] [x / exit: Exit]
echo 扫描到的 大力玩具 源码目录：
for /d %%a in (%pt%-*) do set i=%%a&call :finddir
echo.
set ver=
set /p ver=请输入版本：
title PT Source Cleaner
if test"%ver%"==test"x" goto :EOF
if test"%ver%"==test"exit" goto :EOF
if test"%ver%"==test"new" set ver=%a%
if test"%ver%"==test"" cls&goto :start0

call :title
echo PowerToys Src [%ver%]
@echo on

rmdir /s /q %pt%-%ver%\.vs
rmdir /s /q %pt%-%ver%\packages
rmdir /s /q %pt%-%ver%\x64
rmdir /s /q %pt%-%ver%\x86
rmdir /s /q %pt%-%ver%\ARM64
REM rmdir /s /q %pt%-%ver%\deps\cxxopts
rmdir /s /q %pt%-%ver%\deps\cziplib
rmdir /s /q %pt%-%ver%\deps\expected-lite
rmdir /s /q %pt%-%ver%\deps\spdlog
REM mkdir %pt%-%ver%\deps\cxxopts
mkdir %pt%-%ver%\deps\cziplib
mkdir %pt%-%ver%\deps\expected-lite
mkdir %pt%-%ver%\deps\spdlog

for /f "delims=" %%a in ('%~dp0bin\es -path %pt%-%ver% wfn:folder:bin') do rmdir /s /q "%%a"
for /f "delims=" %%a in ('%~dp0bin\es -path %pt%-%ver% wfn:folder:obj') do rmdir /s /q "%%a"
for /f "delims=" %%a in ('%~dp0bin\es -path %pt%-%ver% wfn:folder:x64') do rmdir /s /q "%%a"
::for /f "delims=" %%a in ('%~dp0bin\es -path %pt%-%ver% wfn:folder:Win32') do rmdir /s /q "%%a"
:: Plugin's Win32 Folder will be cleaned also!!!
for /f "delims=" %%a in ('%~dp0bin\es -path %pt%-%ver% wfn:folder:"Generated Files"') do rmdir /s /q "%%a"
for /f "delims=" %%a in ('%~dp0bin\es -path %pt%-%ver% ext:user') do del /f /q "%%a"

::del /f /q "%pt%-%ver%\src\modules\imageresizer\dll\ImageResizerExt_i.c"
::del /f /q "%pt%-%ver%\src\modules\imageresizer\dll\ImageResizerExt_i.h"
::del /f /q "%pt%-%ver%\src\modules\imageresizer\dll\RCa11568"
rmdir /s /q %pt%-%ver%\src\modules\videoconference\VideoConferenceProxyFilter\Release
rmdir /s /q %pt%-%ver%\src\modules\videoconference\VideoConferenceProxyFilter\Debug
rmdir /s /q %pt%-%ver%\src\modules\videoconference\VideoConferenceShared\Release
rmdir /s /q %pt%-%ver%\src\modules\videoconference\VideoConferenceShared\Debug
rmdir /s /q %pt%-%ver%\src\common\version\Release
rmdir /s /q %pt%-%ver%\src\common\version\Debug

@echo off
pause
:goto :eof

:finddir
echo ·%i%
set i=%i:~11%
if "%i:~27,1%"=="-" goto :eof
if "%i:~27,1%"==" " goto :eof
REM if "%i:~-5%"=="-orig" goto :eof
REM if "%i:~-9%"=="-ORIGINAL" goto :eof
REM if "%i:~-8%"=="-fullbak" goto :eof
REM if "%i:~-11%"=="-FULLBACKUP" goto :eof
REM if "%i:~-8%"=="-FULLBAK" goto :eof
REM if "%i:~-6%"=="-patch" goto :eof
REM if "%i:~-6%"=="-PATCH" goto :eof
set i=%i:~21%
set a=%i%
echo [NEW] -- %a%
goto :EOF

:title
title PT Source Cleaner - %ver%
if test"%ver%"==test"%a%" title PT Source Cleaner - %ver% [Newest]
if not exist "%pt%-%ver%" title PT Source Cleaner - %ver% [Not exist!]
goto :EOF