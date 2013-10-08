set msBuildDir=%WINDIR%\Microsoft.NET\Framework\v4.0.30319

call %msBuildDir%\msbuild /target:Rebuild /property:Configuration=Release "FBReader.sln" 

set msBuildDir=