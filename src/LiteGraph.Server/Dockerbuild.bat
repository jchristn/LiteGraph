@ECHO OFF
IF "%1" == "" GOTO :Usage
IF "%2" == "" GOTO :Usage
ECHO.
ECHO Building for linux/amd64 and linux/arm64/v8...
docker buildx build -f Dockerfile --platform linux/amd64,linux/arm64/v8 --tag jchristn/litegraph:%1 --load .

IF "%2" NEQ "1" GOTO :Done

ECHO.
ECHO Pushing image...
docker push jchristn/litegraph:%1

GOTO :Done

:Usage
ECHO.
ECHO Provide two arguments; the first being the version and the second being 1 or 0 to indicate whether or not to push.
ECHO Example: dockerbuild.bat v2.0.0 1

:Done
ECHO.
ECHO Done
@ECHO ON
