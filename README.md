# Bảo tàng ảo — di sản Việt Nam bằng photogrammetry

Tài liệu này dành cho thành viên nhóm muốn tải dự án về, mở trong Unity, chạy thử và build lại.

Dự án là một bảo tàng ảo góc nhìn thứ nhất (Đề tài 28: Bảo tồn di sản văn hóa bằng photogrammetry). Người xem đi trong sảnh trưng bày 6 bản quét 3D thật của hiện vật Việt Nam. Khi đến gần một hiện vật, bảng thuyết minh tiếng Việt hiện ra.

## Chạy nhanh, không cần Unity

1. Vào mục **Releases** của repo này.
2. Tải file `VirtualMuseum-Windows.zip`.
3. Giải nén, rồi chạy `VirtualMuseum.exe`.

Bản build chỉ chạy trên Windows 64-bit.

## Mở dự án trong Unity

**Yêu cầu:** Unity **6000.3.26f1**. Cài đúng phiên bản này qua Unity Hub.

- Không dùng 6000.3.24f1. Bản release của phiên bản đó báo build thành công nhưng player không khởi động được.
- Không dùng Unity 6.6 trở lên. Dự án chưa được kiểm tra trên các phiên bản đó.

**Các bước:**

1. Clone repo:
   ```
   git clone https://github.com/NiTz130/bao-tang-ao.git
   ```
2. Mở Unity Hub. Chọn **Add → Add project from disk**, rồi chọn thư mục vừa clone.
3. Mở dự án. Lần đầu, Unity import khoảng 400 MB texture và model, nên có thể mất 10–20 phút.
4. Mở scene `Assets/Museum/Scenes/Museum.unity`.
5. Bấm **Play**.

Thư mục `Library/` không có trong repo. Unity tự tạo lại thư mục này ở lần mở đầu tiên.

## Điều khiển

| Phím | Tác dụng |
|---|---|
| W A S D | Đi lại |
| Chuột | Nhìn xung quanh |
| Shift | Đi nhanh |
| E | Nhắm vào hiện vật rồi nhấn E để xem gần. Khi không nhắm, E mở rộng hoặc thu gọn câu chuyện |
| Kéo chuột trái | Xoay hiện vật khi đang xem gần |
| Cuộn chuột | Phóng to hoặc thu nhỏ hiện vật khi đang xem gần |
| E hoặc Esc | Thoát chế độ xem gần |
| Esc | Mở khóa con trỏ chuột. Click vào cửa sổ để khóa lại |

## Hiện vật trưng bày

| Ô | Hiện vật | Tác giả bản quét | Giấy phép |
|---|---|---|---|
| 01 | Tượng Shiva khổ hạnh (Chăm, Bình Định, thế kỷ 14) | michelon_e (Sketchfab) | CC BY 4.0 |
| 02 | Vua Chăm được thần hóa (cuối thế kỷ 10) | Minneapolis Institute of Art | CC0 |
| 03 | Ấm men ngọc có nắp (thế kỷ 13–14) | Minneapolis Institute of Art | CC0 |
| 04 | Bình hoa lam dáng quả lê (thế kỷ 15) | Minneapolis Institute of Art | CC0 |
| 05 | Tượng quan hầu ở lăng Khải Định | eyesCloud3D (Sketchfab) | CC BY 4.0 |
| Trung tâm | Nhà bia lăng Tự Đức (Huế) | vashar (Sketchfab), dữ liệu CyArk / Open Heritage 3D | CC BY 4.0 |

Mỗi thư mục `Assets/Museum/Models/<Tên>/` có file `SOURCE.txt`. File này ghi đường dẫn gốc, giấy phép, số tam giác và các bước xử lý (giảm lưới, bake normal map).

**Tài nguyên khác:**

- Font Be Vietnam Pro: SIL Open Font License (`Assets/Museum/Fonts/OFL.txt`).
- Texture sàn và tường: Poly Haven, CC0 (`Assets/Museum/Textures/CREDITS.txt`).

## Cấu trúc thư mục

| Đường dẫn | Nội dung |
|---|---|
| `Assets/Museum/Scenes/Museum.unity` | Scene chính, là scene duy nhất được build |
| `Assets/Museum/Scripts/` | Code C# (assembly `Museum.Runtime`) |
| `Assets/Museum/Content/` | Một file `ExhibitInfo` cho mỗi hiện vật: tiêu đề, tóm tắt, câu chuyện |
| `Assets/Museum/Models/` | Model và texture của 6 hiện vật |
| `Assets/Museum/Tests/` | Test EditMode và PlayMode |
| `Docs/narration_vi.json` | Bản gốc của lời thuyết minh tiếng Việt |
| `Docs/plans/` | Kế hoạch các tính năng |

## Chạy test

1. Trong Unity, mở **Window → General → Test Runner**.
2. Chọn tab **EditMode**, rồi bấm **Run All**. Kết quả mong đợi: 55/55 đạt.
3. Chọn tab **PlayMode**, rồi bấm **Run All**. Kết quả mong đợi: 54/54 đạt.

Một số test PlayMode cố ý ghi lỗi vào Console, ví dụ "NarrativeDirector: player, panel and actions must be assigned.". Các dòng này là bình thường.

## Build bản chạy

1. Mở **File → Build Profiles**.
2. Chọn **Windows**, kiến trúc **x86_64**.
3. Bấm **Build** và chọn thư mục `Builds/Windows/`.
4. Chạy file `.exe` vừa build để kiểm tra. Unity báo "Build Succeeded" chưa chắc là player khởi động được.

## Lưu ý

- Chỉ dùng Input System mới. Không dùng `UnityEngine.Input`.
- Thay đổi về hiển thị phải áp dụng cho cả hai bộ cấu hình `Assets/Settings/PC_*` và `Assets/Settings/Mobile_*`.
- Sau khi di chuyển tường, trần hoặc đèn, phải bake lại ánh sáng (**Window → Rendering → Lighting → Generate Lighting**).
- Chữ tiếng Việt phải dùng font asset Be Vietnam Pro. Font mặc định LiberationSans không có dấu tiếng Việt.
