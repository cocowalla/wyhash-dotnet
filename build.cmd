dotnet restore .\wyhash-dotnet.sln
dotnet build .\src\WyHash\WyHash.csproj --configuration Release -p:EnableSourceLink=true

dotnet test .\test\UnitTests\UnitTests.csproj

dotnet pack .\src\WyHash\WyHash.csproj -c Release
