
param(
    [string]$nuget_path= $("C:\nuget")
    )
    
	wget "https://raw.githubusercontent.com/rducom/ALM/master/build/ComputeVersion.ps1" -outfile "ComputeVersion.ps1"
    . .\ComputeVersion.ps1 
    
    $version = Compute .\Web\RDD.Web\RDD.Web.csproj
    $props = "/p:Configuration=Debug,VersionPrefix="+($version.Prefix)+",VersionSuffix="+($version.Suffix)
    $propack = "/p:PackageVersion="+($version.Semver) 
 
    dotnet restore
    dotnet build .\RDD.sln $props
    dotnet pack .\Domain\RDD.Domain\RDD.Domain.csproj --configuration Debug $propack -o $nuget_path
    dotnet pack .\Infra\RDD.Infra\RDD.Infra.csproj --configuration Debug $propack -o $nuget_path
    dotnet pack .\Web\RDD.Web\RDD.Web.csproj --configuration Debug $propack -o $nuget_path
 