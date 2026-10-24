# Hỏi đáp về code

**Người đọc:** cả ba thành viên nhóm, nhất là người nói slide 7–11.
**Mục đích:** khi giảng viên hỏi về code, mở đúng file, chỉ đúng dòng và giải thích ngắn.
**Nguồn:** `Assets/Museum/Scripts` (10 file, 751 dòng) và `Assets/Museum/Tests`, đọc ngày 17/10/2026. Số dòng trong file này khớp với code tại thời điểm đó.

Câu hỏi không về code nằm ở [03_hoi_dap.md](03_hoi_dap.md).

---

## 1. Cách trả lời câu hỏi về code

1. **Mở sẵn code trước buổi.** Mở Visual Studio hoặc VS Code với thư mục `Assets/Museum/Scripts`. Mở sẵn 3 file hay bị hỏi: `NarrativeDirector.cs`, `NarrativeAnchorLogic.cs`, `ExhibitInspector.cs`.
2. **Trả lời theo 3 bước:** nói hàm đó làm gì, chỉ dòng code, rồi nêu test kiểm tra nó.
3. **Không biết thì nói thật.** Ví dụ: "Phần này em chưa nắm chi tiết, em xin xem lại code và trả lời sau."

---

## 2. Bản đồ code (thuộc lòng bảng này)

| File | Dòng | Vai trò | Test kiểm tra |
|---|---|---|---|
| [`NarrativeDirector.cs`](../../Assets/Museum/Scripts/NarrativeDirector.cs) | 93 | Bộ điều phối: tìm vùng gần nhất mỗi khung hình, xử lý phím E | [`NarrativeDirectorInputTests`](../../Assets/Museum/Tests/PlayMode/NarrativeDirectorInputTests.cs), [`NarrativeDirectorPlayModeTests`](../../Assets/Museum/Tests/PlayMode/NarrativeDirectorPlayModeTests.cs) |
| [`NarrativeAnchorLogic.cs`](../../Assets/Museum/Scripts/NarrativeAnchorLogic.cs) | 57 | Logic thuần: chọn hiện vật gần nhất, mở/thu bảng | [`NarrativeAnchorLogicTests`](../../Assets/Museum/Tests/EditMode/NarrativeAnchorLogicTests.cs) |
| [`NarrativeAnchor.cs`](../../Assets/Museum/Scripts/NarrativeAnchor.cs) | 22 | Gắn trên mỗi hiện vật: nội dung, bán kính, đèn | Gián tiếp qua các test trên |
| [`ExhibitInfo.cs`](../../Assets/Museum/Scripts/ExhibitInfo.cs) | 16 | "Phiếu thông tin" 7 trường (ScriptableObject) | Gián tiếp |
| [`ExhibitInfoPanel.cs`](../../Assets/Museum/Scripts/ExhibitInfoPanel.cs) | 85 | Hiện bảng thuyết minh, tự giảm cỡ chữ cho vừa | [`ExhibitInfoPanelTests`](../../Assets/Museum/Tests/EditMode/ExhibitInfoPanelTests.cs), [`ExhibitInfoPanelFitTests`](../../Assets/Museum/Tests/EditMode/ExhibitInfoPanelFitTests.cs) |
| [`ExhibitTargeting.cs`](../../Assets/Museum/Scripts/ExhibitTargeting.cs) | 118 | Bắn tia nhắm, làm sáng đèn, tâm ngắm, bút laze | [`ExhibitTargetingTests`](../../Assets/Museum/Tests/PlayMode/ExhibitTargetingTests.cs) |
| [`ExhibitInspector.cs`](../../Assets/Museum/Scripts/ExhibitInspector.cs) | 228 | Xem gần: máy trạng thái 4 pha, xoay, zoom, trả về | [`ExhibitInspectorTests`](../../Assets/Museum/Tests/PlayMode/ExhibitInspectorTests.cs) |
| [`InspectMath.cs`](../../Assets/Museum/Scripts/InspectMath.cs) | 18 | Phép tính thuần cho xem gần | [`InspectMathTests`](../../Assets/Museum/Tests/EditMode/InspectMathTests.cs) |
| [`FirstPersonController.cs`](../../Assets/Museum/Scripts/FirstPersonController.cs) | 78 | Đi, nhìn, khóa chuột, trạng thái `Frozen` | [`FirstPersonControllerTests`](../../Assets/Museum/Tests/PlayMode/FirstPersonControllerTests.cs) |
| [`ScreenFader.cs`](../../Assets/Museum/Scripts/ScreenFader.cs) | 36 | Màn hình sáng dần khi vào cảnh | [`FadeTests`](../../Assets/Museum/Tests/PlayMode/FadeTests.cs) |

Ngoài ra, [`MuseumLayoutTests`](../../Assets/Museum/Tests/EditMode/MuseumLayoutTests.cs) kiểm tra bố cục phòng, và [`MuseumSceneWiringTests`](../../Assets/Museum/Tests/EditMode/MuseumSceneWiringTests.cs) kiểm tra các thành phần trong cảnh đã được nối với nhau.

**Sơ đồ nói miệng:** người chơi đi bằng `FirstPersonController`. Mỗi khung hình, `NarrativeDirector` hỏi `NarrativeAnchorLogic` "người chơi đang ở vùng nào", và hỏi `ExhibitTargeting` "người chơi đang nhắm vào gì". Khi nhấn E, Director quyết định gọi `ExhibitInfoPanel` (đọc) hay `ExhibitInspector` (xem gần).

---

## 3. Câu hỏi về code và câu trả lời

### C1. "Em mở đoạn code chọn hiện vật gần nhất cho thầy xem."

**Đường dẫn:** [`Assets/Museum/Scripts/NarrativeAnchorLogic.cs:12-34`](../../Assets/Museum/Scripts/NarrativeAnchorLogic.cs#L12-L34)

Mở `NarrativeAnchorLogic.cs`, hàm `FindNearest` (dòng 12–34).

```csharp
float dx = positions[i].x - player.x;
float dz = positions[i].z - player.z;
float sqr = dx * dx + dz * dz;
if (sqr <= r * r && sqr < bestSqr)
```

Giải thích:

- Chỉ lấy hiệu theo trục x và z, bỏ trục y (chiều cao). Vì vậy khoảng cách được đo trên mặt sàn.
- Hàm so sánh **bình phương** khoảng cách với bình phương bán kính, nên không cần tính căn bậc hai. Kết quả so sánh vẫn đúng mà nhanh hơn.
- Hàm giữ lại hiện vật có khoảng cách nhỏ nhất. Không có hiện vật nào trong vùng thì trả về `-1`.
- Đầu hàm kiểm tra đầu vào: danh sách là `null` hoặc hai danh sách khác độ dài thì báo lỗi ngay (dòng 14–16).

### C2. "Phím E xử lý ở đâu?"

**Đường dẫn:** [`Assets/Museum/Scripts/NarrativeDirector.cs:70-78`](../../Assets/Museum/Scripts/NarrativeDirector.cs#L70-L78)

Mở `NarrativeDirector.cs`, hàm `Update` (dòng 70–78).

```csharp
if (inspector != null && inspector.IsActive) inspector.Exit();
else if (inspector != null && aimed != null && aimed.Info != null && inspector.Enter(aimed))
    panel.Show(aimed.Info, true, InspectHint);
else if (logic.ActiveIndex != -1)
{
    logic.ToggleExpanded();
    panel.Show(anchors[logic.ActiveIndex].Info, logic.Expanded);
}
```

Ba nhánh `if / else if` chính là thứ tự ưu tiên ở slide 9: đang xem gần thì thoát, đang nhắm thì xem gần, còn lại thì mở/thu bảng. Biến `aimed` được đọc **trước** khi gọi `Enter`, vì `Enter` sẽ tắt bộ nhắm (comment ở dòng 70).

### C3. "Làm sao hiện vật về đúng chỗ cũ?"

**Đường dẫn:** [`Assets/Museum/Scripts/ExhibitInspector.cs:68-101`](../../Assets/Museum/Scripts/ExhibitInspector.cs#L68-L101) · [`Assets/Museum/Scripts/ExhibitInspector.cs:189-211`](../../Assets/Museum/Scripts/ExhibitInspector.cs#L189-L211)

Mở `ExhibitInspector.cs`.

- Hàm `Enter` (dòng 68–101) lưu vật chứa và vị trí, góc xoay, tỷ lệ **cục bộ** của mô hình (dòng 76–79), rồi tắt các collider đang bật và ghi lại chúng (dòng 81–83).
- Hàm `Restore` (dòng 189–211) gắn mô hình lại vào vật chứa và gán lại đúng các giá trị đã lưu. Sau đó nó bật lại collider, mở khóa người chơi, bật lại bộ nhắm, tắt đèn và hiệu ứng.

Điểm chính: `Restore` **gán lại giá trị gốc**, không cộng trừ dần. Vì vậy dù xem gần bao nhiêu lần, mô hình cũng không bị lệch dần.

### C4. "Nếu có lỗi giữa chừng thì hiện vật có bị kẹt không?"

**Đường dẫn:** [`Assets/Museum/Scripts/ExhibitInspector.cs:63-66`](../../Assets/Museum/Scripts/ExhibitInspector.cs#L63-L66) · [`Assets/Museum/Scripts/ExhibitInspector.cs:115-126`](../../Assets/Museum/Scripts/ExhibitInspector.cs#L115-L126) · [`Assets/Museum/Scripts/ExhibitInspector.cs:189-211`](../../Assets/Museum/Scripts/ExhibitInspector.cs#L189-L211)

Không. Có ba đường đều dẫn về `Restore`:

- `OnDisable` (dòng 63–66): thành phần bị tắt khi đang xem gần thì trả hiện vật về.
- Dòng 118: mô hình bị xoá thì dọn trạng thái.
- Dòng 119–126: bệ (anchor) bị xoá thì xoá luôn mô hình mồ côi, không để nó lơ lửng.

`Restore` kiểm tra `null` trước mỗi tham chiếu (dòng 191–205), vì lúc đó một số đối tượng có thể đã bị xoá.

### C5. "Giải thích máy trạng thái."

**Đường dẫn:** [`Assets/Museum/Scripts/ExhibitInspector.cs:8`](../../Assets/Museum/Scripts/ExhibitInspector.cs#L8) · [`Assets/Museum/Scripts/ExhibitInspector.cs:103-113`](../../Assets/Museum/Scripts/ExhibitInspector.cs#L103-L113) · [`Assets/Museum/Scripts/ExhibitInspector.cs:134-179`](../../Assets/Museum/Scripts/ExhibitInspector.cs#L134-L179)

`ExhibitInspector.cs`, dòng 8: `enum InspectPhase { Idle, Entering, Viewing, Leaving }`. Hàm `Update` dùng `switch (Phase)` (dòng 134–179):

- **Entering:** tăng `progress` từ 0 đến 1, nội suy vị trí và tỷ lệ từ bệ ra trước camera. Đủ 1 thì chuyển sang Viewing.
- **Viewing:** giữ chuột trái để xoay, cuộn để zoom. Để yên 2 giây thì mô hình tự quay chậm, và mô hình nhấp nhô nhẹ.
- **Leaving:** nội suy từ vị trí hiện tại về bệ. Đủ 1 thì gọi `Restore`, về Idle.

Hàm `Exit` (dòng 103–113) lấy **vị trí hiện tại** làm điểm xuất phát (dòng 107). Vì vậy, nhấn thoát khi mô hình còn đang bay ra thì mô hình quay về ngay từ chỗ đó, không bị giật.

### C6. "Mô hình to nhỏ khác nhau, sao vừa khung hình?"

**Đường dẫn:** [`Assets/Museum/Scripts/InspectMath.cs:7-8`](../../Assets/Museum/Scripts/InspectMath.cs#L7-L8) · [`Assets/Museum/Scripts/ExhibitInspector.cs:220-226`](../../Assets/Museum/Scripts/ExhibitInspector.cs#L220-L226)

`InspectMath.FitScale` (dòng 7–8 trong `InspectMath.cs`) lấy kích thước lớn nhất của mô hình rồi tính hệ số để cạnh dài nhất bằng `viewSize` (0,45 m). Hàm cũng chặn trường hợp kích thước bằng 0, NaN hoặc vô cực, khi đó trả về 1.

Kích thước mô hình được tính bằng hàm `Bounds` (`ExhibitInspector.cs`, dòng 220–226): gộp hộp bao của mọi Renderer con.

### C7. "Zoom giới hạn thế nào?"

**Đường dẫn:** [`Assets/Museum/Scripts/InspectMath.cs:12-13`](../../Assets/Museum/Scripts/InspectMath.cs#L12-L13)

`InspectMath.Zoom` (dòng 12–13): mỗi nấc cuộn nhân tỷ lệ với 1,1, rồi kẹp trong khoảng 0,6–1,6 bằng `Mathf.Clamp`.

### C8. "Raycast viết thế nào?"

**Đường dẫn:** [`Assets/Museum/Scripts/ExhibitTargeting.cs:112-116`](../../Assets/Museum/Scripts/ExhibitTargeting.cs#L112-L116)

`ExhibitTargeting.cs`, hàm `Cast` (dòng 112–114). Tia đi từ vị trí camera theo hướng `forward`. Tầm là `laserDistance` (20 m) khi bật laze, còn lại là `maxDistance` (4 m). Hàm `FindTarget` (dòng 116) lấy collider bị trúng rồi tìm `NarrativeAnchor` ở vật cha bằng `GetComponentInParent`. Nhờ vậy, tia trúng bất kỳ phần nào của mô hình cũng nhận ra đúng hiện vật.

### C9. "Đèn sáng lên rồi tắt có bị sai dần độ sáng không?"

**Đường dẫn:** [`Assets/Museum/Scripts/ExhibitTargeting.cs:63-82`](../../Assets/Museum/Scripts/ExhibitTargeting.cs#L63-L82) · [`Assets/Museum/Scripts/ExhibitTargeting.cs:47-56`](../../Assets/Museum/Scripts/ExhibitTargeting.cs#L47-L56)

Không. `ExhibitTargeting.cs` lưu độ sáng gốc của mỗi đèn **một lần** vào `baseIntensity` (dòng 30, dòng 68). Độ sáng đích luôn tính từ giá trị gốc này: gốc × 1,5 khi nhắm, bằng gốc khi thôi nhắm (dòng 78). Khi thành phần bị tắt, `OnDisable` trả mọi đèn về độ sáng gốc (dòng 49–50).

### C10. "Giới hạn góc nhìn ở đâu?"

**Đường dẫn:** [`Assets/Museum/Scripts/FirstPersonController.cs:59-67`](../../Assets/Museum/Scripts/FirstPersonController.cs#L59-L67)

`FirstPersonController.cs`, dòng 63: `pitch = Mathf.Clamp(pitch - d.y, -85f, 85f);`. Xoay trái phải thì xoay cả thân người chơi (dòng 62). Xoay lên xuống chỉ xoay `cameraPivot`. Khi `Frozen` là `true`, cả di chuyển lẫn xoay bị bỏ qua (dòng 59 và 67).

### C11. "Trọng lực xử lý sao?"

**Đường dẫn:** [`Assets/Museum/Scripts/FirstPersonController.cs:71-75`](../../Assets/Museum/Scripts/FirstPersonController.cs#L71-L75)

`FirstPersonController.cs`, dòng 71–75. Mỗi khung hình, vận tốc rơi cộng thêm `gravity × deltaTime`. Khi đang đứng trên sàn, vận tốc rơi được đặt về −2 để nhân vật luôn bám sàn. `CharacterController.Move` lo việc va chạm với tường và sàn.

### C12. "Vì sao dùng `unscaledDeltaTime`?"

**Đường dẫn:** [`Assets/Museum/Scripts/ExhibitInspector.cs:131`](../../Assets/Museum/Scripts/ExhibitInspector.cs#L131) · [`Assets/Museum/Scripts/FirstPersonController.cs:190`](../../Assets/Museum/Scripts/FirstPersonController.cs#L190)

Để hiệu ứng (đèn, bay ra bay vào, fade) chạy đúng tốc độ ngay cả khi game bị chỉnh `Time.timeScale`, ví dụ khi tạm dừng. Riêng phần di chuyển dùng `Time.deltaTime` thường, vì di chuyển nên dừng khi game dừng.

### C13. "Các thành phần biết nhau bằng cách nào?"

**Đường dẫn:** [`Assets/Museum/Scripts/NarrativeDirector.cs:9-13`](../../Assets/Museum/Scripts/NarrativeDirector.cs#L9-L13) · [`Assets/Museum/Scripts/NarrativeDirector.cs:24-31`](../../Assets/Museum/Scripts/NarrativeDirector.cs#L24-L31) · [`Assets/Museum/Tests/EditMode/MuseumSceneWiringTests.cs`](../../Assets/Museum/Tests/EditMode/MuseumSceneWiringTests.cs)

Bằng trường `[SerializeField]` được gán sẵn trong cảnh (ví dụ `NarrativeDirector.cs`, dòng 9–13). Nhóm không dùng `GameObject.Find` theo tên, vì đổi tên đối tượng là hỏng. Nếu thiếu tham chiếu, `OnEnable` ghi lỗi rõ ràng và tắt thành phần (dòng 26–30). Test `MuseumSceneWiringTests` kiểm tra mọi tham chiếu trong cảnh đã được gán.

### C14. "Vì sao tách `NarrativeAnchorLogic` và `InspectMath` ra riêng?"

**Đường dẫn:** [`Assets/Museum/Scripts/NarrativeAnchorLogic.cs`](../../Assets/Museum/Scripts/NarrativeAnchorLogic.cs) · [`Assets/Museum/Scripts/InspectMath.cs`](../../Assets/Museum/Scripts/InspectMath.cs) · [`Assets/Museum/Tests/EditMode/NarrativeAnchorLogicTests.cs`](../../Assets/Museum/Tests/EditMode/NarrativeAnchorLogicTests.cs) · [`Assets/Museum/Tests/EditMode/InspectMathTests.cs`](../../Assets/Museum/Tests/EditMode/InspectMathTests.cs)

Hai lớp này không phụ thuộc vào cảnh Unity. Vì vậy có thể kiểm thử bằng EditMode: chạy nhanh, không cần mở cảnh. Phần còn lại phụ thuộc vào cảnh thì kiểm thử bằng PlayMode.

### C15. "Cho thầy xem một test."

**Đường dẫn:** [`Assets/Museum/Tests/EditMode/NarrativeAnchorLogicTests.cs`](../../Assets/Museum/Tests/EditMode/NarrativeAnchorLogicTests.cs)

Mở `Tests/EditMode/NarrativeAnchorLogicTests.cs`. File này có khoảng 20 test cho logic chọn vùng: đứng ngoài mọi vùng, đứng đúng mép vùng, hai vùng chồng nhau, bán kính bằng 0, đầu vào sai. Nói: "Mỗi test đặt người chơi ở một vị trí, rồi kiểm tra hàm trả đúng hiện vật."

### C16. "Code có chỗ nào còn hạn chế không?"

**Đường dẫn:** [`Assets/Museum/Scripts/NarrativeDirector.cs:33`](../../Assets/Museum/Scripts/NarrativeDirector.cs#L33) · [`Assets/Museum/Scripts/ExhibitInfoPanel.cs:58`](../../Assets/Museum/Scripts/ExhibitInfoPanel.cs#L58) · [`Assets/Museum/Scripts/ExhibitInspector.cs:158`](../../Assets/Museum/Scripts/ExhibitInspector.cs#L158) · [`Assets/Museum/Scripts/ExhibitTargeting.cs:112-114`](../../Assets/Museum/Scripts/ExhibitTargeting.cs#L112-L114)

Có, và nhóm đánh dấu bằng comment `ponytail:` trong code:

- `NarrativeDirector.cs`, dòng 33: danh sách hiện vật chỉ lấy một lần khi bắt đầu. Hiện vật thêm vào lúc chạy sẽ không được nhận. Bảo tàng hiện có hiện vật cố định nên chưa cần sửa.
- `ExhibitInfoPanel.cs`, dòng 58: cỡ chữ chỉ được chỉnh khi bảng hiện ra. Nếu đổi kích thước cửa sổ khi đang đọc, chữ chưa tự chỉnh lại.
- `ExhibitInspector.cs`, dòng 158: mỗi khung hình chỉ nhận một nấc cuộn chuột.

Ngoài ra: khi bật bút laze, tầm nhắm 20 m cũng áp dụng cho việc xem gần, nên có thể mở xem gần một hiện vật ở xa.

### C17. "Nhóm có dùng AI để viết code không?"

*Nhóm tự quyết định câu trả lời và trả lời trung thực. Nếu có dùng, nói rõ AI hỗ trợ phần nào, và nhóm đã kiểm tra lại bằng cách nào (đọc code, chạy 128 test, chạy bản build).*

---

## 4. Chuẩn bị trước buổi

1. Mở sẵn 3 file ở mục 1 trong trình soạn code.
2. Mỗi người đọc lại ít nhất các câu C1, C2, C3 và C5. Đây là các câu dễ bị hỏi nhất.
3. Nếu có thời gian, mở Unity và chạy thử một test EditMode (`NarrativeAnchorLogicTests`) để có thể trình diễn khi được hỏi. Phải đóng bản build trước khi mở Unity.
