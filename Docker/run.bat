@echo off

IF "%1" == "" GOTO :Usage

if not exist litegraph.json (
  echo Configuration file litegraph.json not found.
  exit /b 1
)

REM Items that require persistence
REM   litegraph.json
REM   litegraph.db
REM   logs/

REM Argument order matters!

docker run ^
  -p 8701:8701 ^
  -t ^
  -i ^
  -e "TERM=xterm-256color" ^
  -v .\litegraph.json:/app/litegraph.json ^
  -v .\litegraph.db:/app/litegraph.db ^
  -v .\logs\:/app/logs/ ^
  jchristn/litegraph:%1

GOTO :Done

:Usage
ECHO Provide one argument indicating the tag. 
ECHO Example: dockerrun.bat v2.0.16
:Done
@echo on
