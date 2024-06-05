dotnet new uninstall xarial.xcad.templates
dotnet pack "%~dp0..\templates\Xarial.XCad.Templates.csproj"
dotnet new install xarial.xcad.templates --nuget-source "%~dp0../templates/bin/Release" --force