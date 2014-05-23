$subscription = "Didix development"
$storage = "didix0dev"
$service = "didix-development"
$slot = "Production"
$package = "CI\Packages\Release\Digimuse.Azure.WebRoles\Digimuse.Azure.WebRoles.cspkg"
$packageConfiguration = "CI\Packages\Release\Digimuse.Azure.WebRoles\ServiceConfiguration.Digimuse.cscfg"
$publishSettings = "C:\TeamCity\Digimuse.publishsettings"
$timeStampFormat = "g"
$deploymentLabel = "TeamCity Digimuse.Azure.WebRoles v%build.number%"
 
Write-Output "Running Azure Imports"
Import-Module Azure
Import-AzurePublishSettingsFile $publishSettings
Set-AzureSubscription -SubscriptionName $subscription -CurrentStorageAccount $storage
 
function Publish(){
 $deployment = Get-AzureDeployment -ServiceName $service -Slot $slot -ErrorVariable a -ErrorAction silentlycontinue 
 
 if ($a[0] -ne $null) {
    Write-Output "$(Get-Date -f $timeStampFormat) - No deployment is detected. Creating a new deployment. "
 }
 
 if ($deployment.Name -ne $null) {
    Write-Output "$(Get-Date -f $timeStampFormat) - Deployment exists in $servicename.  Upgrading deployment."
    UpgradeDeployment
 } else {
    CreateNewDeployment
 }
}
 
function CreateNewDeployment()
{
    write-progress -id 3 -activity "Creating New Deployment" -Status "In progress"
    Write-Output "$(Get-Date -f $timeStampFormat) - Creating New Deployment: In progress"
 
    $opstat = New-AzureDeployment -Slot $slot -Package $package -Configuration $packageConfiguration -label $deploymentLabel -ServiceName $service
 
    $completeDeployment = Get-AzureDeployment -ServiceName $service -Slot $slot
    $completeDeploymentID = $completeDeployment.deploymentid
 
    write-progress -id 3 -activity "Creating New Deployment" -completed -Status "Complete"
    Write-Output "$(Get-Date -f $timeStampFormat) - Creating New Deployment: Complete, Deployment ID: $completeDeploymentID"
}
 
function UpgradeDeployment()
{
    write-progress -id 3 -activity "Upgrading Deployment" -Status "In progress"
    Write-Output "$(Get-Date -f $timeStampFormat) - Upgrading Deployment: In progress"
 
    $setdeployment = Set-AzureDeployment -Upgrade -Slot $slot -Package $package -Configuration $packageConfiguration -label $deploymentLabel -ServiceName $service -Force
 
    $completeDeployment = Get-AzureDeployment -ServiceName $service -Slot $slot
    $completeDeploymentID = $completeDeployment.deploymentid
 
    write-progress -id 3 -activity "Upgrading Deployment" -completed -Status "Complete"
    Write-Output "$(Get-Date -f $timeStampFormat) - Upgrading Deployment: Complete, Deployment ID: $completeDeploymentID"
}
 
Write-Output "Create Azure Deployment"
Publish