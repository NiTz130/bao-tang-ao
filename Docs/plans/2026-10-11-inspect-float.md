# Plan: chế độ Xem với hiệu ứng lơ lửng 3D

**Người đọc:** người thực hiện code và scene trong dự án Unity `C:\code\mon` qua UnityMCP. Plan không chỉ định model.
**Mục đích:** khi người chơi nhắm vào hiện vật và nhấn E, hiện vật rời bệ và bay ra trước mặt người chơi. Hiện vật lơ lửng ở đó; người chơi xoay và phóng to được. Nhấn E hoặc Esc thì hiện vật bay về đúng chỗ cũ.
**Bản tham khảo:** chế độ "Xem" trong `C:\codev2\monsangthu7cn\game3d\js\interact.js`, dòng 117–187 và 253–268.
**Trạng thái:** đã chốt mọi quyết định (người dùng chọn "theo đề xuất" ngày 11/10/2026). Chưa có thay đổi nào trong code.

## Quyết định đã chốt

| # | Quyết định |
|---|---|
| D1 | Thoát bằng E hoặc Esc. Không dùng Q. Esc vẫn mở khóa chuột như hiện nay; click để khóa lại |
| D2 | Lơ lửng = nhấp nhô 1,2 cm, chu kỳ 3 s, cộng tự xoay chậm 12°/s. Tự xoay ngừng khi đang kéo và chạy lại 2 s sau lần kéo cuối |
| D3 | Nền làm mờ bằng Depth of Field Gaussian và vignette. Không giảm exposure, không dùng camera chồng lớp |
| D4 | Khi nhắm trúng hiện vật, E mở chế độ Xem cho hiện vật đang nhắm, kể cả khi người chơi đứng trong vùng neo của hiện vật khác |

## Sự thật đã kiểm tra trong scene (11/10/2026)

- `Player` (gốc, có `FirstPersonController`) → `CameraPivot` → `Main Camera`. Camera: FOV 65, near 0,05, đã bật post-processing.
- `NarrativeDirector` nằm trên `Narrative` (gốc).
- Bảng thông tin neo **góc dưới trái**, rộng 680/1920. Vì vậy hiện vật đặt lệch **sang phải**.
- Mỗi điểm neo có đúng một model con có Renderer:

  | Ô | Model | Cỡ (m) | Collider trên model |
  |---|---|---|---|
  | Exhibit_01 | ChamShiva | 0,51 × 0,99 × 0,66 | Không |
  | Exhibit_02 | ChamDeifiedKing | 0,48 × 1,00 × 0,51 | Không |
  | Exhibit_03 | CeladonEwer | 0,17 × 0,21 × 0,17 | Không |
  | Exhibit_04 | BottleVase | 0,14 × 0,27 × 0,14 | Không |
  | Exhibit_05 | KhaiDinhStatue | 0,46 × 1,00 × 0,46 | Không |
  | Exhibit_Centerpiece | TuDucStele | 3,40 × 2,20 × 3,35 | Có, collider đặc |

- Volume `PostProcessing` (global, priority 0) có Tonemapping, ColorAdjustments, Bloom, Vignette, WhiteBalance. Chưa có Depth of Field.
- Cả `PC_Renderer` và `Mobile_Renderer` đều có `postProcessData`.
- Action `Player/Interact` có interaction Hold. `WasPressedThisFrame()` vẫn nhận lần nhấn ngắn; test `Director_PressE_TogglesStory` đã chứng minh.
- Action có sẵn: `Player/Look`, `Player/Attack` (chuột trái). Không có action cho cuộn chuột → dùng `Mouse.current.scroll`, giống cách `FirstPersonController` dùng `Keyboard.current` cho Esc.

## 1. Executor — codebase

Làm theo thứ tự. Với mỗi bước code: viết test trước, chạy và thấy test thất bại, rồi mới viết code cho test đạt.

### Bước 1 — `Assets/Museum/Scripts/InspectMath.cs` (mới)

Lớp `public static class InspectMath` trong namespace `Museum`. Chỉ có hàm thuần, không đọc trạng thái Unity.

| Hàm | Hành vi |
|---|---|
| `float FitScale(float maxExtent, float targetSize)` | Trả về `targetSize / maxExtent`. Nếu `maxExtent <= 0` hoặc không hữu hạn, trả về 1 |
| `float Ease(float t)` | `Mathf.SmoothStep(0, 1, Mathf.Clamp01(t))` |
| `float Zoom(float current, float scrollSteps, float min = 0.6f, float max = 1.6f)` | `Mathf.Clamp(current * Mathf.Pow(1.1f, scrollSteps), min, max)`. Cuộn lên (số dương) thì phóng to |
| `float Bob(float time, float amplitude, float period)` | `amplitude * Mathf.Sin(2π · time / period)`. Nếu `period <= 0` hoặc `amplitude == 0`, trả về 0 |

**Test — `Tests/EditMode/InspectMathTests.cs`:**
- `FitScale_LargeModel_ShrinksToTarget`: (3,4; 0,45) → 0,1324 ± 0,0001.
- `FitScale_SmallModel_GrowsToTarget`: (0,21; 0,45) → 2,1429 ± 0,0001.
- `FitScale_ZeroNegativeOrNaN_ReturnsOne`: 0, -1, `float.NaN`, `float.PositiveInfinity` → 1.
- `Ease_Endpoints_AndClamp`: 0 → 0; 1 → 1; 0,5 → 0,5; -1 → 0; 2 → 1.
- `Zoom_StepsUpAndDown`: (1; 1) → 1,1; (1; -1) → 0,9091.
- `Zoom_ManySteps_StopsAtLimits`: (1; 50) → 1,6; (1; -50) → 0,6.
- `Bob_ZeroAmplitudeOrPeriod_IsZero`.
- `Bob_IsPeriodic`: `Bob(t) == Bob(t + period)` với sai số 1e-5; `Bob(period/4) == amplitude`.

### Bước 2 — `Assets/Museum/Scripts/FirstPersonController.cs` (sửa)

- Thêm `public bool Frozen { get; set; }`.
- Trong `Update`: khi `Frozen`, bỏ qua phần xoay theo Look và đặt vector di chuyển ngang bằng 0. Trọng lực vẫn tính như cũ.
- Không đổi xử lý Esc và click.

**Test — `Tests/PlayMode/FirstPersonControllerTests.cs`** (kế thừa `InputTestFixture`, theo mẫu `NarrativeDirectorInputTests`):
- `Frozen_IgnoresMoveAndLook`: giữ W và di chuột 0,3 s → vị trí x/z và góc xoay không đổi.
- `Frozen_StillAppliesGravity`: đặt người chơi lơ lửng 1 m trên mặt sàn → sau 0,5 s, y giảm.
- `NotFrozen_MovesWithW`: hồi quy; giữ W 0,3 s → z tăng.

### Bước 3 — `Assets/Museum/Scripts/ExhibitTargeting.cs` (sửa)

- Trong `OnDisable`, thêm `if (crosshair != null) crosshair.enabled = false;`.
- `Update` đã bật lại tâm ngắm; không đổi.

**Test:** thêm `Disable_HidesCrosshair` vào `ExhibitTargetingTests`. 14 test hiện có giữ nguyên.

### Bước 4 — `Assets/Museum/Scripts/ExhibitInfoPanel.cs` (sửa)

- Thêm tham số tùy chọn: `Show(ExhibitInfo info, bool expanded, string hintOverride = null)`. Khi `hintOverride` khác null, dòng gợi ý dùng chuỗi này.
- Chuỗi dùng trong chế độ Xem: `Kéo chuột trái để xoay · Cuộn để phóng to · E hoặc Esc để quay lại`.

**Test:** thêm `Show_WithHintOverride_UsesIt` và `Show_WithoutHintOverride_KeepsDefaultHint` vào `ExhibitInfoPanelTests`. 10 test hiện có và 5 test fit giữ nguyên.

### Bước 5 — `Assets/Museum/Scripts/ExhibitInspector.cs` (mới)

**Trường serialized:**

| Trường | Kiểu | Mặc định |
|---|---|---|
| `cam` | Camera | bắt buộc |
| `player` | FirstPersonController | bắt buộc |
| `targeting` | ExhibitTargeting | bắt buộc |
| `inspectLight` | Light | bắt buộc |
| `inspectVolume` | Volume | bắt buộc |
| `actions` | InputActionAsset | bắt buộc |
| `moveSeconds` | float, `Min(0)` | 0,45 (0 = tức thì) |
| `viewDistance` | float | 0,85 m |
| `viewportPoint` | Vector2 | (0,66; 0,5) |
| `viewSize` | float | 0,45 m (cạnh lớn nhất sau khi co giãn) |
| `rotateDegreesPerPixel` | float | 0,25 |
| `bobAmplitude` / `bobPeriod` | float | 0,012 m / 3 s |
| `idleSpinDegreesPerSecond` | float | 12 |
| `idleSpinDelay` | float | 2 s |
| `lightIntensity` | float | 4 (chỉnh lại sau khi nhìn tận mắt) |

**API công khai:** `bool Enter(NarrativeAnchor anchor)`, `void Exit()`, `bool IsActive` (Phase khác Idle), `InspectPhase Phase` (enum `Idle, Entering, Viewing, Leaving`), `NarrativeAnchor Anchor`.

**OnEnable:** thiếu một trong 6 tham chiếu bắt buộc thì ghi `LogError("ExhibitInspector: cam, player, targeting, inspectLight, inspectVolume and actions must be assigned.")` và tắt component. Lấy `Player/Look` và `Player/Attack`. Đặt `inspectLight.enabled = false` và `inspectVolume.weight = 0`.

**Enter(anchor):**
1. Trả về `false` khi: Phase khác Idle; anchor null; không có con trực tiếp nào chứa Renderer.
2. Model = con trực tiếp đầu tiên của anchor có Renderer trong cây con.
3. Lưu tư thế gốc: `parent`, `localPosition`, `localRotation`, `localScale`. Lưu danh sách collider đang bật trên model, rồi tắt chúng.
4. Tính `fit = InspectMath.FitScale(maxExtent, viewSize)`. `maxExtent` là cạnh lớn nhất của bounds gộp mọi Renderer, đo lúc Enter. Tỷ lệ đích = tỷ lệ thế giới gốc × fit.
5. Điểm đích (thế giới) = `cam.ViewportToWorldPoint(viewportPoint, viewDistance)` trừ đi độ lệch tâm bounds, để **tâm bounds** nằm đúng điểm đích.
6. `model.SetParent(null, true)`. Đặt Phase = Entering, `player.Frozen = true`, `targeting.enabled = false`, `inspectLight.enabled = true`.
7. Trả về `true`.

**Update** (dùng `Time.unscaledDeltaTime`):
- **Entering:** p chạy từ 0 đến 1 trong `moveSeconds`; dùng `e = Ease(p)`. Vị trí lerp, xoay slerp từ tư thế bắt đầu, tỷ lệ lerp. `inspectVolume.weight = e`, cường độ đèn = `lightIntensity · e`. Khi p = 1 thì chuyển sang Viewing.
- **Viewing:**
  - Khi giữ Attack: xoay model quanh trục `cam.up` theo `-look.x` và quanh trục `cam.right` theo `look.y`, nhân `rotateDegreesPerPixel`. Cập nhật mốc thời gian lần kéo cuối.
  - Khi không kéo quá `idleSpinDelay`: xoay quanh `Vector3.up` với `idleSpinDegreesPerSecond`.
  - Cuộn chuột: `zoom = InspectMath.Zoom(zoom, Mouse.current.scroll.y.ReadValue() / 120f)`. Tỷ lệ = tỷ lệ đích × zoom.
  - Vị trí = điểm đích + `cam.up · Bob(thời gian xem, …)`. Tâm bounds được giữ đúng điểm đích khi xoay; tính lại độ lệch tâm từ bounds hiện tại.
- **Leaving:** p chạy từ 0 đến 1. Lerp và slerp từ tư thế hiện tại về tư thế **thế giới** gốc (tính từ parent gốc). Weight volume và đèn giảm về 0. Khi p = 1 thì gọi `Restore()`.

**Nhận phím** (trong `Update` của inspector, chỉ khi Phase là Entering hoặc Viewing): Esc → `Exit()`. Phím E do `NarrativeDirector` xử lý (Bước 6), không nhận ở đây, để không bắt trùng.

**Exit():** chỉ có tác dụng khi Phase là Entering hoặc Viewing. Ghi nhận tư thế hiện tại làm điểm bắt đầu, đặt Phase = Leaving. `moveSeconds = 0` thì `Restore()` ngay.

**Restore()** (dùng chung cho kết thúc Leaving, OnDisable, OnDestroy):
- Nếu model còn tồn tại: `SetParent(parent gốc, false)`, rồi đặt lại đúng `localPosition`, `localRotation` và `localScale` gốc. Bật lại các collider đã tắt (bỏ qua collider đã bị hủy).
- `player.Frozen = false`, `targeting.enabled = true`, `inspectLight.enabled = false`, `inspectVolume.weight = 0`, Phase = Idle, `zoom = 1`.
- Kiểm tra null cho từng tham chiếu, vì lúc thoát game thứ tự hủy không cố định.

**Model hoặc anchor bị hủy khi đang xem:** đầu mỗi `Update`, nếu model là null thì gọi `Restore()`. Không có exception.

**OnDisable:** nếu đang xem thì `Restore()` ngay, không tween. Đây là bảo vệ dữ liệu: hiện vật không bao giờ bị kẹt lệch khỏi bệ.

**Test — `Tests/PlayMode/ExhibitInspectorTests.cs`** (kế thừa `InputTestFixture`). Dùng scene tạm: camera tại gốc nhìn +Z, hai anchor có model là cube con cỡ 0,3 m và 3,4 m, một Volume với profile rỗng, một Light, một `FirstPersonController` và một `ExhibitTargeting` đã nối.

| Test | Kiểm tra |
|---|---|
| `Enter_AfterMoveSeconds_ModelAtViewPoint` | Sau 0,5 s: tâm bounds cách điểm đích dưới 0,01 m; cạnh lớn nhất ≈ 0,45 ± 0,01; Phase = Viewing |
| `Enter_SetsSideEffects` | Collider của model tắt; đèn bật; weight = 1; `Frozen = true`; `targeting.enabled = false` |
| `Enter_Midway_IsBetweenHomeAndTarget` | Ở 0,2 s, khoảng cách tới điểm gốc và tới điểm đích đều lớn hơn 0,05 m |
| `Exit_RestoresExactLocalPose` | Sau Enter, xoay, zoom, Exit và 0,5 s: `localPosition`, `localRotation`, `localScale` và parent bằng đúng giá trị gốc (sai số 1e-4) |
| `Exit_RestoresSideEffects` | Collider bật lại; đèn tắt; weight = 0; `Frozen = false`; targeting bật |
| `ExitDuringEnter_ReturnsHome` | Exit ở 0,2 s → sau 0,5 s về đúng tư thế gốc |
| `EnterWhileActiveOrLeaving_IsIgnored` | Gọi Enter lần hai trả về `false`; Anchor không đổi |
| `Drag_WithAttack_Rotates` | Giữ chuột trái và di chuyển chuột → rotation đổi |
| `Look_WithoutAttack_DoesNotRotateModel` | Chỉ di chuyển chuột → không xoay (sau khi đặt `idleSpinDegreesPerSecond = 0`) |
| `Scroll_ZoomsWithinLimits` | Cuộn lên 20 nấc → tỷ lệ = tỷ lệ đích × 1,6 |
| `Bob_MovesVertically` | Với spin = 0, y thay đổi trong ±0,012 m qua 1,5 s; biên độ 0 thì y đứng yên |
| `IdleSpin_StopsWhileDragging` | Khi đang kéo, không có thêm góc tự xoay |
| `DisableWhileActive_RestoresImmediately` | Tắt inspector khi đang xem → model về đúng chỗ trong cùng frame |
| `ModelDestroyedWhileActive_NoErrors` | Hủy model → không có lỗi trong log; Phase = Idle; `Frozen = false` |
| `AnchorWithoutModel_EnterReturnsFalse` | Anchor không có con → `false`, mọi trạng thái giữ nguyên |
| `LargeModel_FitsViewSize` | Model 3,4 m → cạnh lớn nhất ≈ 0,45 |
| `Escape_Exits` | Nhấn Esc khi đang xem → Phase = Leaving, rồi Idle |
| `MissingWiring_LogsErrorAndDisables` | `LogAssert.Expect` thông báo lỗi; `enabled = false` |
| `ZeroMoveSeconds_IsInstant` | `moveSeconds = 0` → Viewing ngay sau một frame; Exit → về chỗ ngay |

### Bước 6 — `Assets/Museum/Scripts/NarrativeDirector.cs` (sửa)

- Thêm hai trường tùy chọn: `[SerializeField] ExhibitTargeting targeting; [SerializeField] ExhibitInspector inspector;`
- Khi nhấn Interact (`interact.WasPressedThisFrame()`), xử lý theo thứ tự:
  1. Nếu `inspector != null && inspector.IsActive`: gọi `inspector.Exit()`. Bảng trở về trạng thái của logic: `ActiveIndex == -1` thì `Hide()`, ngược lại `Show(info, logic.Expanded)`.
  2. Nếu `inspector != null && targeting != null && targeting.Current != null` và `inspector.Enter(targeting.Current)` trả về `true`: gọi `panel.Show(targeting.Current.Info, true, hint chế độ Xem)`.
  3. Còn lại: bật/tắt mở rộng như hiện nay.
- Khi `inspector.IsActive`, bỏ qua lệnh đổi bảng do người chơi rời hoặc vào vùng neo. Người chơi đứng yên nên trường hợp này hiếm, nhưng phải chặn để bảng không đổi giữa lúc xem.
- Inspector thoát bằng Esc mà không qua director, nên director cần nhận ra khi `IsActive` chuyển từ true sang false và đưa bảng về trạng thái của logic như mục 1.

**Test — thêm vào `Tests/PlayMode/NarrativeDirectorInputTests.cs`:**
- `PressE_WhileAiming_EntersInspectWithExpandedStory`: bảng hiện đúng hiện vật đang nhắm, ở dạng mở rộng, dòng gợi ý là chuỗi chế độ Xem.
- `PressE_WhileInspecting_ExitsAndRestoresPanel`.
- `Escape_WhileInspecting_RestoresPanel`.
- `PressE_AimingAtOtherExhibitInsideOverlap_InspectsAimedOne` (D4).
- `PressE_NotAiming_TogglesAsBefore`: hồi quy.
- `NoInspectorAssigned_BehavesAsBefore`: hồi quy. Test `Director_PressE_TogglesStory` hiện có vẫn phải đạt mà không sửa.

### Bước 7 — `Assets/Museum/Settings/Museum_InspectProfile.asset` (mới)

Tạo VolumeProfile có:
- `DepthOfField`: mode Gaussian, `gaussianStart = 1.2`, `gaussianEnd = 4`, `gaussianMaxRadius = 1.5`, `highQualitySampling = true`.
- `Vignette`: `intensity = 0.45`, `smoothness = 0.4`.

Không thêm `ColorAdjustments` (D3). Tạo bằng code trong snippet Bước 8, hoặc bằng `manage_asset`.

### Bước 8 — Nối scene: `Docs/scene_wiring/fx_e_inspect.cs.txt` (mới)

Viết theo mẫu `fx_a`…`fx_d`: chạy lại an toàn được, tự kiểm tra, in `RESULT_OK` hoặc `RESULT_FAIL`, lưu scene. Trước khi chạy, chép `Museum.unity` sang `C:\code\mon_backup_scene\Museum.unity.bak2` và kiểm bằng `cmp`.

Snippet làm các việc sau:
1. Tạo hoặc cập nhật `Main Camera/InspectLight`: Point, Realtime, `shadows = None`, `range = 2`, màu 3800 K, `localPosition = (0.35, 0.3, 0.3)`, `enabled = false`.
2. Tạo profile ở Bước 7 nếu chưa có.
3. Tạo hoặc cập nhật GameObject gốc `InspectVolume`: `Volume`, `isGlobal = true`, `priority = 10`, `weight = 0`, `sharedProfile` là profile ở Bước 7.
4. Thêm `ExhibitInspector` vào `Player` nếu chưa có. Nối `cam`, `player`, `targeting` (trên `Main Camera`), `inspectLight`, `inspectVolume`, `actions` (`Assets/InputSystem_Actions.inputactions`).
5. Trên `NarrativeDirector`: gán `targeting` và `inspector`.
6. Kết quả đạt: `RESULT_OK inspector=1 light=InspectLight volume=InspectVolume prio=10 dof=True director=wired dirty=False`.

**Test — thêm vào `Tests/EditMode/MuseumSceneWiringTests.cs`:**
- `Inspector_IsWired`: có đúng 1 `ExhibitInspector`, và 6 tham chiếu khác null.
- `Director_HasTargetingAndInspector`.
- `InspectVolume_HasDepthOfField_AndStartsAtZeroWeight`.
- `InspectLight_IsRealtimeShadowlessAndOff`.
- `EveryAnchor_HasModelChildWithRenderer`.

## 2. Phần còn lại

**Chạy kiểm tra sau Bước 8:**
- EditMode toàn bộ: dự kiến **40 + 8 + 2 + 5 = 55** test (8 `InspectMathTests`, 2 test bảng thông tin, 5 test scene).
- PlayMode toàn bộ: dự kiến **23 + 3 + 1 + 19 + 6 = 52** test (3 controller, 1 targeting, 19 inspector, 6 director).
- `dotnet build Museum.Runtime.csproj` và `read_console` (error, warning).
- Mỗi lần chạy, báo mã job và số liệu lấy từ `get_test_job`.

**Rà diff:** chỉ các file sau được thay đổi: 6 script (2 mới, 4 sửa), 1 profile, scene, các file test và 1 snippet. Không có file `.csproj`, `Library/` hay `ProjectSettings/` nào thay đổi do tay người.

**Tiêu chí nghiệm thu:**

| Tiêu chí | Test chứng minh |
|---|---|
| Nhắm vào hiện vật và nhấn E: hiện vật bay ra trước mặt, lệch phải, nền mờ | `Enter_*`, `PressE_WhileAiming_*`, `InspectVolume_*` |
| Hiện vật lơ lửng: nhấp nhô và tự xoay chậm | `Bob_MovesVertically`, `IdleSpin_StopsWhileDragging` |
| Kéo chuột trái để xoay, cuộn để phóng to trong khoảng 0,6–1,6 | `Drag_*`, `Look_WithoutAttack_*`, `Scroll_*` |
| E hoặc Esc: hiện vật về đúng chỗ cũ | `Exit_RestoresExactLocalPose`, `ExitDuringEnter_*`, `Escape_Exits` |
| Hiện vật không bao giờ bị kẹt lệch khỏi bệ | `DisableWhileActive_*`, `ModelDestroyedWhileActive_*` |
| Bia Tự Đức 3,4 m vẫn vừa khung | `LargeModel_FitsViewSize` |
| Người chơi đứng yên khi xem | `Frozen_*`, `Enter_SetsSideEffects` |
| Hành vi cũ không đổi | `PressE_NotAiming_*`, `NoInspectorAssigned_*`, toàn bộ test hiện có |

**Kiểm tra bằng tay** (`NOT RUN` cho đến khi có người nhìn): bấm Play và xem lần lượt cả 6 hiện vật.
- Bảng thông tin mở rộng không che hiện vật ở các màn 16:9 và 2200×942.
- Ấm men ngọc và bình hoa lam không bị chói ở `lightIntensity = 4`. Nếu chói, giảm giá trị này.
- Nền mờ vừa phải; tâm ngắm ẩn khi xem và hiện lại khi thoát.

**Build và chạy thử:** `manage_build` bản release. Kiểm tra DLL có giờ ghi sau lúc bắt đầu build. Chạy player 40 s, đọc `-logFile` và chụp ảnh.

**Rủi ro:**
- Hiện vật cách camera 0,85 m có thể cắt vào tường nếu người chơi đứng sát tường. Bệ và tầm nhắm 4 m làm rủi ro thấp; kiểm tra bằng tay.
- DoF trên bộ cấu hình Mobile tốn hiệu năng. Đo FPS hoặc ghi `NOT RUN`.
- Cuộn chuột lấy thẳng từ `Mouse.current`, nên tay cầm chơi game không zoom được.
- Esc vừa thoát chế độ Xem vừa mở khóa chuột (D1). Người chơi phải click để khóa lại.

**Quyền:** chạy snippet sửa scene cần người dùng duyệt `execute_code`. Không có việc nào gửi dữ liệu ra ngoài máy.

**Bằng chứng cần báo khi xong:** dòng `RESULT_OK` của snippet; mã job và số liệu EditMode, PlayMode; trạng thái build và giờ ghi DLL; log và ảnh chụp khi chạy thử; danh sách mục `NOT RUN` kèm lý do.
