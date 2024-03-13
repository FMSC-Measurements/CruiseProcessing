SET sevenZip=%programW6432%\7-Zip\7z.exe

if exist "%sevenZip%" (
  "%sevenZip%" %*
)