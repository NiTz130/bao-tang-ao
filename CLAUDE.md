# CLAUDE.md

This file provides guidance to Claude Code (claude.ai/code) when working with code in this repository.

## Project

- Unity **6000.3.26f1** project (upgraded from 6000.3.24f1), created from the URP (Universal Render Pipeline) template.
- A virtual heritage museum: a first-person walk through a hall with photogrammetry scans of Vietnamese heritage artifacts (Cham sculpture, ceramics, Nguyen dynasty monuments). Walking near an exhibit shows a Vietnamese info panel; the Interact action (E) expands the story.
- Main scene: `Assets/Museum/Scenes/Museum.unity`. It is the only enabled scene in Build Settings. `Assets/Scenes/SampleScene.unity` is the template scene, kept but disabled.
- Code: `Assets/Museum/Scripts` (assembly `Museum.Runtime`). Content: `Assets/Museum/Content` (one `ExhibitInfo` asset per exhibit, generated from the Vietnamese narration JSON). Models: `Assets/Museum/Models/<Exhibit>/`, each with a `SOURCE.txt` that gives the author, the licence and the scan status.
- Git repository, branch `main`.
- Docs: `Docs/narration_vi.json` (nguồn lời thuyết minh), `Docs/plans/` (kế hoạch), `Docs/scene_wiring/fx_*.cs.txt` (đoạn mã nối cảnh, chạy lại được nhiều lần).

## Architecture

`NarrativeDirector` (on the Player) is the hub. It reads the distance from `player` to each `NarrativeAnchor` (one per exhibit, holds an `ExhibitInfo` asset and the spot lights to highlight). It shows or hides `ExhibitInfoPanel` and handles the Interact action (E) and Esc.

- `ExhibitTargeting` raycasts from the camera (4 m), drives the crosshair, and boosts the spot light of the aimed exhibit.
- `ExhibitInspector` is optional. When E hits an aimed exhibit, it lifts the exhibit in front of the camera, spins it, turns on `InspectLight` and `InspectVolume` (depth of field + vignette), and locks `FirstPersonController`. Exit restores the exact previous pose.
- `ScreenFader` fades the screen around state changes.
- Pure logic is split into `NarrativeAnchorLogic` and `InspectMath` so EditMode tests can cover it. Components link to each other through `[SerializeField]` fields wired in the scene (`MuseumSceneWiringTests` checks the wiring). A new component needs wiring in `Museum.unity`, not only code.

## How to work in this project

Use the **UnityMCP** server (package `com.coplaydev.unity-mcp`) for all editor work. The Unity Editor must be open with this project. Read `mcpforunity://instances` to get the instance name. The hash can change after an editor upgrade. The MCP HTTP server listens on `127.0.0.1:8080`. If the server is down, start it in Unity (Window → MCP for Unity → Start Server), then run `/mcp` to reconnect.

Rules:

- The main model (Opus 5.5, medium effort) plans, dispatches, and verifies. Sonnet 5.5 subagents (`model: "sonnet"`, high effort) do the hands-on Unity work.
- Give every task at least one subagent. Split a task that is too large for one agent into smaller tasks.
- Give **one task to one subagent**. Do not give a subagent several jobs. Run independent tasks in parallel. Run dependent tasks in sequence.
- Do not let two parallel agents edit the same scene or asset.
- Check work through UnityMCP before you report it as done. Read the object back (`find_gameobjects`, `mcpforunity://scene/gameobject/{id}`) and read the console (`read_console`, types `error` and `warning`).
- After you edit scripts, wait for compilation to finish. Check `mcpforunity://editor/state` (`data.advice.ready_for_tools`) before you call more tools.

## Commands

There is no build or lint script. Use UnityMCP tools:

| Task | Tool |
|---|---|
| Editor state, compile status | resource `mcpforunity://editor/state` |
| Run tests (EditMode or PlayMode) | `run_tests`, then `get_test_job` |
| Read compile and runtime errors | `read_console` |
| Build a player | `manage_build` |

Tests use the Unity Test Framework (`com.unity.test-framework` 1.6.0). They are in `Assets/Museum/Tests/EditMode` (`Museum.Tests.EditMode`) and `Assets/Museum/Tests/PlayMode` (`Museum.Tests.PlayMode`, which also references `Unity.InputSystem.TestFramework`). The PlayMode test `Director_MissingWiring_LogsErrorAndDisables` logs the error "NarrativeDirector: player, panel and actions must be assigned." on purpose.

To run tests from the command line, first close the Editor. Unity cannot open the same project twice.

```
"C:\Program Files\Unity\Hub\Editor\6000.3.26f1\Editor\Unity.exe" -batchmode -projectPath C:\code\mon -runTests -testPlatform EditMode -testResults results.xml
```

To run one test, add `-testFilter <FullTestName>`.

Use 6000.3.26f1. Do not use 6000.3.24f1: its Windows 64-bit release player (`win64_player_nondevelopment_mono`) had no `MonoBleedingEdge` folder. A player built with it reported "Success" but could not start. Unity Hub also has 6000.6.2f1 installed. Do not use it.

Player builds go to `Builds/` (release: `Builds/Windows/VirtualMuseum.exe`). After a build, run the player and check its log (`-logFile`). A build that reports "Success" does not prove the player starts.

## Settings to know

- **Input:** only the new Input System is active (`activeInputHandler: 1`). Do not use `UnityEngine.Input`. Actions are in `Assets/InputSystem_Actions.inputactions`.
- **Rendering:** URP 17.3 with two quality tiers. Each tier has its own pipeline asset and renderer: `Assets/Settings/PC_*` and `Assets/Settings/Mobile_*`. A rendering change must go into both tiers. Color space is Linear.
- **Scripts:** museum code compiles into `Museum.Runtime` (it references `Unity.InputSystem`, `Unity.TextMeshPro` and `UnityEngine.UI`). Other scripts compile into the default `Assembly-CSharp`.
- **Layout:** a rotunda (r 8, skylight, Tự Đức stele at the centre) with a vestibule to the south and three rooms through arched corridors: West = Cham sculpture, North = ceramics, East = Nguyễn tombs. Coordinates are in `Docs/plans/2026-10-16-rotunda-rebuild.md`; `MuseumLayoutTests` checks rooms, walkability, targeting, titles and probe coverage. Decor is under the `Decor` root, never has a `NarrativeAnchor`, and is not static.
- **Lighting:** the ceiling area lights (`Lighting/Area_*`) are Baked, and the spot light on each exhibit is Realtime. Scan meshes and decor are not lightmap-static; light probes light them. After you move the architecture or the lights, bake again. Poll `Lightmapping.isRunning` from separate calls; `Thread.Sleep` blocks the main thread and stalls the bake. The lighting settings are in `Assets/Museum/Scenes/Museum_LightingSettings.lighting`.
- **Generated meshes** (`Assets/Museum/Decor/Meshes/`): check the triangle winding after you create one (cross product of each triangle against its vertex normals). Inverted meshes render black, even under realtime lights.
- **Known editor issues:** after you delete reflection probes, URP keeps stale probe data and renders white with a `ReflectionProbeManager` NullReference; reopen the scene. Many `Camera.Render` calls in a row have crashed the editor (D3D12 fence error); render at most two shots per call.
- **Vietnamese text:** use the TMP font asset made from Be Vietnam Pro. LiberationSans has no Vietnamese diacritics.
- **Generated files:** `*.csproj` and `*.sln` are generated by Unity. Do not edit them. Do not edit `Library/`, `Temp/`, or `Logs/` either.
