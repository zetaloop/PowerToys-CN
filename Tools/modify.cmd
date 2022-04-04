@echo off
:start0
cd /d %~dp0..
set pt=%cd%
cd /d %~dp0

set pt=%pt%\PowerToys
title PT Source Patcher - [new: Newest Version] [x / exit: Exit]
echo 扫描到的 大力玩具 源码目录：
for /d %%a in (%pt%-*) do set i=%%a&call :finddir
echo.
set ver=
set /p ver=请输入版本：
title PT Source Patcher
if test"%ver%"==test"x" goto :EOF
if test"%ver%"==test"exit" goto :EOF
if test"%ver%"==test"new" set ver=%a%
if test"%ver%"==test"" cls&goto :start0

call :title
echo PowerToys Src [%ver%]
@echo on

copy /y %pt%-%ver%\x86\Release\modules\VideoConference\*_x86* %pt%-%ver%\x64\Release\modules\VideoConference\
mkdir %pt%-%ver%\x64\Release\modules\launcher\Plugins\VSCodeWorkspace\ 2>nul
copy /y %pt%-%ver%\x64\Release\modules\launcher\Plugins\VSCodeWorkspaces %pt%-%ver%\x64\Release\modules\launcher\Plugins\VSCodeWorkspace\

@echo off
for /f "delims=" %%a in (zhcn_modify_list.txt) do set zhcn=%%a&call :zhcn 
@echo on

@echo off
pause
:goto :eof

:finddir
echo ·%i%
set i=%i:~15%
if "%i:~-9%"=="-ORIGINAL" goto :eof
set a=%i%
goto :EOF

:title
title PT Source Cleaner - %ver%
if test"%ver%"==test"%a%" title PT Source Cleaner - %ver% [Newest]
if not exist "%pt%-%ver%" title PT Source Cleaner - %ver% [Not exist!]
goto :EOF

:zhcn
set zhcn=%pt%-%ver%\x64\Release\%zhcn%
for /f "delims=" %%a in ('echo %zhcn%') do set zhcnd=%%~dpa&set zhcnn=%%~na&set zhcnx=%%~xa
mkdir %zhcnd%zh-CN 2>nul
@echo on
copy /y blank %zhcnd%zh-CN\%zhcnn%.resources%zhcnx%
@echo off
goto :EOF