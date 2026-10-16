# Rebuild: sảnh tròn + 3 phòng chủ đề

Người đọc: các subagent làm việc trong Unity và model chính kiểm tra. Mục đích: dựng lại `Assets/Museum/Scenes/Museum.unity` theo ảnh trong `Docs/tham_khao`. Mọi tọa độ dưới đây là cố định. Không tự đổi số. Nếu một số không dùng được, dừng và báo lại.

## Quy ước

- Đơn vị mét. Trục +z là hướng bắc. Sàn ở y = 0.
- "yaw" là góc quay quanh trục y. Với yaw 90 mặt hiện vật quay về +x, yaw 180 quay về −z, yaw 270 quay về −x.
- Mọi bước sửa scene đều lưu scene, rồi in `RESULT_OK` hoặc `RESULT_FAIL`.
- Không thêm `NarrativeAnchor` cho đồ trang trí. Scene phải có đúng 6 anchor.
- Không dùng các model Khmer (BayonRelief, Devata, BuddhaHead, GuardianLion).
- Đồ trang trí không có bảng tên hiện vật.
- Mesh tự tạo bằng script không có UV2. Đặt đồ trang trí **không static**, để light probe chiếu sáng. Chỉ phần kiến trúc dạng khối hộp được đặt lightmap-static.
- Kính và đồ trang trí đứng gần hiện vật: không có collider, hoặc đặt ở layer `Ignore Raycast`. Lý do: tia ngắm `ExhibitTargeting` (4 m) dùng `DefaultRaycastLayers` và trúng cả trigger.
- Chữ 3D: dùng `TextMeshPro` (không phải UGUI) với font `Assets/Museum/Fonts/BeVietnamPro-SemiBold SDF.asset` hoặc bản Regular.

## Bảng bố cục

### Không gian

| Khối | Giới hạn x | Giới hạn z | Cao trần | Màu tường |
|---|---|---|---|---|
| Sảnh tròn (Rotunda) | tâm (0,0), bán kính trong 8, tường dày 0,4 | — | tường 8, giếng trời bán kính 5 ở y = 8 | đá ngà (texture `Wall_Ivory`) |
| Tiền sảnh (Vestibule) | −3 … 3 | −16 … −8 | 4 | đá ngà |
| Phòng Tây: Điêu khắc Chăm | −20,2 … −8 | −6 … 6 | 5,5 | đất nung #9A4B32, chân tường gạch đỏ cao 1,2 m |
| Phòng Bắc: Gốm sứ Việt | −6 … 6 | 8 … 20,2 | 5,5 | xanh navy #1F2A44 |
| Phòng Đông: Lăng tẩm nhà Nguyễn | 8 … 20,2 | −6 … 6 | 5,5 | đỏ sơn son #6E1E1A, viền vàng #B8913A |

Sàn mọi khối: đá marble sáng, bóng (smoothness khoảng 0,8).

### Cửa vòm trên tường sảnh tròn

Có 4 cửa ở hướng N, S, E, W. Mỗi cửa rộng 3,0 m. Phần thẳng cao 3,0 m, trên có vòm bán nguyệt bán kính 1,5 m, nên đỉnh vòm cao 4,5 m. Hành lang ngắn nối từ tường sảnh (bán kính 8–8,4) tới tường phòng có cùng mặt cắt đó. Lối đi phải để trống đủ 3,0 × 3,0 m.

### Hiện vật

Gốc `Exhibit_XX` đặt ở y = 1,0 (đỉnh bệ). Bệ cao 1,0 m, mặt bệ 0,8 × 0,8 m, tên `Plinth_XX`, nằm dưới root `Plinths`.

| Anchor | Vị trí (x, z) | yaw | Chỗ đứng xem (x, z) | Phòng |
|---|---|---|---|---|
| Exhibit_01 (Shiva) | (−12, −2,5) | 90 | (−10, −2,5) | Tây |
| Exhibit_02 (Vua Chăm) | (−17,5, 2,5) | 90 | (−15,5, 2,5) | Tây |
| Exhibit_03 (Ấm men ngọc) | (−2,5, 12,5) | 180 | (−2,5, 10,5) | Bắc |
| Exhibit_04 (Bình hoa lam) | (2,5, 17,5) | 180 | (2,5, 15,5) | Bắc |
| Exhibit_05 (Tượng quan hầu) | (14, 0) | 270 | (11,5, 0) | Đông |
| Exhibit_Centerpiece (Nhà bia Tự Đức) | (0, 0), y = 0,25 | 180 | (0, −3,2) | Sảnh tròn |

- Bệ của nhà bia: `Platform_Centerpiece` là hình trụ bán kính 2,6, cao 0,25, tâm (0, 0,125, 0).
- Giữ nguyên bán kính anchor (3 và 3,5), collider, collider trigger và các model con.

### Đèn hiện vật (Realtime, đặt dưới root `Lighting`, giữ đúng tên)

- `Spot_Exhibit_XX`: đặt ở độ cao trần phòng trừ 0,3 m. Lệch 2 m về phía chỗ đứng xem. Quay bằng LookAt về tâm bounds của model.
- `Spot_Centerpiece_L` ở (−2,5, 7,5, −3) và `Spot_Centerpiece_R` ở (2,5, 7,5, −3). Cả hai LookAt (0, 1,35, 0).
- Sau khi di chuyển đèn, chạy lại `Docs/scene_wiring/fx_b_lights.cs.txt`.

### Người chơi

Đặt `Player` ở (0, 0, −14), yaw 0.

### Ghế dài trong sảnh tròn

Có 4 ghế cong (hoặc ghế thẳng) ở bán kính 4,2, tại các góc 45°, 135°, 225° và 315°. Các trục N, S, E, W phải để trống.

### Đường đi để kiểm tra (test dùng đúng các điểm này)

- Vòng tròn đi bộ bán kính 6: S (0, −6), SW (−4,24, −4,24), W (−6, 0), NW (−4,24, 4,24), N (0, 6), NE (4,24, 4,24), E (6, 0), SE (4,24, −4,24).
- Điểm cửa: W (−9, 0), N (0, 9), E (9, 0). Điểm tiền sảnh: (0, −9).
- Điểm ngay sau cửa, trong phòng: W (−10,5, 0), N (0, 10,5), E (10,5, 0). Đường đi luôn qua điểm này trước khi tới chỗ đứng xem, để không quẹt vào mép cửa.
- Từ chỗ xuất phát (0, −14), đi qua (0, −9) rồi tới S. Đi quanh vòng tròn tới điểm gần cửa, qua cửa, rồi tới chỗ đứng xem.
- Nhà bia: đi từ S tới (0, −3,2).

### Chi tiết hình học (T4, chốt ngày 16/10)

Mục này ghi đè các số ở bảng "Không gian" khi hai chỗ khác nhau.

- **Mặt trong tường phòng.**
  - Phòng Tây: x từ −20,2 tới −9,0.
  - Phòng Bắc: z từ 9,0 tới 20,2.
  - Phòng Đông: x từ 9,0 tới 20,2.
  - Tường gần sảnh của mỗi phòng dày 0,4 m, nằm ngoài mặt trong (Tây: x từ −9,4 tới −9,0).
  - Mặt trong và độ dày các tường còn lại theo cùng quy tắc.
- **Hành lang nối sảnh với phòng.**
  - Rộng 3,0 m, trần phẳng ở y = 4,5.
  - Hai tường bên ở z = ±1,5 (với phòng Bắc thì ở x = ±1,5). Hai tường này chạy từ mặt ngoài tường sảnh (r = 8,4) tới mặt ngoài tường phòng.
  - Mỗi đầu hành lang có một `ArchSpandrel` ở y = 3,0. Mặt mesh quay theo hướng đi.
- **Tường sảnh tròn.**
  - Là mesh vành khuyên cong: bán kính trong 8,0, ngoài 8,4, cao 0 tới 8. Mỗi đoạn cong 2°. UV tính theo mét.
  - Mỗi cửa có nửa góc là asin(1,5 / 8) ≈ 10,8°. Bên dưới y = 3 ở chỗ cửa thì để trống.
  - Từ y = 4,5 tới y = 8 ở chỗ cửa là một khối cong lấp.
  - Từ y = 3 tới y = 4,5 ở chỗ cửa là một `ArchSpandrel`, đặt tiếp tuyến tại r = 8,2.
- **Mái sảnh.**
  - Vành trần phẳng ở y = 8, dày 0,3, từ r = 5 tới r = 8,4.
  - Đĩa giếng trời bán kính 5 ở y = 8,3, dùng vật liệu `M_R_Skylight`.
  - Lưới thanh mảnh 0,06 m, cách nhau 1 m, ở y = 8,25, dùng vật liệu `M_R_TrackBlack`.
  - Gờ (cornice) vòng quanh ở y = 7,7 tới 7,8, nhô vào trong 0,12 m.
- **Sàn.**
  - Đĩa sảnh bán kính 8,4 lát `M_R_FloorMarble`.
  - Vòng trang trí trên sàn từ r = 5,5 tới r = 5,8, nổi lên y = 0,002, dùng vật liệu `M_R_TextPlate`.
  - Mỗi phòng, hành lang và tiền sảnh có một tấm sàn riêng.
  - Thêm một `BoxCollider` sàn chung, không có renderer, phủ x từ −21 tới 21, z từ −17 tới 21, mặt trên ở y = 0.
- **Tiền sảnh.**
  - Cao 5 m. Tường hai bên ở x = ±3, chạy từ z = −16 tới z = −7,3 để ăn vào tường sảnh.
  - Tường cuối ở z = −16.
- **Cách dựng khối hộp.**
  - Dùng mesh hộp có UV theo mét, không kéo giãn cube.
  - Mỗi kích thước lưu thành một mesh asset riêng trong `Assets/Museum/Decor/Meshes/Arch/`, tạo UV2.
  - Kiến trúc đặt lightmap-static và có collider: `BoxCollider` cho hộp, `MeshCollider` cho mesh cong.

## Chữ trên tường (tiếng Việt trước, tiếng Anh dưới, nhỏ hơn)

| Vị trí | Dòng 1 | Dòng 2 |
|---|---|---|
| Trên cửa W, mặt quay vào sảnh | ĐIÊU KHẮC CHĂM | Cham Sculpture |
| Trên cửa N | GỐM SỨ VIỆT NAM | Vietnamese Ceramics |
| Trên cửa E | LĂNG TẨM NHÀ NGUYỄN | Nguyen Dynasty Royal Tombs |
| Trên cửa S, mặt quay vào sảnh | BẢO TÀNG DI SẢN VIỆT NAM | Vietnamese Heritage Museum |
| Tường đầu tiên bên phải khi vào mỗi phòng | Tên phòng (giống trên) | Dòng phụ tiếng Anh |

Chữ màu vàng nhạt #E9D9A6 trên nền tối, hoặc màu nâu đậm #3A2A1A trên nền sáng. Độ tương phản ≥ 4,5:1.

## Đồ trang trí theo phòng (không có bảng tên)

- **Sảnh tròn:** cột bổ tường (pilaster) giữa các cửa, gờ tường, giếng trời lưới kính phát sáng, 4 ghế gỗ.
- **Tiền sảnh:** trống đồng Đông Sơn (`Models/DongSonDrum`) đặt trên bệ tròn ở (0, −11). Bệ bán kính 0,6, cao 0,9. Mặt trước bệ có bảng ghi công: "Trống đồng Đông Sơn — mô hình: Aaannnn, CC BY 4.0". Đây là ghi công bắt buộc của giấy phép, không phải bảng tên hiện vật.
- **Phòng Chăm:**
  - Cột áp tường bằng gạch đỏ.
  - Khối đá sa thạch kiểu bệ thờ (chồng các khối có gờ) dọc hai tường dài.
  - Tấm phù điêu đá phẳng trên tường cuối.
  - 2 ghế gỗ.
- **Phòng Gốm:**
  - Tủ kính áp tường dọc tường x = ±5,6 và tường cuối z = 19,8. Đế gỗ tối, kính trong suốt, 3 tầng kệ.
  - Đồ gốm dạng tròn xoay đặt trong tủ: bát, đĩa trên giá, hũ, bình, chum nhỏ. Men ngọc #9DB8A0, men trắng lam (nền trắng #EDEBE3 và một số món men lam #2C4A8C), men nâu #6B3F22.
  - 2 ghế gỗ.
- **Phòng Nguyễn:**
  - 2 hàng cột sơn son có đai vàng ở z = ±3,5, x = 10, 13, 16, 19.
  - Bình phong gỗ ở tường cuối.
  - Đỉnh đồng (lư hương) và vạc đồng dạng tròn xoay, màu đồng #7A5A2E, kim loại.
  - Đèn lồng treo.
  - 2 ghế gỗ.
- **Mọi phòng:** ray đèn trên trần (thanh mảnh và các đèn nhỏ, chỉ để trang trí).

## Ánh sáng

- **Sảnh tròn:** giếng trời phát sáng, cộng các area light Baked dưới giếng trời và quanh vành tường.
- **Mỗi phòng:** một lưới area light Baked trên trần, khoảng 3 m một đèn, cường độ ấm (3000–3500 K).
- **Light probe:** một `LightProbeGroup` phủ mọi khối, lưới khoảng 2 m, ở 3 độ cao (0,5; 1,6; 3,0).
- **Reflection probe:** mỗi khối một cái (sảnh tròn, tiền sảnh, 3 phòng).
- Bake lại sau khi xong kiến trúc và đèn.
- Không đổi pipeline asset. Nếu buộc phải đổi, đổi cả `PC_*` và `Mobile_*`.

## Thứ tự công việc

| # | Việc | Phụ thuộc |
|---|---|---|
| T0 | Sao lưu scene và lighting data sang `C:\code\mon_backup_scene\*.bak3`. Thử một lần sửa scene nhỏ qua `execute_code` để xem có bị chặn không. | — |
| T1 | Viết test `Assets/Museum/Tests/EditMode/MuseumLayoutTests.cs`. Test phải fail với scene cũ. | T0 |
| T2 | Vật liệu cho kiến trúc, trang trí và men gốm, đặt trong `Assets/Museum/Materials/Rebuild/`. Texture CC0 từ Poly Haven, ghi vào `CREDITS.txt`. | T1 |
| T3 | Mesh tròn xoay (gốm, đỉnh, vạc, đèn lồng) và mesh vòm cửa, đặt trong `Assets/Museum/Decor/Meshes/`. | T1 (chạy song song với T2) |
| T4 | Kiến trúc: xóa khối cũ trong `Architecture`, dựng sảnh tròn, tiền sảnh, 3 phòng và cửa vòm. | T2, T3 |
| T5 | Dời hiện vật, bệ, đèn spot và Player. Chạy `fx_b`. | T4 |
| T6a–T6e | Trang trí sảnh tròn, tiền sảnh, phòng Chăm, phòng Gốm, phòng Nguyễn. Làm lần lượt (cùng một scene). | T5 |
| T7 | Chữ trên tường | T6 |
| T8 | Đèn baked, probe, bake | T7 |
| T9 | Chạy toàn bộ test EditMode và PlayMode. Chụp ảnh. Build release và chạy thử. | T8 |

## Test (T1)

File `MuseumLayoutTests.cs` mở scene thật ở chế độ chỉ đọc, giống `MuseumSceneWiringTests`. Gọi `Physics.SyncTransforms()` trước khi cast.

1. Mỗi anchor nằm trong khối phòng của nó (theo bảng trên).
2. Hình cầu bán kính của hai anchor bất kỳ không chồng lên nhau.
3. Đi được từ chỗ xuất phát tới từng chỗ đứng xem. Dùng `CapsuleCast` (bán kính 0,3, cao 1,7, bỏ qua trigger) giữa các điểm đường đi liên tiếp.
4. Từ mỗi chỗ đứng xem, tia ở độ cao mắt 1,6 m chiếu về tâm bounds của model trúng collider thuộc đúng anchor đó. Tia trúng cả trigger, giống `ExhibitTargeting`.
5. Mọi `TMP_Text` trong scene dùng font BeVietnamPro và không thiếu glyph.
6. Có chữ tiêu đề cho cả 3 phòng ("ĐIÊU KHẮC CHĂM", "GỐM SỨ VIỆT NAM", "LĂNG TẨM NHÀ NGUYỄN").
7. Mọi anchor nằm trong vùng bao của light probe.
8. Player nằm trong tiền sảnh.

Kiểm tra độ nhạy: chặn tạm một cửa bằng một khối có collider. Test 3 phải fail.

## Phần còn lại (kiểm tra của model chính)

- Chạy toàn bộ test EditMode và PlayMode. Báo kết quả bằng số.
- Đọc console, chỉ quan tâm loại error và warning.
- Chụp ảnh ở độ cao mắt: tiền sảnh, giữa sảnh tròn, 3 cửa, trong mỗi phòng. So với ảnh tham khảo.
- Build release `Builds/Windows/VirtualMuseum.exe`. Chạy thử với `-logFile` và đọc log.
- Không commit nếu người dùng chưa đồng ý.
- **Rủi ro:**
  - Thời gian bake.
  - Giới hạn số đèn realtime trên mỗi object của URP.
  - Chữ tiếng Việt thiếu glyph.
  - Collider của đồ trang trí chặn tia ngắm.
