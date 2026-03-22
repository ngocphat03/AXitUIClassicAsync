#!/bin/bash

function usage() {
    echo "Usage: ./create_ui.sh -n <Name> -t <Type> [-o <OutDir>]"
    echo "  -n, --name     Name of the UI (e.g., Settings)"
    echo "  -t, --type     Type of the UI (Screen or Popup)"
    echo "  -o, --outdir   Output directory (default: .)"
    exit 1
}

NAME=""
TYPE=""
OUTDIR="."

while [[ "$#" -gt 0 ]]; do
    case $1 in
        -n|--name) NAME="$2"; shift ;;
        -t|--type) TYPE="$2"; shift ;;
        -o|--outdir) OUTDIR="$2"; shift ;;
        *) usage ;;
    esac
    shift
done

if [[ -z "$NAME" || -z "$TYPE" ]]; then
    usage
fi

if [[ "$TYPE" != "Screen" && "$TYPE" != "Popup" ]]; then
    echo "Error: Type must be either 'Screen' or 'Popup'"
    exit 1
fi

mkdir -p "$OUTDIR"

MODEL="using AxitUnityTemplate.UI.Classic.Async;\n\npublic class ${NAME}Model : Base${TYPE}Model\n{\n}\n"
VIEW="using AxitUnityTemplate.UI.Classic.Async;\n\npublic class ${NAME}View : Base${TYPE}View\n{\n}\n"
PRESENTER="using AxitUnityTemplate.UI.Classic.Async;\nusing Cysharp.Threading.Tasks;\n\npublic class ${NAME}Presenter : Base${TYPE}Presenter<${NAME}View, ${NAME}Model>\n{\n    public override string ${TYPE}Path => \"UI/${NAME}View\";\n\n    public override void Awake() { }\n    public override void OnEnable() { }\n    public override void OnDisable() { }\n    public override void OnDestroy() { }\n}\n"

SCRIPT_CONTENT="$MODEL\n$VIEW\n$PRESENTER"
FINAL_PATH="${OUTDIR}/${NAME}View.cs"

echo -e "$SCRIPT_CONTENT" > "$FINAL_PATH"
echo "Success: Created ${NAME}View.cs in $FINAL_PATH"
