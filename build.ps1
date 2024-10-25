
param(
    [string]$nuget_path= $("C:\nuget")
    )
    
	wget "https://raw.githubusercontent.com/rducom/ALM/master/build/ComputeVersion.ps1" -outfile "ComputeVersion.ps1"
    . .\ComputeVersion.ps1 
    
    $version = Compute .\Web\Rdd.Web\Rdd.Web.csproj
    $props = "/p:Configuration=Debug,VersionPrefix="+($version.Prefix)+",VersionSuffix="+($version.Suffix)
    $propack = "/p:PackageVersion="+($version.Semver) 
 
    dotnet restore
    dotnet build .\Rdd.sln $props
    dotnet pack .\Domain\Rdd.Domain\Rdd.Domain.csproj --configuration Debug $propack -o $nuget_path
    dotnet pack .\Infra\Rdd.Infra\Rdd.Infra.csproj --configuration Debug $propack -o $nuget_path
    dotnet pack .\Application\Rdd.Application\Rdd.Application.csproj --configuration Debug $propack -o $nuget_path
    dotnet pack .\Web\Rdd.Web\Rdd.Web.csproj --configuration Debug $propack -o $nuget_path
    dotnet pack .\Web\Rdd.Web.AutoMapper\RDD.Web.AutoMapper.csproj --configuration Debug $propack -o $nuget_path
    dotnet pack .\Web\Rdd.Z.EntityFramework\Rdd.Z.EntityFramework.csproj --configuration Debug $propack -o $nuget_path
 