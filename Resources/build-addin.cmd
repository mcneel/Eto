echo off

rem set MSBUILD=c:\Windows\Microsoft.NET\Framework\v4.0.30319\MSBuild.exe
set MSBUILD="%ProgramFiles(x86)%\MSBuild\14.0\bin\MSBuild.exe"

%MSBUILD% -t:BuildAddins Build.proj

pause