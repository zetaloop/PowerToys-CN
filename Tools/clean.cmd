@echo off
:start0
cd /d %~dp0..
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
rmdir /s /q %pt%-%ver%\deps\cxxopts
rmdir /s /q %pt%-%ver%\deps\cziplib
rmdir /s /q %pt%-%ver%\deps\expected-lite
rmdir /s /q %pt%-%ver%\deps\spdlog
mkdir %pt%-%ver%\deps\cxxopts
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
rmdir /s /q %pt%-%ver%\src\modules\videoconference\VideoConferenceProxyFilter\Win32

@echo off
pause
:goto :eof

:finddir
echo ·%i%
set i=%i:~15%
if "%i:~-9%"=="-ORIGINAL" goto :eof
if "%i:~-11%"=="-FULLBACKUP" goto :eof
if "%i:~-8%"=="-FULLBAK" goto :eof
if "%i:~-6%"=="-PATCH" goto :eof
set a=%i%
goto :EOF

:title
title PT Source Cleaner - %ver%
if test"%ver%"==test"%a%" title PT Source Cleaner - %ver% [Newest]
if not exist "%pt%-%ver%" title PT Source Cleaner - %ver% [Not exist!]
goto :EOF