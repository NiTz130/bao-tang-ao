# Bảo tàng ảo — di sản Việt Nam bằng photogrammetry

Tài liệu này dành cho giảng viên và thành viên nhóm cần mở source, chạy thử, kiểm thử và build lại sản phẩm.

Dự án là bảo tàng ảo góc nhìn thứ nhất (Đề tài 28: Bảo tồn di sản văn hóa bằng photogrammetry). Người xem khám phá sảnh tròn và ba phòng, đọc thuyết minh tiếng Việt và xem gần sáu hiện vật chính. Không gian còn có các mô hình trưng bày bổ sung.

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

1. Giải nén bộ source. Nếu tải từ GitHub, clone repo:
   ```
   git clone https://github.com/NiTz130/bao-tang-ao.git
   ```
2. Mở Unity Hub. Chọn **Add → Add project from disk**, rồi chọn thư mục vừa clone.
3. Chờ Unity import model, texture và các gói phụ thuộc. Lần mở đầu cần kết nối mạng để tải gói.
4. Mở scene `Assets/Museum/Scenes/Museum.unity`.
5. Bấm **Play**.

Thư mục `Library/` không có trong repo. Unity tự tạo lại thư mục này ở lần mở đầu tiên.

## Điều khiển

| Phím | Tác dụng |
|---|---|
| W A S D | Đi lại |
| Chuột | Nhìn xung quanh |
| Shift | Đi nhanh |
| 1 | Bật hoặc tắt bút laze, tăng tầm ngắm từ 4 m lên 20 m |
| E | Nhắm vào hiện vật rồi nhấn E để xem gần. Khi không nhắm, E mở rộng hoặc thu gọn câu chuyện |
| Kéo chuột trái | Xoay hiện vật khi đang xem gần |
| Cuộn chuột | Phóng to hoặc thu nhỏ hiện vật khi đang xem gần |
| E hoặc Esc | Thoát chế độ xem gần |
| Esc | Mở khóa con trỏ chuột. Click vào cửa sổ để khóa lại |
| F9 | Bắt đầu hoặc dừng ghi hiệu năng; khi dừng, lưu kết quả CSV |

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

Các mô hình bổ sung gồm trống đồng Tam Chúc, Cao Đỉnh, nghê điện Thái Hòa, hũ men lam có nắp, tượng Shiva, đèn lồng Hội An và hũ tiền đồng. Nguồn và giấy phép nằm cạnh từng model.

**Tài nguyên khác:**

- Font Be Vietnam Pro: SIL Open Font License (`Assets/Museum/Fonts/OFL.txt`).
- Texture sàn và tường: Poly Haven, CC0 (`Assets/Museum/Textures/CREDITS.txt`).

## Cấu trúc thư mục

| Đường dẫn | Nội dung |
|---|---|
| `Assets/Museum/Scenes/Museum.unity` | Scene chính, là scene duy nhất được build |
| `Assets/Museum/Scripts/` | Code C# (assembly `Museum.Runtime`) |
| `Assets/Museum/Content/` | Một file `ExhibitInfo` cho mỗi hiện vật: tiêu đề, tóm tắt, câu chuyện |
| `Assets/Museum/Models/` | Model, texture và giấy phép tài nguyên |
| `Assets/Museum/Tests/` | Test EditMode và PlayMode |
| `Docs/narration_vi.json` | Bản gốc của lời thuyết minh tiếng Việt |
| `Packages/` | Danh sách và phiên bản gói phụ thuộc |
| `ProjectSettings/` | Cấu hình Unity và scene được build |

## Chạy test

1. Trong Unity, mở **Window → General → Test Runner**.
2. Chọn tab **EditMode**, rồi bấm **Run All**. Bộ test hiện có 78 ca.
3. Chọn tab **PlayMode**, rồi bấm **Run All**. Bộ test hiện có 65 ca.

Chạy PlayMode trong Editor ngoài chế độ batch. Bốn test hiển thị laze từng thất bại trong batch nhưng đạt khi chạy ngoài batch.

Một số test PlayMode cố ý ghi lỗi vào Console, ví dụ "NarrativeDirector: player, panel and actions must be assigned.". Các dòng này là bình thường.

## Build bản chạy

1. Mở **File → Build Profiles**.
2. Chọn **Windows**, kiến trúc **x86_64**.
3. Bấm **Build** và chọn thư mục `Builds/Windows/`.
4. Chạy file `.exe` vừa build để kiểm tra. Unity báo "Build Succeeded" chưa chắc là player khởi động được.

## Đo hiệu năng

Nhấn F9 hai lần để bắt đầu rồi kết thúc một lượt ghi khi đang tham quan. Chương trình lưu CSV trong `Application.persistentDataPath`; log ghi đường dẫn cụ thể.

Để đo tự động tại năm vị trí, chạy trong thư mục bản build:

```powershell
.\VirtualMuseum.exe -benchmark
```

Chương trình đợi 2 giây và đo 10 giây tại mỗi vị trí, sau đó xuất CSV và thoát. CSV có FPS trung bình, thời gian khung hình, p95/p99 và 1% low. Bộ đếm không khả dụng có giá trị `-1`; không coi đó là kết quả đo hợp lệ.

## Lưu ý

- Chỉ dùng Input System mới. Không dùng `UnityEngine.Input`.
- Thay đổi về hiển thị phải áp dụng cho cả hai bộ cấu hình `Assets/Settings/PC_*` và `Assets/Settings/Mobile_*`.
- Sau khi di chuyển tường, trần hoặc đèn, phải bake lại ánh sáng (**Window → Rendering → Lighting → Generate Lighting**).
- Chữ tiếng Việt phải dùng font asset Be Vietnam Pro. Font mặc định LiberationSans không có dấu tiếng Việt.
