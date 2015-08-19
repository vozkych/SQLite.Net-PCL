set version=3.7.1

@mkdir output
del output\*.*
nuget pack SQLite.Net.nuspec -o output -Version %version%
nuget pack SQLite.Net.Async.nuspec -o output -Version %version%
nuget pack SQLite.Net.Platform.Win32.nuspec -o output -Version %version%
nuget pack SQLite.Net.Platform.XamarinAndroid.nuspec -o output -Version %version%
nuget pack SQLite.Net.Platform.XamarinIOS.nuspec -o output -Version %version%

nuget push output\*.nupkg -Source http://nugets.vapolia.fr/
pause

