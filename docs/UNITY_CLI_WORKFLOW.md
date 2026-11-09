# Unity CLI Workflow

Verified locally: 2026-11-10, Unity 6000.0.80f1.

## Installed Tools

- CLI: `C:/Program Files/Unity Hub/resources/unity.exe`, version 1.0.0-beta.13.
  It is installed but is not in this agent terminal's PATH. Use the absolute
  path; do not reinstall or upgrade to solve a PATH-only issue.
- Project already includes `com.unity.pipeline` 0.9.1-exp.1. No package/Editor
  upgrade was made for BUG-019.
- Scope commands to this project's path, especially when another Editor is open.

```powershell
$unity = 'C:\Program Files\Unity Hub\resources\unity.exe'
& $unity command --project-path 'C:\Zlozka\MegaTreskKarty' editor status --json
& $unity command --project-path 'C:\Zlozka\MegaTreskKarty' script recompile --json
& $unity command --project-path 'C:\Zlozka\MegaTreskKarty' script recompile_status --json
& $unity command --project-path 'C:\Zlozka\MegaTreskKarty' test run --mode playmode --async_tests --json
& $unity command --project-path 'C:\Zlozka\MegaTreskKarty' test status --json
```

## Safety And Result Ownership

- Check real Editor process/project lock before launching. Never launch a second
  Editor against an open project or close the user's Editor to run tests.
- Pipeline can run tests against the existing Editor. PlayMode tests must use
  `--async_tests`; synchronous HTTP tests are rejected because domain reload
  drops the request. Poll until completion, not merely until the request succeeds.
- Compilation briefly restarts Pipeline. Wait for the same project to reconnect;
  a short missing-server interval is not a reason to start a second Editor.
- Read the nested result's success/status too: CLI process exit 0 alone does not
  prove an in-memory script compiled or a test run passed. Inspect diagnostics,
  total/pass/fail and the completed result in `Temp/pipeline_test_status.json`.
- Copy test evidence outside the repository before the next run overwrites it.
  Do not claim an XML run when the evidence is Pipeline's JSON result.
- GUI/scene mutations still require explicit product approval. Author through
  UnityEditor serialized objects, PrefabUtility and scene APIs, saving only named
  scenes/assets. Do not rewrite YAML or edit generated csproj/package-cache files.
- For temporary bulk authoring, use an inspected script outside Assets via
  `script run --file Temp/Example.cs --entry Example.Apply`. Remove the one-off
  script after verification; do not ship a runtime hotpatch or debug switch.
- Keep live service calls, authentication, credentials and unrelated loaded
  scenes out of visual previews. Never print Pipeline descriptor/auth tokens.

## Overlay Screenshot Limitation

The installed Pipeline `capture screenshot` command renders a camera into a
RenderTexture. It does not include ScreenSpaceOverlay canvases in that render;
a plain camera-background image is not evidence that the UI is blank in-game.
BUG-019 visual QA used a temporary, unsaved ScreenSpaceCamera preview for the
authored canvas, with the camera/mode restored afterward, and checked rendered
PNGs plus TMP overflow at 540x1200 and 540x960. No saved canvas/render setting
was changed for screenshot capture. Device smoke remains separate acceptance.

References: [Unity CLI/Pipeline](https://docs.unity.com/en-us/unity-cli/unity-pipeline/unity-pipeline-package),
[CLI usage](https://docs.unity.com/en-us/unity-cli/use-unity-cli).
