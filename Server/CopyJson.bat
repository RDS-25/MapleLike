@echo off
setlocal

set "currentDir=%~dp0"

rem Maps.Json을 복사합니다.
set "sourcePath=%currentDir%ServerContents\Room\Data\Maps.Json"
set "destinationPath=%currentDir%ServerContents\bin\Debug\net8.0"

if exist "%sourcePath%" (
    xcopy "%sourcePath%" "%destinationPath%" /Y
    echo 파일이 성공적으로 복사되었습니다: %sourcePath% -> %destinationPath%
) else (
    echo 오류: 출발 파일이 존재하지 않습니다: %sourcePath%
)

rem Monsters.Json을 복사합니다.
set "sourcePath=%currentDir%ServerContents\Room\Data\Monsters.Json"
if exist "%sourcePath%" (
    xcopy "%sourcePath%" "%destinationPath%" /Y
    echo 파일이 성공적으로 복사되었습니다: %sourcePath% -> %destinationPath%
) else (
    echo 오류: 출발 파일이 존재하지 않습니다: %sourcePath%
)

rem PlayerSpawnPositions.Json을 복사합니다.
set "sourcePath=%currentDir%ServerContents\Room\Data\PlayerSpawnPositions.Json"
if exist "%sourcePath%" (
    xcopy "%sourcePath%" "%destinationPath%" /Y
    echo 파일이 성공적으로 복사되었습니다: %sourcePath% -> %destinationPath%
) else (
    echo 오류: 출발 파일이 존재하지 않습니다: %sourcePath%
)

pause
endlocal
