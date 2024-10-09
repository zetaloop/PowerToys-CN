chcp 65001>nul
@echo off
echo 正在安装 PowerToys 组策略...
%1 mshta vbscript:CreateObject("Shell.Application").ShellExecute("cmd.exe","/c %~s0 ::","","runas",1)(window.close)&&goto :eOF
cd /d %~dp0
xcopy /Y /I /E ".\PowerToys.admx" "%systemroot%\PolicyDefinitions\"
xcopy /Y /I /E ".\zh-CN\PowerToys.adml" "%systemroot%\PolicyDefinitions\zh-CN\"
xcopy /Y /I /E ".\en-US\PowerToys.adml" "%systemroot%\PolicyDefinitions\en-US\"
msg %username% 安装完成