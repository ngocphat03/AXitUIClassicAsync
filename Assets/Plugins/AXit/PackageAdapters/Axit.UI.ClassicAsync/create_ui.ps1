param (
    [Parameter(Mandatory=$true)]
    [string]$Name,
    
    [Parameter(Mandatory=$true)]
    [ValidateSet("Screen","Popup")]
    [string]$Type,
    
    [Parameter(Mandatory=$false)]
    [string]$OutDir = "."
)

if (-not (Test-Path $OutDir)) {
    New-Item -ItemType Directory -Path $OutDir | Out-Null
}

$Model = "using AxitUnityTemplate.UI.Classic.Async;`n`npublic class ${Name}Model : Base${Type}Model`n{`n}`n"
$View = "using AxitUnityTemplate.UI.Classic.Async;`n`npublic class ${Name}View : Base${Type}View`n{`n}`n"
$Presenter = @"
using AxitUnityTemplate.UI.Classic.Async;
using Cysharp.Threading.Tasks;

public class ${Name}Presenter : Base${Type}Presenter<${Name}View, ${Name}Model>
{
    public override string $($Type)Path => `"UI/${Name}View`";

    public override void Awake() { }
    public override void OnEnable() { }
    public override void OnDisable() { }
    public override void OnDestroy() { }
}
"@

$ScriptContent = "$Model`n$View`n$Presenter"
$FinalPath = Join-Path -Path $OutDir -ChildPath "${Name}View.cs"
Set-Content -Path $FinalPath -Value $ScriptContent
Write-Host "Success: Created ${Name}View.cs in $FinalPath"
