run:
	# what you need is:
	# -pkill I3Helper
	# while this is for runit(8):
	-sv down ~/.local/share/service-graphic/i3helper
	dotnet run --project ./src/ -- 13131

sln:
	-pkill I3Helper
	-rm -r ./out
	dotnet publish -c Release ./I3Helper.sln -o ./out
