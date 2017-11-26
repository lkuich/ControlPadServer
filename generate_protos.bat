setlocal

cd /d %~dp0


set TOOLS_PATH=C:\Users\loren\.nuget\packages\grpc.tools\1.7.1\tools\windows_x64

set PROJECT=SocketService

%TOOLS_PATH%\protoc.exe -I %PROJECT%/protos --csharp_out %PROJECT% %PROJECT%/protos/services.proto --grpc_out %PROJECT% --plugin=protoc-gen-grpc=%TOOLS_PATH%\grpc_csharp_plugin.exe

endlocal
pause