# Thay đồ trang trí bằng bản quét thật

**Người đọc:** các subagent làm việc trong Blender và Unity.

**Mục đích:** đưa 7 bản quét lấy từ Sketchfab vào dự án, thay các đồ trang trí đang dựng bằng script. Tài liệu này ghi mọi con số để agent không phải tự chọn.

## Bảng model

| # | Thư mục `Assets/Museum/Models/` | Sketchfab UID | Tác giả, giấy phép | Mặt gốc | Ngân sách tam giác | Kích thước trưng bày (chiều lớn nhất) | Vật liệu |
|---|---|---|---|---|---|---|---|
| S1 | `TamChucDrum` | 2d969ba1ae734ab28019ab0e48244188 | atlantictruong19, CC BY 4.0 | 205k | 150k | 0.70 m (đường kính mặt trống) | đồng |
| S2 | `CaoDinh` | a3357b6bbb384ea8bf185f3c2e1a9d30 | vudn54, CC BY 4.0 | 100k | giữ nguyên | 1.60 m (chiều cao) | đồng |
| S3 | `NgheThaiHoa` | 391d289bad2f48238c8f436d49dc60bb | atlantictruong19, CC BY 4.0 | 2.96M | 150k + bake normal | 1.10 m (chiều cao, gồm đế nếu có) | đồng |
| S4 | `CoveredJarBlueWhite` | 38e5877fc22d4a81b28b0388b26fe4e4 | Cleveland Museum of Art, CC0 | 94k | giữ nguyên | 0.23 m (cao thật, có nắp) | men gốm |
| S5 | `LordSivaMicmac` | 53511798f33a4ca8a2ef4cd9c5fbbb28 | micmac, CC BY 4.0 | 620k | 150k + bake normal | 0.90 m (chiều cao) | sa thạch |
| S6 | `HoiAnLantern` | 90517b64386c4022a826602814b342e0 | db4, CC BY 4.0 | 55k | giữ nguyên | 0.55 m (chiều cao) | vải, tre |
| S7 | `BronzeCoinJar` | c57e1b8957bd44fd904ec3c129bb3922 | atlantictruong19, CC BY 4.0 | 2.80M | 100k + bake normal | 0.40 m (chiều cao) | đồng |

S4 dùng bản có cả nắp (`1998.285 Covered Jar`). Không dùng hai bản `.a` và `.b`.

## Quy trình Blender (mỗi model một agent, chạy lần lượt vì chỉ có một Blender)

1. Xóa sạch scene Blender (mọi object, mesh, material, image mồ côi).
2. `import_asset` từ Sketchfab với UID trong bảng. Ghi lại số tam giác và số material.
3. Gỡ phần không thuộc hiện vật: nền đất, bệ quét dính kèm, mảnh rời nhỏ hơn 1% số tam giác. Ghi lại phần đã gỡ. Giữ đế đá hoặc đế gỗ nếu là một phần của hiện vật.
4. Gộp thành một object. Áp dụng mọi transform.
5. Đặt hướng: mặt chính của hiện vật nhìn về **−Y** của Blender, trục đứng là **+Z**.
6. Đặt gốc ở **tâm đáy** (x, y giữa bounds, z = min). Đưa gốc về (0,0,0).
7. Scale theo cột "Kích thước trưng bày".
8. Nếu số tam giác lớn hơn ngân sách: nhân bản mesh gốc làm high, dùng Decimate (Collapse) trên bản low tới đúng ngân sách, rồi bake normal tangent space từ high sang low (4096, cage extrusion nhỏ, Non-Color). Xóa high sau khi bake.
9. Texture: dùng ảnh gốc. Ảnh lớn hơn 4096 thì thu về 4096. Tên file theo mẫu `<Thư mục>_BaseColor[_n].png|jpg`, `_Normal`, `_MetallicSmoothness` (R = metallic, A = 1 − roughness), `_AO`. Chỉ xuất kênh nào nguồn có thật hoặc vừa bake.
10. Kiểm tra pháp tuyến: không có mặt lật. Đếm số tam giác có pháp tuyến hình học ngược với pháp tuyến đỉnh. Phải bằng 0, hoặc dưới 1% với mesh quét có lỗ.
11. Xuất FBX vào `C:\code\mon\Assets\Museum\Models\<Thư mục>\<Thư mục>.fbx`. Dùng `apply_scale_options='FBX_SCALE_ALL'`, `axis_forward='-Z'`, `axis_up='Y'`, `use_selection=True`, `path_mode='COPY'` và `embed_textures=False`. Lưu texture cạnh file FBX.
12. Viết `SOURCE.txt` theo mẫu `Assets/Museum/Models/ChamShiva/SOURCE.txt`. Gồm Title, Author, Licence (kèm dòng credit đầy đủ và URL), Scan check (đọc mô tả và tag trên `https://api.sketchfab.com/v3/models/<UID>` rồi ghi kết luận "photogrammetry", "LiDAR", "không rõ" hoặc "không phải bản quét"), số mặt nguồn và số mặt xuất, danh sách texture và vật liệu nên dùng trong Unity.
13. Chụp viewport (`look`) một ảnh để kiểm tra hướng và texture.

## Vị trí trong Unity (một agent sau khi xong cả 7 model)

Mọi món là đồ trang trí dưới `Decor/<Phòng>`. Không thêm `NarrativeAnchor`. Không đặt static. Layer `Ignore Raycast` nếu nằm trong vòng 4 m quanh hiện vật chính. Collider: BoxCollider theo bounds cho món đặt trên sàn, không collider cho món trong tủ hoặc treo.

| # | Vị trí (x, y đáy, z) | Yaw (mặt chính nhìn về) | Thay cho |
|---|---|---|---|
| S1 | (0, 0.90, −11) trên `Decor/Vestibule/DrumPedestal` | 180 (nhìn về −z, phía người vào) | `Decor/Vestibule/DongSonDrum` (tắt, không xóa) |
| S2 | (17.5, 0, −5.2) | 0 (nhìn về −x hoặc giữa phòng; chọn hướng có hoa văn chính) | `Decor/Room_East_Nguyen/Cauldron_-5,2` (tắt) |
| S3 ×2 | (18.9, 0, −2.7) và (18.9, 0, 2.7) | quay về −x, hai con đối xứng (con z dương scale x −1 nếu cần) | không thay; mới |
| S4 | tâm kệ dưới của `Cab_N1`, thay món gốm ở giữa kệ dưới | 180 | một món gốm ở giữa kệ dưới `Cab_N1` (tắt) |
| S5 | (−16.15, 1.00, 5.60) trên `Altar_Top_-16,15_N` | 180 | không thay; mới |
| S6 ×4 | vị trí 4 đèn lồng cũ, đỉnh đèn treo dưới dây | 0 | 4 `Lantern` cũ (tắt; giữ dây) |
| S7 | trên một bệ mới 0.6 × 0.8 × 0.6 (M_R_Sandstone) tại (17.5, 0, 5.2) | 270 | `Decor/Room_East_Nguyen/Cauldron_5,2` (tắt) |

**Bảng ghi công:** mỗi món CC BY cần một tấm nhỏ cùng kiểu với `Decor/Vestibule/CreditPlate` + `CreditText` (font BeVietnamPro, màu #E9D9A6). Nội dung: tên tiếng Việt, rồi dòng credit `"<Title>" by <Author>, CC BY 4.0`, rồi URL rút gọn `sketchfab.com/3d-models/<UID>`. Đèn lồng S6 dùng chung một tấm ở tường gần cửa phòng Nguyễn. S4 (CC0) vẫn có nhãn tên, ghi "Bảo tàng Nghệ thuật Cleveland, CC0".

S1 thay trống cũ, nên tấm ghi công ở tiền sảnh phải đổi nội dung sang trống Tam Chúc. Bỏ dòng "mô hình của nghệ sĩ".

## Kiểm tra cuối

- EditMode và PlayMode đều đạt (hiện tại 65/65 và 54/54).
- Chụp ảnh tiền sảnh, phòng Chăm, phòng Gốm, phòng Nguyễn.
- Bake lại ánh sáng (đồ mới không static nên chỉ cần probe; bake lại để reflection probe thấy đồ mới).
- Build release, chạy player, đọc log.
