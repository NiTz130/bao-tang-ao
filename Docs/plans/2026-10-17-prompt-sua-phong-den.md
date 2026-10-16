# Prompt: sửa 3 phòng bị đen sau khi bake

Đọc trước: `CLAUDE.md` và `Docs/plans/2026-10-16-rotunda-rebuild.md`. Mọi việc với Unity đều làm qua UnityMCP. Scene cần sửa: `Assets/Museum/Scenes/Museum.unity`.

## Bối cảnh

- Scene đã được dựng lại, gồm sảnh tròn, tiền sảnh, 3 phòng và 3 hành lang. Ánh sáng đã bake: có 41 đèn Area Baked, 427 light probe và 5 reflection probe.
- Lần chạy gần nhất, EditMode đạt 65/65.
- **Lỗi:** mọi vật dùng lightmap trong 3 phòng (`Architecture/Room_*`) và 3 hành lang (`Architecture/Corridor_*`) đều đen, kể cả các bệ Cube có sẵn trong phòng. Lỗi có ở Edit mode, ở Play mode và vẫn còn sau khi khởi động lại Unity. Sảnh tròn và tiền sảnh hiển thị bình thường.
- **Những gì đã kiểm tra là đúng:**
  - Texture lightmap đọc trên CPU: sàn phòng sáng khoảng 0.89.
  - Dir map.
  - UV2 của mesh.
  - `lightmapIndex` và `lightmapScaleOffset`.
  - Độ sáng light probe trong phòng bằng 0.59 lần sảnh tròn.
- **Thí nghiệm đáng chú ý:** khi gán tạm `lightmapScaleOffset` của sàn tiền sảnh cho sàn phòng, sàn phòng sáng lên. Vậy vùng lightmap dành cho phòng bị đọc ra đen trên GPU, dù dữ liệu trên đĩa vẫn sáng.

## Lưu ý bắt buộc

- **Không gọi `Thread.Sleep` trong `execute_code`.** Lệnh này chặn main thread nên bake không chạy được. Khi chờ bake, gọi `Lightmapping.isRunning` nhiều lần, mỗi lần một lệnh riêng.
- **Hạn chế gọi `Camera.Render` liên tiếp.** Driver D3D12 đã làm Editor crash một lần. Mỗi lệnh chỉ chụp tối đa 2 ảnh.
- Nếu gặp lỗi `ReflectionProbeManager` NullReference hoặc ảnh trắng toàn bộ sau khi xoá probe, mở lại scene. Nguyên nhân là URP còn giữ cache probe cũ.
- Không sửa `Assets/Settings/PC_*` và `Mobile_*`, test, mã nguồn hay vị trí object. Chỉ được sửa dữ liệu ánh sáng và các trường nêu trong từng bước.
- Không commit.

## Bước 1: bake lại từ đầu

1. Chạy `Lightmapping.Clear()`. Xoá mọi file sinh ra trong `Assets/Museum/Scenes/Museum/` (`Lightmap-*`, `ReflectionProbe-*`, `LightingData.asset`), rồi chạy `AssetDatabase.Refresh()`.
2. Mở lại scene.
3. Chạy `Lightmapping.BakeAsync()`. Chờ bằng cách gọi `isRunning` nhiều lần, giới hạn 25 phút. Sau đó lưu scene.
4. Chụp 3 ảnh ở độ cao 1.6 m:
   - Từ (−9.5, 1.6, 0) nhìn về −x.
   - Từ (0, 1.6, 9.5) nhìn về +z.
   - Từ (9.5, 1.6, 0) nhìn về +x.
5. **Nếu 3 phòng đã sáng:** sang bước 2.
6. **Nếu vẫn đen:** chẩn đoán theo thứ tự dưới đây. Sửa nguyên nhân đầu tiên tìm được, bake lại, rồi chụp lại.
   - **a. Import setting của lightmap.** So sánh `Lightmap-N_comp_light.exr` mà phòng dùng với lightmap mà sảnh dùng. Kiểm tra textureType, format, compression và mipmap.
   - **b. Flag static của renderer.** Dump mọi property của MeshRenderer `Room_West_Cham/Floor` và `Vestibule/Floor` bằng `SerializedObject`. In ra các trường khác nhau, chú ý `m_ScaleInLightmap`, `m_StitchLightmapSeams` và `m_ReceiveGI`.
   - **c. Đèn Area của phòng.** Kiểm tra từng đèn: `lightmapBakeType` là Baked, đèn đang bật (enabled), `cullingMask` có layer Default, kích thước khác 0.
   - **d. Lightmap có chia nhiều vùng không.** Tạm tắt sảnh và tiền sảnh, bake riêng một phòng. Nếu phòng sáng, lỗi nằm ở cách xếp lightmap. Khi đó đổi `lightmapMaxSize` sang 4096, hoặc giảm `lightmapResolution` xuống 8.
   - Sau 2 lần bake mà vẫn đen thì **dừng lại và báo cáo**. Không thử bừa.

## Bước 2: hiện vật nhận ánh sáng light probe

- Kiểm tra và sửa mọi `Renderer` con của `Exhibit_01..05` và `Exhibit_Centerpiece`:
  - `lightProbeUsage = BlendProbes`.
  - `receiveGI = LightProbes`.
  - `probeAnchor` để trống, hoặc trỏ vào chính hiện vật đó.
- In giá trị trước và sau khi sửa.
- Tắt tạm các `Lighting/Spot_*` và chụp `Exhibit_01`. Hiện vật phải vẫn nhìn thấy được, không đen hẳn. Bật lại các spot rồi lưu scene.

## Bước 3: đồ đồng ánh xanh

- Chụp `Decor/Room_East_Nguyen/IncenseBurner` và `Cauldron_*` từ (11.5, 1.6, 0), nhìn về +x.
- Nếu đồ đồng vẫn ánh xanh: giảm `_Smoothness` của `M_R_Bronze` từ 0.45 xuống 0.3 và giữ nguyên màu `#7A5A2E`. Sau đó chụp lại.

## Bước 4: kiểm tra cuối

1. Chạy `run_tests` cho EditMode và PlayMode. Cả hai bộ phải đạt hết.
   - Test PlayMode `Director_MissingWiring_LogsErrorAndDisables` cố ý ghi một dòng lỗi vào log. Đây không phải lỗi thật.
2. **Kiểm tra độ nhạy của test đi bộ:**
   - Thêm tạm một khối có BoxCollider 3×4×0.5 m ở (−8.6, 2, 0) để chặn cửa phòng Tây.
   - Chạy test `EveryExhibit_IsReachableOnFoot`. Test phải đỏ.
   - Xoá khối, chạy lại test. Test phải xanh.
   - Không lưu scene khi khối còn trong scene.
3. Build bản release Windows 64-bit bằng `manage_build` ra `Builds/Windows/VirtualMuseum.exe`, dùng Unity 6000.3.26f1.
4. Chạy player:
   ```
   VirtualMuseum.exe -logFile C:\code\mon\Builds\player.log
   ```
   Chờ khoảng 20 giây rồi tắt player. Trong log không được có `Exception`, `error` của shader hay crash.
5. Gọi `read_console` với loại error và warning.

## Báo cáo phải có

- Kết quả từng bước: đạt, không đạt, hoặc dừng kèm lý do.
- Nguyên nhân gốc của lỗi phòng đen, nếu tìm ra.
- Đường dẫn các ảnh đã chụp.
- Kết quả test dạng số: chạy / đạt / lỗi / NOT RUN cho EditMode và PlayMode. Nêu lý do cho mỗi mục NOT RUN.
- Kết quả build, và các dòng đáng chú ý trong `player.log`.
- Mọi thay đổi so với prompt này, kèm lý do.
