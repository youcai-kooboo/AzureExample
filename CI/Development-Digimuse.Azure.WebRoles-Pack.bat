set checkoutDir=%1

echo "Packing Digimuse.Azure.WebRoles"
"C:\Program Files (x86)\MSBuild\12.0\bin\MSBuild.exe" WindowsAzure1.sln /t:"7_ Azure\Digimuse_Azure_WebRoles":Publish /p:Configuration=Release;TargetProfile=Release;PublishDir=%checkoutDir%\src\CI\Packages\Release\Digimuse.Azure.WebRoles\
echo "Digimuse.Azure.WebRoles.cspkg has been created"