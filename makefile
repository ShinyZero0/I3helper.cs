run:
	-pkill I3IPC
	dotnet run --project ./src/ -- 13131

sln:
	-pkill I3Helper
	-rm -r ./out
	dotnet publish -c Release ./I3Helper.sln -o ./out
