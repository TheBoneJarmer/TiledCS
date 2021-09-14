$branch = git branch --show-current;

Remove-Item *.nupkg
dotnet pack -c Release -o . ./src/TiledCS.csproj

if ($branch -eq "develop")
{
    dotnet nuget push -s github -k $env:GITHUB_API_KEY *.nupkg
}
elseif ($branch -eq "main")
{
    dotnet nuget push -s nuget.org -k $env:NUGET_API_KEY *.nupkg
}
else
{
    echo "Can only publish nuget package on develop or main branch";
}
