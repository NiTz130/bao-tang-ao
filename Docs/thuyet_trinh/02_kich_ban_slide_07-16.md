# Kịch bản thuyết trình: slide 7–16 và demo

**Người đọc:** các thành viên nói phần chức năng, kết quả, demo và kết luận (slide 7–16).
**Mục đích:** trình bày đúng các chức năng, chạy demo không lỗi và kết thúc bài.
**Thời lượng:** khoảng 7 phút 30 giây, cộng 3 phút demo.
**Nguồn:** bản Canva `ThuyetTrinh_BaoTangAo (2).pptx` (16 slide, đọc ngày 17/10/2026) và mã nguồn trong `Assets/Museum/Scripts`.

Các file liên quan:

- Phần slide 1–6: [01_kich_ban_slide_01-06.md](01_kich_ban_slide_01-06.md).
- Câu hỏi và trả lời: [03_hoi_dap.md](03_hoi_dap.md). Câu hỏi về code: [04_hoi_dap_code.md](04_hoi_dap_code.md). Phần này dễ bị hỏi ở nhóm C, D và E.

**Phân vai (nhóm tự điền):**

| Slide | Người nói |
|---|---|
| 7–11: các chức năng | ................ |
| 12–16: kết quả, demo, hạn chế, kết luận | ................ |

---

## 1. Số liệu cần thuộc cho slide 7–16

| Nội dung | Số liệu |
|---|---|
| Sảnh | Sảnh tròn bán kính 8 m, nhà bia ở tâm |
| Vùng đọc | Bán kính 3 m (5 hiện vật), 3,5 m (nhà bia). Đo trên mặt sàn (mặt phẳng XZ) |
| Tầm nhắm | 4 m. Bật bút laze (phím 1): 20 m |
| Phản hồi khi nhắm | Đèn spot sáng thêm 1,5 lần, tâm ngắm phóng to, màu hổ phách |
| Góc nhìn | Giới hạn −85° đến 85° |
| Xem gần | Zoom 0,6–1,6 lần. Trạng thái: Idle → Entering → Viewing → Leaving → Idle |
| Kiểm thử | 128/128 đạt: EditMode 66/66, PlayMode 62/62 (17/10/2026) |
| Bản phát hành | v1.2 cho Windows, khởi động được |
| Chưa làm | Chưa đo FPS, chưa thử thủ công đầy đủ, chưa hỗ trợ kính VR |

---

## 2. Lời nói cho từng slide

Mỗi đoạn là lời đọc to. Dòng in nghiêng là ghi chú cho người nói.

### Slide 7. Di chuyển (35 giây)

Người xem tham quan theo góc nhìn thứ nhất: WASD để đi, chuột để nhìn quanh, giữ Shift để đi nhanh. Góc nhìn lên xuống được giới hạn từ âm 85 đến 85 độ, nên camera không bị lật ngược. Khi người xem cầm hiện vật lên xem gần, hệ thống bật trạng thái Frozen để khóa việc đi và nhìn. Lúc đó chuột chỉ dùng để xoay hiện vật. Khi thoát, người xem lại điều khiển như cũ. Vậy khi đến một hiện vật, người xem biết thông tin bằng cách nào?

### Slide 8. Thuyết minh theo vị trí (45 giây)

Bảng thuyết minh hiện ra theo chỗ người xem đứng, không cần nhắm vào hiện vật. Hệ thống liên tục đo khoảng cách trên mặt sàn và chọn hiện vật gần nhất trong vùng. Mỗi vùng có bán kính 3 mét, riêng nhà bia ở giữa sảnh là 3,5 mét. Lúc đầu, bảng chỉ hiện phần tóm tắt. Muốn đọc câu chuyện đầy đủ, người xem quay mặt khỏi hiện vật rồi nhấn E. Nhấn E lần nữa thì bảng thu gọn. Sang hiện vật khác, bảng tự đổi nội dung. Ra khỏi mọi vùng, bảng tự ẩn. Vì sao phải quay mặt đi trước khi nhấn E? Câu trả lời ở slide sau.

### Slide 9. Nhắm chọn và phím E (50 giây)

Ngoài chỗ đứng, hệ thống còn theo dõi hướng nhìn. Từ camera có một tia nhìn thẳng ra trước, dài 4 mét. Bật bút laze bằng phím 1 thì tia dài 20 mét. Khi tia trúng hiện vật, đèn chiếu vào hiện vật sáng hơn và tâm ngắm chuyển màu hổ phách. Phím E xử lý theo thứ tự ưu tiên. Đang xem gần thì E để thoát. Đang nhắm trúng hiện vật thì E mở xem gần. Các trường hợp còn lại thì E mở hoặc thu gọn bảng thuyết minh. Ví dụ, người xem đứng cạnh bình gốm nhưng nhìn vào bức tượng: nhấn E sẽ mở bức tượng. Thoát ra thì bảng lại hiện thông tin bình gốm. Nói ngắn gọn: chỗ đứng quyết định nội dung đọc, hướng nhìn quyết định hiện vật được xem gần.

### Slide 10. Xem gần, xoay và thu phóng (50 giây)

Khi mở xem gần, việc đầu tiên là ghi nhớ hiện vật đang ở đâu: vị trí, góc xoay, tỷ lệ và vật chứa nó. Nhờ vậy, lúc thoát hệ thống trả hiện vật về chính xác. Sau đó, hệ thống tạm tắt va chạm, tách mô hình khỏi bệ và đưa ra trước camera. Hệ thống tính tâm và kích thước của mô hình để mô hình vừa khung, dù là cái ấm 21 cm hay nhà bia lớn. Mô hình nằm bên phải, câu chuyện bên trái. Người xem giữ chuột trái và kéo để xoay, cuộn chuột để phóng to hoặc thu nhỏ từ 0,6 đến 1,6 lần. Nhấn E hoặc Esc thì mô hình về bệ, va chạm và quyền điều khiển được trả lại.

### Slide 11. Quản lý trạng thái (50 giây)

Để xem gần không bị lỗi, nhóm chia quá trình thành bốn trạng thái. Idle là lúc hiện vật nằm trên bệ. Entering là lúc mô hình đang bay ra trước camera. Viewing là lúc người xem xoay và phóng to. Leaving là lúc mô hình bay về bệ. Nếu chỉ dùng một biến "đang xem", rất khó xử lý khi người xem bấm thoát lúc mô hình còn đang bay ra. Với máy trạng thái, hệ thống lấy vị trí hiện tại làm điểm bắt đầu và đưa mô hình về luôn. Cuối lượt, hàm Restore gắn mô hình lại vào bệ, trả vị trí đã lưu, bật lại va chạm, ánh sáng và điều khiển. Nếu thành phần bị tắt giữa chừng, OnDisable cũng gọi Restore, nên hiện vật không bị "kẹt" giữa không trung.

### Slide 12. Kết quả: sảnh bảo tàng (30 giây)

Đây là ảnh chụp từ bản chạy thật. Sảnh tròn có bán kính 8 mét, trên trần có giếng trời. Nhà bia Tự Đức đặt ở tâm sảnh. Ba cửa vòm dẫn sang ba phòng: Chăm ở phía tây, gốm ở phía bắc và lăng tẩm Nguyễn ở phía đông. Đèn trần là đèn đã tính sẵn ánh sáng (bake), còn mỗi hiện vật có một đèn spot riêng chạy thời gian thực.

### Slide 13. Kết quả: xem chi tiết (30 giây)

Đây là màn hình khi xem gần nhà bia Tự Đức. Bên trái là tên, niên đại, chất liệu, câu chuyện và nguồn. Bên phải là mô hình đưa ra gần. Dòng dưới hướng dẫn kéo chuột, cuộn chuột và E hoặc Esc để quay lại. Điểm chính là người xem vừa đọc vừa xem, không phải rời khỏi bảo tàng để tìm thông tin. Bây giờ nhóm xin trình diễn trực tiếp.

### Slide 14. Demo (3 phút)

*Chuyển sang cửa sổ VirtualMuseum. Làm theo mục 3. Vừa làm vừa nói một câu ngắn cho mỗi bước.*

### Slide 15. Hạn chế và hướng phát triển (45 giây)

Kiểm thử tự động đã chạy đạt cả 128 ca. Hạn chế lớn nhất còn lại là nhóm chưa đo hiệu năng trên một cấu hình máy cụ thể, và chưa thử thủ công hết các tình huống. Đó là việc làm đầu tiên. Tiếp theo là kiểm tra giao diện khi cửa sổ nhỏ hoặc nội dung dài. Khi sản phẩm đã ổn định, nhóm muốn thêm thuyết minh bằng giọng nói, nhiều ngôn ngữ, chọn cỡ chữ và tuyến tham quan có hướng dẫn. Hỗ trợ kính VR là một nhánh riêng, vì cần thiết kế lại thao tác và thử trên kính thật, không chỉ thay camera. Về dữ liệu, cần giữ riêng bản quét chất lượng cao để lưu trữ, tách khỏi bản đã giảm lưới dùng trong ứng dụng.

### Slide 16. Kết luận (40 giây)

Tóm lại, nhóm đã xây dựng một bảo tàng ảo gồm tiền sảnh, sảnh tròn và ba phòng chủ đề. Sáu hiện vật chính có thuyết minh tiếng Việt, bảy bản quét khác dùng để trang trí và ghi công đúng giấy phép. Người xem có thể đi lại, đọc, nhắm chọn, dùng bút laze, xem gần và xoay hiện vật. Khi thoát, hiện vật về đúng chỗ cũ. 128 trên 128 ca kiểm thử tự động đạt, và bản v1.2 cho Windows chạy được. Giá trị của đề tài là kết hợp mô hình di sản với thông tin và cho người xem tự khám phá. Nhóm xin cảm ơn thầy và các bạn đã lắng nghe, và xin nhận câu hỏi.

---

## 3. Kịch bản demo (khoảng 3 phút)

Dùng một hiện vật xuyên suốt: **ấm men ngọc** ở phòng Bắc (Gốm).

| Bước | Thao tác | Câu nói |
|---|---|---|
| 1 | Từ tiền sảnh, dùng WASD và chuột đi vào sảnh tròn, rẽ sang phòng Bắc, đến gần ấm | "Khi em lại gần ấm men ngọc, bảng tóm tắt tự hiện ra." |
| 2 | Quay mặt khỏi ấm, nhấn E. Đọc lướt. Nhấn E lần nữa | "Quay mặt đi rồi nhấn E thì mở câu chuyện. Nhấn lại thì thu gọn." |
| 3 | Nhìn vào ấm: tâm ngắm đổi màu hổ phách, đèn sáng hơn | "Khi nhắm trúng, đèn sáng hơn và tâm ngắm đổi màu." |
| 4 | Nhấn E. Giữ chuột trái kéo để xoay, cuộn để phóng to | "Nhấn E để cầm ấm lên. Em xoay để thầy thấy vòi và nắp." |
| 5 | Nhấn E hoặc Esc. Đi vài bước | "Thoát ra thì ấm về đúng bệ, và em đi tiếp được." |
| 6 (nếu còn giờ) | Nhấn phím 1, chỉ tia laze vào nhà bia ở xa | "Phím 1 bật bút laze, tầm nhắm tăng lên 20 mét." |

**Khi demo gặp sự cố:**

- **Chuột không xoay được camera:** bấm chuột trái vào cửa sổ game một lần để khóa chuột lại. Esc mở khóa chuột khi không xem gần.
- **Nhấn E mà mở xem gần thay vì mở câu chuyện:** đó là đúng thiết kế, vì đang nhắm trúng hiện vật. Nói: "Đây chính là thứ tự ưu tiên ở slide 9." Thoát ra, quay mặt đi rồi nhấn lại.
- **Ứng dụng bị treo hoặc không mở:** chuyển ngay sang slide 12 và 13 (ảnh chụp từ bản chạy). Nói rõ là đang dùng ảnh thay cho demo.

---

## 4. Việc cần làm trước buổi

1. **Kiểm tra trống đồng Đông Sơn.** Mô hình `DongSonDrum` có trong cảnh, nhưng tác giả ghi đây là tác phẩm tự dựng, không phải bản quét. Trong cảnh chưa có nhãn ghi điều này. Chọn một cách: thêm nhãn "mô hình của nghệ sĩ", hoặc gỡ trống khỏi cảnh. Nếu không kịp, xem câu Q12 trong file hỏi đáp.
2. **Mở sẵn bản chạy.** Chạy `Builds/Windows/VirtualMuseum.exe` trên máy trình chiếu trước 10 phút. Đi thử một vòng theo kịch bản demo ở mục 3.
3. **Điền phân vai** ở đầu file này và tên người nói slide 7 vào câu chuyển lời của slide 6.
