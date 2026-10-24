# Chỉ mục tư liệu báo cáo INF1353 — đề 28

**Người đọc:** nhóm viết báo cáo. **Mục đích:** tìm nội dung và bằng chứng tương ứng với mục A của đề.

**Cập nhật:** 22/10/2026. Mở [Tư liệu viết báo cáo](../HO_TRO_VIET_BAO_CAO.md) để lấy nội dung, 18 bảng và 10 hình có chú thích.

## Bản đồ sử dụng

| Nội dung mục A | Nơi lấy trong tệp hỗ trợ | Trạng thái |
|---|---|---|
| Câu 1a–1b: bài toán, người dùng, mục tiêu, chức năng | Mục 2.1–2.2; bảng 2 | Đã tổng hợp |
| Câu 1c–1d: phi chức năng, ràng buộc, tiêu chí kiểm chứng | Mục 2.3–2.4; bảng 3–4 | Có ngưỡng và căn cứ lựa chọn |
| Câu 2a–2b: phương án và công nghệ | Mục 3.1–3.2; bảng 5–6 | Có bảng so sánh 3 phương án theo 5 tiêu chí |
| Câu 2c–2d: kiến trúc, luồng xử lý, môi trường, tương tác | Mục 3.3–3.4; bảng 7; hình 1–3 | Có đủ 3 sơ đồ và chú thích điều kiện phím E |
| Câu 3a: phát triển, tích hợp | Mục 4–5; bảng 8; hình 4–10 | Có 7 ảnh sản phẩm, mô tả quy trình và tổ chức tài nguyên |
| Câu 3b: kiểm thử | Mục 6.1; bảng 9–10 | 143/143 đạt; có JSON gốc từng ca; bảng đại diện đã đối chứng |
| Câu 3b: đo hiệu năng | Mục 6.2–6.5; bảng 11–15 | Có 3 CSV, mỗi tệp 5 điểm; đủ chỉ số và định nghĩa |
| Câu 3b: tối ưu và hạn chế | Mục 7–8; bảng 16–18 | Có số tam giác trước–sau và bộ nhớ texture ước tính |

*Ghi chú:* không lập chương riêng cho Câu 4 theo phạm vi người dùng đã chốt. Mục B và Phụ lục 02 chỉ là căn cứ đối chiếu. Chúng không biến dữ liệu đang có thành kết luận tự động đạt điểm A.

## Tệp dữ liệu và bằng chứng

| Tệp hoặc thư mục | Dùng cho nội dung | Cách đọc |
|---|---|---|
| [models.csv](models.csv) | Danh mục 18 mô hình, nguồn, giấy phép và số tam giác | Danh mục tài nguyên; không mặc định cả 18 đều là bản quét hoặc đều đang trưng bày |
| [models_summary.md](models_summary.md) | Cách tính giảm lưới và bộ nhớ texture | 85,4% giảm tam giác áp dụng cho 10 mô hình có số gốc; 67,2% giảm bộ nhớ là ước tính |
| [perf_run1.csv](perf_run1.csv), [perf_run2.csv](perf_run2.csv), [perf_run3.csv](perf_run3.csv) | Kết quả từng lượt benchmark | Thống kê theo đoạn đo, không phải toàn bộ mẫu thời gian từng khung hình |
| [perf_median.csv](perf_median.csv) | Bảng kết quả chính | Trung vị từng cột tại từng điểm qua ba lượt |
| [Ảnh](../img) | Môi trường, bảng thông tin và xem gần | 7 ảnh đã có; không còn chờ chụp theo B8 cũ |
| [Sơ đồ](../img/diagrams) | Kiến trúc, trạng thái, mặt bằng | 3 PNG đã có; HTML và JSON là nguồn đi kèm |
| [Log lượt 1](../../../Builds/Windows/bench_check.log), [lượt 2](../../../Builds/Windows/bench_run2.log), [lượt 3](../../../Builds/Windows/bench_run3.log) | Môi trường chạy và đường dẫn CSV gốc | Log ở ngoài cuoi_ki, được liên kết tương đối trong repo |
| [Log khởi động](../../../Builds/Windows/smoke_player.log) | Player đã nạp engine và các hệ thống | Không chứng minh toàn bộ chức năng hoặc độ ổn định dài hạn |
| [results_editmode.json](results_editmode.json), [results_playmode.json](results_playmode.json) | Kết quả kiểm thử tổng và chi tiết | 78/78 EditMode và 65/65 PlayMode; 0 lỗi, 0 bỏ qua |

## Nguồn của con số kiểm thử

Luna đã chạy lại qua UnityMCP ngày **22/10/2026**, trên Unity **6000.3.26f1**: **EditMode 78/78** và **PlayMode 65/65**, tổng **143/143 đạt, 0 lỗi, 0 bỏ qua**. Có đủ 143 bản ghi chi tiết trong hai JSON liên kết ở bảng trên.

EditMode chạy khoảng 21:36; PlayMode chạy khoảng 21:37–21:38 (UTC+7). Lần khởi tạo trước đó timeout trước khi chạy ca nào; không tính thành test lỗi. Kết quả tổng và 14 hàng ca đại diện trong tệp hỗ trợ đã được đối chiếu với phản hồi MCP. Kiểm thử Editor không thay thế kiểm tra bản build hoặc demo trực tiếp.

## Phạm vi đã chốt

- Dùng đề 28 trong Phụ lục 01. Trọng tâm là ứng dụng di sản trên Windows và xử lý dữ liệu mô hình có sẵn.
- Nội dung yêu cầu, so sánh phương án, thiết kế và dữ liệu đo đã được tập hợp cho người viết.
- Chữ “bản quét” chỉ dùng theo bằng chứng tương ứng. DongSonDrum là mô hình dựng tay; Devata chưa có xác nhận nguồn quét từ metadata tác giả.
- Ghi bộ nhớ texture là ước tính. Không gọi các lựa chọn chuyển động là bằng chứng đã chống say.
- Video sẽ được quay sau. Không bổ sung thêm yêu cầu sản phẩm nộp trong lần tổng hợp này.
- Mẫu trường, thông tin sinh viên và phân công thực tế do nhóm điền khi biên soạn bản tiểu luận.

## Trạng thái Unity sau lượt kiểm thử

Editor đã về EditMode, scene `Museum` đang mở và không biên dịch. Lượt này chưa build lại hoặc kiểm tra hình ảnh trực tiếp.

So với trạng thái trước khi chạy, Git ghi nhận thêm thay đổi ở hai font `BeVietnamPro-Regular SDF.asset`, `BeVietnamPro-SemiBold SDF.asset` và hai tệp `ProjectSettings/EditorSettings.asset`, `ProjectSettings/ProjectSettings.asset`. Font có thêm atlas và ký tự; `m_EnterPlayModeOptions` đổi từ 0 sang 1, còn `preloadedAssets` chuyển thành danh sách rỗng. Log chưa đủ để xác định nguyên nhân. Các thay đổi được giữ nguyên, chưa hoàn tác; cần kiểm tra cấu hình trước lần build tiếp theo.
