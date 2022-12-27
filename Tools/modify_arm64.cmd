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

mkdir %pt%-%ver%\ARM64\Release\modules\launcher\Plugins\VSCodeWorkspace\ 2>nul
copy /y %pt%-%ver%\ARM64\Release\modules\launcher\Plugins\VSCodeWorkspaces %pt%-%ver%\ARM64\Release\modules\launcher\Plugins\VSCodeWorkspace\

@echo off
for /f "delims=" %%a in (zhcn_modify_list.txt) do set zhcn=%%a&call :zhcn
@echo on

@echo off
for /f "delims=" %%a in (zhcn_modify_list2.txt) do set zhcn=%%a&call :zhcn2
@echo on

@echo off
pause
:goto :eof

:finddir
echo ·%i%
set i=%i:~15%
if "%i:~-9%"=="-ORIGINAL" goto :eof
if "%i:~-5%"=="-orig" goto :eof
if "%i:~-6%"=="-patch" goto :eof
if "%i:~-4%"=="-bak" goto :eof
set i=%i:~11%
set a=%i%
echo [NEW] -- %a%
goto :EOF

:title
title PT Source Patcher - %ver%
if test"%ver%"==test"%a%" title PT Source Patcher - %ver% [Newest]
if not exist "%pt%-%ver%" title PT Source Patcher - %ver% [Not exist!]
goto :EOF

:zhcn
set zhcn=%pt%-%ver%\ARM64\Release\%zhcn%
for /f "delims=" %%a in ('echo %zhcn%') do set zhcnd=%%~dpa&set zhcnn=%%~na&set zhcnx=%%~xa
mkdir %zhcnd%zh-CN 2>nul
@echo on
copy /y blank %zhcnd%zh-CN\%zhcnn%.resources%zhcnx%
@echo off
goto :EOF

:zhcn2
set zhcn=%pt%-%ver%\ARM64\Release\%zhcn%
for /f "delims=" %%a in ('echo %zhcn%') do set zhcnd=%%~dpa&set zhcnn=%%~na&set zhcnx=%%~xa
mkdir %zhcnd%zh-CN 2>nul
@echo on
copy /y %zhcnn%.resources%zhcnx% %zhcnd%zh-CN\%zhcnn%.resources%zhcnx%
@echo off
goto :EOF