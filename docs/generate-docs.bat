@echo off
set API_SPEC=openapi.json
set OUTPUT_FILE=index.html


echo Generating z %API_SPEC%...
redoc-cli bundle %API_SPEC% -o %OUTPUT_FILE%

if %ERRORLEVEL% EQU 0 (
    echo Generated: %OUTPUT_FILE%
) else (
    echo Error
)
pause