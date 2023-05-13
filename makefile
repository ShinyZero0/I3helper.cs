run-ipc:
	-pkill I3IPC
	dotnet run --project ./I3IPC/

run-client:
	dotnet run --project ./I3Client/

sln:
	-pkill I3IPC 
	-pkill I3Client
	rm -r ./out
	dotnet publish -c Release ./I3IPCsln.sln -o ./out
