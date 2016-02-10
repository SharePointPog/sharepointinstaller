PATH = %PATH%;C:\Program Files\Common Files\Microsoft Shared\web server extensions\12\BIN
SET SPDIR=C:\Program Files\Common Files\Microsoft Shared\web server extensions\12
SET FILENAME=Stsadm.Commands.wsp

"%SPDIR%\bin\stsadm.exe" -o retractsolution -name %FILENAME% -immediate
"%SPDIR%\bin\stsadm.exe" -o execadmsvcjobs
"%SPDIR%\bin\stsadm.exe" -o deletesolution -override -name %FILENAME%

pause
cls
"%SPDIR%\bin\stsadm.exe" -o addsolution -filename %FILENAME%
"%SPDIR%\bin\stsadm.exe" -o deploysolution -name %FILENAME% -force -allowgacdeployment -allowCasPolicies -immediate
"%SPDIR%\bin\stsadm.exe" -o execadmsvcjobs

SET URL=
pause