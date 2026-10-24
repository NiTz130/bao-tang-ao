# Tóm tắt tối ưu tài nguyên mô hình (trước và sau)

Người đọc: nhóm làm báo cáo cuối kỳ. Mục đích: lấy số liệu "trước/sau" cho danh mục 18 mô hình.
Bảng chi tiết nằm ở `models.csv` (cùng thư mục).

Đây là danh mục tài nguyên, không phải số bản quét hoặc số đối tượng đang hoạt động trong scene. Nội dung và bảng dùng để viết báo cáo nằm trong [tệp hỗ trợ](../HO_TRO_VIET_BAO_CAO.md).

## Số liệu lấy từ đâu

| Nguồn | Cột / số liệu |
|---|---|
| `SOURCE.txt` (người làm mô hình ghi) | tác giả, giấy phép, tình trạng scan, `tris_original`, `tris_shipped_source`, `tex_original` |
| Đo trong Unity (AssetDatabase, chỉ đọc) | `tris_actual`, `verts_actual`, kích thước ảnh sau import, định dạng nén |
| Đọc từ đĩa | `mesh_mb` (file FBX), `tex_mb` (tổng file ảnh); đơn vị MiB (1 MiB = 1.048.576 byte) |
| Tự tính | bộ nhớ texture (xem dưới) |

Kiểm tra chéo CSV: `tris_actual` khớp `tris_shipped_source` ở cả 18 mô hình. Nguồn của hai cột được ghi trong bảng trên; lần tổng hợp báo cáo không đo lại mesh trong Unity.

## Tam giác (triangles)

- Tổng sau xử lý trong danh mục: **2.053.588** tam giác, **1.477.936** đỉnh, 18 mô hình.
- Chỉ **10 hiện vật** có số tam giác gốc trong `SOURCE.txt`. Với 10 hiện vật này: **10.450.845 → 1.529.239**, giảm **85,4%**.
- 8 hiện vật còn lại (524.349 tam giác sau tối ưu) không có số gốc, nên không đưa vào phép so sánh.

Ba mô hình nặng nhất lúc gốc:

| Mô hình | Gốc | Sau | Giảm |
|---|---|---|---|
| NgheThaiHoa | 2.956.829 | 149.999 | 94,9% |
| BronzeCoinJar | 2.802.032 | 100.000 | 96,4% |
| TuDucStele | 1.918.731 | 300.000 | 84,4% |
| Cộng 3 mô hình | 7.677.592 | 549.999 | 92,8% |

## Texture

Ước tính bộ nhớ GPU = rộng x cao x byte mỗi điểm ảnh, cộng chuỗi mip (thêm khoảng 1/3). Định dạng đo trong Unity (Standalone): DXT1 = 0,5 byte/điểm (ảnh màu, AO), DXT5 và BC7 = 1 byte/điểm (normal, metallic-smoothness). Đây là ước tính, không phải số đo từ Profiler.

| Trường hợp | Có mip (MiB) | Không mip (MiB) |
|---|---|---|
| Gốc (ảnh màu theo cỡ gốc trong `SOURCE.txt` khi có, ảnh khác theo file trên đĩa) | 1.237,3 | 928,0 |
| File trên đĩa, nếu import không giới hạn cỡ | 965,3 | không tính |
| **Ước tính theo cấu hình sau import trong Unity** | **405,3** | **304,0** |

- Giảm so với "gốc": **67,2%** (có mip). Giảm so với "file trên đĩa": **58,0%**.
- Lý do giảm thêm: Unity import giới hạn 2048 px cho 9 hiện vật (ví dụ ChamShiva 8192 → 2048) và 4096 px cho 8 hiện vật. TuDucStele dùng 4096 px cho ảnh màu và 2048 px cho normal.
- Ba mô hình nặng nhất (bộ nhớ gốc 384 MiB → 128 MiB, giảm 66,7%): TuDucStele 256 → 64 (75%), NgheThaiHoa 64 → 32 (50%), BronzeCoinJar 64 → 32 (50%).
- Dung lượng đĩa: 18 file FBX = 76,58 MiB; ảnh = 612,57 MiB.
- Số vật liệu (đo từ Renderer): 1 slot cho 13 hiện vật; CeladonEwer, ChamShiva, CoveredJarBlueWhite, Devata có 2 slot; TuDucStele có 4 slot.

## Hiện vật thiếu số tam giác gốc

`SOURCE.txt` chỉ ghi số đã xuất, không ghi số gốc, ở 8 hiện vật: BottleVase, BuddhaHead, CeladonEwer, ChamDeifiedKing, Devata, DongSonDrum, GuardianLion, KhaiDinhStatue. Ô `tris_original` để trống, không đoán.
LordSivaMicmac không ghi cỡ texture gốc, nên ô `tex_original` cũng trống.

## Lưu ý khi trích dẫn

- DongSonDrum là mô hình nghệ sĩ tự dựng, không phải bản scan (theo `SOURCE.txt`).
- Devata chưa được metadata tác giả xác nhận nguồn quét. Một số mô hình khác được nhận định từ ảnh và lưới; xem `scan_status` trong CSV.
- CaoDinh, CoveredJarBlueWhite, HoiAnLantern gần như không decimate (giảm 10% hoặc 0%); phần tối ưu của chúng nằm ở texture (8192 → 4096).
- Số bộ nhớ trên chỉ cho đường Standalone (PC). Cấp Mobile có thể khác và chưa đo.
