# Kịch bản thuyết trình: slide 1–6

**Người đọc:** người nói phần mở đầu (slide 1–6).
**Mục đích:** nói đúng, trôi chảy phần giới thiệu, bộ sưu tập, photogrammetry và công nghệ, rồi chuyển lời cho người nói phần chức năng.
**Thời lượng:** khoảng 4 phút 30 giây.
**Nguồn:** bản Canva `ThuyetTrinh_BaoTangAo (2).pptx` (16 slide, đọc ngày 17/10/2026) và các file `SOURCE.txt` trong `Assets/Museum/Models`.

Các file liên quan:

- Phần slide 7–16 và demo: [02_kich_ban_slide_07-16.md](02_kich_ban_slide_07-16.md).
- Câu hỏi và trả lời: [03_hoi_dap.md](03_hoi_dap.md). Câu hỏi về code: [04_hoi_dap_code.md](04_hoi_dap_code.md). Phần của bạn dễ bị hỏi nhất ở nhóm A, nhóm B và câu Q13.

---

## 1. Số liệu cần thuộc cho slide 1–6

| Nội dung | Số liệu |
|---|---|
| Không gian | Tiền sảnh (nam), sảnh tròn có giếng trời, 3 phòng: Tây = Chăm, Bắc = Gốm, Đông = Nguyễn |
| Hiện vật | 6 hiện vật chính có thuyết minh, 7 bản quét trang trí có bảng ghi công, tổng cộng 13 bản quét |
| Niên đại | Shiva: thế kỷ XIV. Vua Chăm: cuối thế kỷ X. Ấm men ngọc: thế kỷ XIII–XIV. Bình hoa lam: cuối thế kỷ XV |
| Giấy phép | CC0 (Bảo tàng Minneapolis, Bảo tàng Cleveland) hoặc CC BY 4.0 (tác giả trên Sketchfab, dữ liệu CyArk) |
| Nhà bia Tự Đức | Dữ liệu CyArk 2019, LiDAR kết hợp photogrammetry, mã DOI `10.26301/n06n-qa49` |
| Giảm lưới | Nhà bia: 1.918.731 → 300.000 tam giác (−84,36 %). Shiva: 698.012 → 150.000 (−78,51 %) |
| Phần mềm | Unity 6000.3.26f1, URP 17.3.0, Input System 1.20.0, Test Framework 1.6.0 |
| Phím | WASD, chuột, Shift, E, Esc, phím 1 (bút laze) |

---

## 2. Lời nói cho từng slide

Mỗi đoạn là lời đọc to. Dòng in nghiêng là ghi chú cho người nói.

### Slide 1. Mở đầu (30 giây)

Kính chào thầy và các bạn. Nhóm em gồm Châu Gia Bảo, Đinh Hoàng Phú và Nguyễn Lê Đức Bình, do thầy Đỗ Trí Nhựt hướng dẫn. Đề tài của nhóm là bảo tồn di sản văn hóa các di tích lịch sử bằng photogrammetry. Sản phẩm là một bảo tàng ảo chạy trên máy tính. Người xem đi lại trong bảo tàng, đọc thuyết minh tiếng Việt và cầm hiện vật lên xem gần. Trước hết, em xin nói vì sao nhóm chọn đề tài này.

### Slide 2. Lý do chọn đề tài (40 giây)

Khi xem di sản qua ảnh, chúng ta chỉ thấy vài góc mà người chụp đã chọn. Muốn xem mặt sau bức tượng hay chiếc nắp của cái ấm thì ảnh không giúp được. Vì vậy, nhóm đưa mô hình 3D của hiện vật vào một sảnh bảo tàng. Mỗi hiện vật có lời thuyết minh, và người xem tự xoay, tự phóng to để quan sát. Nhóm xin nói rõ: sản phẩm giúp mọi người tiếp cận và tìm hiểu di sản qua bản sao số. Sản phẩm không thay thế hồ sơ đo đạc chuyên môn. Vậy nhóm đặt mục tiêu cụ thể thế nào?

### Slide 3. Mục tiêu và phạm vi (50 giây)

Mục tiêu có ba phần. Thứ nhất là không gian: một tiền sảnh, một sảnh tròn có giếng trời, và ba phòng theo chủ đề: điêu khắc Chăm, gốm Việt và lăng tẩm nhà Nguyễn. Trong đó có sáu hiện vật chính có thuyết minh và bảy bản quét khác để trang trí. Thứ hai là tương tác: đi lại, đọc thuyết minh, nhắm chọn, dùng bút laze, xem gần, xoay, phóng to, rồi trở lại tham quan. Hiện vật luôn về đúng chỗ cũ. Thứ ba là nội dung: mỗi hiện vật có tên, niên đại, chất liệu, xuất xứ, nguồn, tóm tắt và câu chuyện. Về giới hạn, đây là bản cho máy tính Windows, dùng bàn phím và chuột, chưa hỗ trợ kính VR. Tóm lại, người xem đi trọn một vòng: đi, đọc, xem gần, rồi trở lại sảnh.

### Slide 4. Bộ sưu tập (45 giây)

Sáu hiện vật chia thành ba nhóm. Nhóm điêu khắc Chăm có tượng Shiva khổ hạnh, thế kỷ XIV, và tượng vua Chăm được thần hóa, cuối thế kỷ X. Nhóm gốm có ấm men ngọc có nắp, thế kỷ XIII–XIV, và bình hoa lam dáng quả lê, cuối thế kỷ XV. Nhóm di tích Huế có tượng quan hầu lăng Khải Định và nhà bia lăng Tự Đức. Nhà bia là cả một công trình, nên nhóm thu nhỏ để trưng bày. Các hiện vật lớn nhỏ rất khác nhau, nên khi xem gần, hệ thống tự chỉnh tỷ lệ cho từng món. Vậy các mô hình này từ đâu ra?

### Slide 5. Photogrammetry (50 giây)

Photogrammetry là cách chụp một vật từ rất nhiều góc. Phần mềm dùng các ảnh đó để dựng lại hình khối 3D, rồi phủ ảnh bề mặt thật lên mô hình. Nhóm không tự chụp hiện vật. Nhóm dùng 13 bản quét đã công bố với giấy phép mở. Riêng nhà bia Tự Đức dùng dữ liệu LiDAR kết hợp photogrammetry của tổ chức CyArk. Phần việc của nhóm là xử lý các mô hình cho nhẹ và hợp với Unity: giảm số tam giác, xử lý UV, bake normal và chỉnh vật liệu. Ví dụ, nhà bia gốc có gần 1,92 triệu tam giác, nhóm giảm còn 300 nghìn. Sau đó nhóm đặt mô hình lên bệ, gắn thuyết minh và vùng tương tác. Quy trình gồm bốn bước: ảnh, tái dựng 3D, xử lý mô hình, rồi đưa vào bảo tàng.

*Đây là slide dễ bị hỏi nhất. Nói chắc câu "nhóm không tự chụp hiện vật". Chuẩn bị câu Q1–Q5 trong file hỏi đáp.*

### Slide 6. Công nghệ (40 giây)

Nhóm dựng sản phẩm bằng Unity 6000.3.26f1 và lập trình bằng C#. URP lo phần hình ảnh, ánh sáng và hiệu ứng. Input System nhận các phím WASD, chuột, Shift, E, Esc và phím 1 để bật bút laze. Chữ tiếng Việt hiển thị bằng TextMesh Pro. Nhóm dùng Test Framework để kiểm thử tự động cả phần tính toán lẫn hành vi khi chạy.

**Câu chuyển lời (5 giây):** Đó là phần giới thiệu và nền tảng của sản phẩm. Tiếp theo, [tên người nói tiếp] sẽ trình bày các chức năng chính, bắt đầu từ cách di chuyển trong bảo tàng.

*Điền tên người nói slide 7 vào chỗ trong ngoặc vuông.*

---

## 3. Việc cần làm trước buổi

1. Đọc to cả 6 đoạn ít nhất hai lần và bấm giờ. Mục tiêu là dưới 5 phút.
2. Thuộc bảng số liệu ở mục 1. Nếu bị hỏi, phải nói số không cần nhìn slide.
3. Đọc trước câu Q1–Q13 trong [03_hoi_dap.md](03_hoi_dap.md).
4. Nếu được hỏi "làm sao biết đây là bản quét" (Q3), mở sẵn ảnh texture `NgheThaiHoa_BaseColor.jpg` và wireframe của một mô hình để chỉ.
