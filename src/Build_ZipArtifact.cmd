::@ECHO OFF
SETLOCAL ENABLEEXTENSIONS

::Boilderplate 
::detect if invoked via Window Explorer
SET interactive=1
ECHO %CMDCMDLINE% | FIND /I "/c" >NUL 2>&1
IF %ERRORLEVEL% == 0 SET interactive=0

::name of this script
SET me=%~n0
::directory of script
SET parent=%~dp0
SET zip="%parent%tools\zip.cmd"

IF NOT DEFINED dateCode (SET dateCode=%date:~10,4%_%date:~4,2%_%date:~7,2%)
IF NOT DEFINED artifactsDir (SET artifactsDir=%parent%/../Artifacts/%dateCode%/)

set outFile=%artifactsDir%CruiseProcessing_%dateCode%.zip

cd %parent%CruiseProcessing\bin\Release\net472

call %parent%Build_Release.cmd 

IF /I "%ERRORLEVEL%" NEQ "0" (
ECHO build failed
IF "%interactive%"=="0" PAUSE
EXIT /B 1
) 

call %zip% a -tzip -spf %outFile%  CruiseProcessing.exe CruiseProcessing.exe.config runtimes\win-x86\native\*.dll runtimes\win-x64\native\*.dll *.dll

::if invoked from windows explorer, pause
IF "%interactive%"=="0" PAUSE
ENDLOCAL
EXIT /B 0