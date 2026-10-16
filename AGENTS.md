# Repository Guidelines

## Project Structure & Module Organization

Đây là dự án Unity 6000.3.26f1 dùng URP. Scene chính là `Assets/Museum/Scenes/Museum.unity`; scene mẫu `Assets/Scenes/SampleScene.unity` không nằm trong bản build. Mã bảo tàng ở `Assets/Museum/Scripts/` (assembly `Museum.Runtime`), dữ liệu hiện vật ở `Assets/Museum/Content/`, còn model và thông tin giấy phép ở `Assets/Museum/Models/<Exhibit>/`. Test nằm trong `Assets/Museum/Tests/EditMode/` và `PlayMode/`. Lời thuyết minh gốc ở `Docs/narration_vi.json`.

## Build, Test, and Development Commands

Mở dự án bằng Unity Hub với phiên bản 6000.3.26f1, sau đó mở scene chính và nhấn Play. Không mở cùng dự án trong hai tiến trình Unity.

Để chạy EditMode khi Unity Editor đã đóng:

```powershell
& 'C:\Program Files\Unity\Hub\Editor\6000.3.26f1\Editor\Unity.exe' -batchmode -projectPath (Get-Location).Path -runTests -testPlatform EditMode -testResults results.xml
```

Đổi `EditMode` thành `PlayMode` để chạy bộ test còn lại. Thêm `-testFilter Namespace.Class.Method` để chạy một test. Build Windows x86_64 qua **File → Build Profiles**; sau build, chạy player để xác nhận nó khởi động. Dự án không có lệnh lint riêng.

## Coding Style & Naming Conventions

Dùng C# theo cấu trúc hiện có: namespace `Museum`, thụt lề 4 dấu cách, lớp và phương thức `PascalCase`, trường private serialize bằng `camelCase`. Giữ logic dùng chung trong `Museum.Runtime`; dùng Input System mới, không dùng `UnityEngine.Input`. Văn bản tiếng Việt trong UI phải dùng TMP font Be Vietnam Pro.

## Testing Guidelines

Dùng Unity Test Framework. Test EditMode cho logic thuần và kiểm tra cấu hình scene; PlayMode cho hành vi runtime và input. Tên test theo mẫu `Method_Condition_ExpectedResult`. Thêm test hồi quy cho lỗi đã sửa và kiểm tra cả nhánh hợp lệ lẫn đầu vào lỗi liên quan. Không coi log lỗi được test chủ ý là lỗi ngoài dự kiến.

## Commits & Pull Requests

Lịch sử gần đây dùng tiêu đề commit ngắn bằng tiếng Việt, mở đầu bằng hành động hoặc phạm vi, ví dụ `Sửa test di chuyển PlayMode: ...`. Giữ commit tập trung. Pull request nên nêu thay đổi, scene hoặc cấu hình bị ảnh hưởng, test đã chạy và kết quả; kèm ảnh hoặc video khi thay đổi hình ảnh hay tương tác trong Unity.

## Project-Specific Safety

Không sửa `Library/`, `Temp/`, `Logs/` hoặc các file `.sln`/`.csproj` do Unity sinh. Thay đổi render phải cập nhật cấu hình PC và Mobile. Sau khi di chuyển kiến trúc hoặc đèn, bake lại ánh sáng. Giữ `SOURCE.txt` và giấy phép đi cùng tài nguyên 3D.
