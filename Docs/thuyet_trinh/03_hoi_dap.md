# Câu hỏi có thể gặp và cách trả lời

**Người đọc:** cả ba thành viên nhóm.
**Mục đích:** trả lời ngắn, đúng sự thật các câu hỏi của thầy và các bạn sau phần thuyết trình.
**Nguồn:** mã nguồn `Assets/Museum/Scripts`, các file `SOURCE.txt` trong `Assets/Museum/Models`, kết quả kiểm thử ngày 17/10/2026.

**Nguyên tắc:** trả lời trong 2–4 câu. Nếu không biết, nói "nhóm chưa đo" hoặc "nhóm chưa kiểm tra điều đó", không đoán.

**Ai trả lời nhóm câu hỏi nào:**

| Nhóm câu hỏi | Liên quan slide | Người trả lời chính |
|---|---|---|
| A. Đề tài và photogrammetry (Q1–Q7) | 2–5 | Người nói slide 1–6 |
| B. Bản quyền và nguồn (Q8–Q12) | 3–5 | Người nói slide 1–6 |
| C. Kỹ thuật và lập trình (Q13–Q24) | 6–13 | Q13: người nói slide 1–6. Còn lại: người nói slide 7–11 |
| D. Kiểm thử và chất lượng (Q25–Q28) | 15–16 | Người nói slide 12–16 |
| E. Mở rộng và phân công (Q29–Q31) | 15 | Cả nhóm |

---

## A. Đề tài và photogrammetry

**Q1. Nhóm có tự chụp và tự dựng mô hình không?**
Không. Nhóm dùng 13 bản quét đã công bố với giấy phép mở. Phần việc của nhóm là xử lý mô hình (giảm lưới, UV, bake normal, vật liệu), dựng bảo tàng, viết thuyết minh và lập trình tương tác. Slide 5 có ghi rõ điều này.

**Q2. Vậy phần "photogrammetry" của đề tài nằm ở đâu?**
Dữ liệu đầu vào là sản phẩm của photogrammetry. Nhóm làm phần sau quét: đưa bản quét vào sử dụng. Cụ thể là làm cho mô hình nặng hàng triệu tam giác chạy được trong ứng dụng thời gian thực mà vẫn giữ chi tiết. Ví dụ, nhà bia giảm từ 1,92 triệu xuống 300 nghìn tam giác, và chi tiết bề mặt được giữ lại bằng normal map.

**Q3. Làm sao chứng minh đây là mô hình quét, không phải dựng tay?**
Có ba lớp bằng chứng:

1. **Nguồn:** nhà bia là dữ liệu CyArk năm 2019, có mã DOI `10.26301/n06n-qa49`. Ấm, bình và tượng vua Chăm do Bảo tàng Nghệ thuật Minneapolis công bố. Shiva có tag `photogrammetry` trên Sketchfab.
2. **Dấu vết trong mô hình:** texture là ảnh chụp thật, có rỉ, vết bẩn, nứt men. Lưới tam giác không đều và vỡ thành hàng nghìn mảnh. Vài mô hình còn dính cả nền bị quét kèm (Cao Đỉnh, Shiva MicMac).
3. **Kích thước khớp hồ sơ:** hũ men lam Cleveland đo được 22,2 cm, hồ sơ bảo tàng ghi 21,8 cm.

*Nếu có máy, mở wireframe của nghê Thái Hòa để thầy xem lưới.*

**Q4. Photogrammetry khác LiDAR thế nào?**
Photogrammetry dựng hình từ nhiều ảnh chụp, nên màu sắc rất thật nhưng hình học kém hơn ở bề mặt trơn hoặc bóng. LiDAR đo khoảng cách bằng tia laser, nên hình học chính xác nhưng không có màu. Nhà bia Tự Đức kết hợp cả hai: LiDAR cho hình khối, ảnh chụp cho màu.

**Q5. Vì sao phải giảm số tam giác? Giảm như vậy có làm mất chi tiết không?**
Mô hình gốc quá nặng cho ứng dụng thời gian thực: chỉ riêng nhà bia đã gần 2 triệu tam giác. Nhóm giảm lưới, rồi bake normal map từ mô hình đầy đủ. Normal map ghi lại các chi tiết nhỏ trên bề mặt, nên khi chiếu sáng, mô hình nhẹ vẫn hiện được các chi tiết đó. Tổng 6 hiện vật chính hiện là 757.661 tam giác.

**Q6. Kích thước hiện vật trong bảo tàng có đúng thật không?**
Không phải món nào cũng đúng. Ấm men ngọc (khoảng 21 cm) và bình hoa lam (khoảng 27 cm) đặt đúng kích thước thật theo hồ sơ bảo tàng. Nhà bia bị thu nhỏ còn khoảng 3,4 m để vừa sảnh. Các tượng có tỷ lệ gần đúng. Vì vậy không nên đo kích thước thật trong ứng dụng.

**Q7. Mô hình có giá trị bảo tồn thật không?**
Giá trị chính là phổ biến và giáo dục: người xem tiếp cận di sản qua bản sao số. Bản đã giảm lưới không dùng để đo đạc hay trùng tu. Muốn lưu trữ thì phải giữ bản quét gốc chất lượng cao, và đó là một hướng phát triển ở slide 15.

## B. Bản quyền và nguồn

**Q8. Dùng mô hình người khác có vi phạm bản quyền không?**
Không. Tất cả mô hình dùng giấy phép CC0 hoặc CC BY 4.0. CC0 cho dùng tự do. CC BY 4.0 cho dùng, kể cả sửa đổi, miễn là ghi công tác giả. Bảy bản quét trang trí có bảng ghi công trong bảo tàng. Mỗi mô hình có file `SOURCE.txt` ghi tác giả, đường link và giấy phép.

**Q9. Niên đại và câu chuyện lấy từ đâu?**
Từ mô tả của bảo tàng hoặc tác giả công bố bản quét, sau đó nhóm viết lại bằng tiếng Việt. Toàn bộ nằm trong file `Docs/narration_vi.json`. Mỗi hiện vật có trường "nguồn" hiện trên bảng thuyết minh.

**Q10. Tượng quan hầu Khải Định là bản quét thế nào?**
Tác giả eyesCloud3D ghi là quét photogrammetry bằng điện thoại. Kích thước trong bảo tàng là gần đúng, không phải kích thước thật.

**Q11. Bảy bản quét trang trí là gì?**
Trống đồng Tam Chúc, Cao Đỉnh (Thế Tổ Miếu, Huế), một cặp nghê điện Thái Hòa, hũ men lam của Bảo tàng Cleveland, tượng Shiva MicMac, đèn lồng Hội An, và hũ tiền đồng.

**Q12. Trống đồng Đông Sơn trong bảo tàng có phải bản quét không?**
Không. Tác giả ghi trên Sketchfab đây là tác phẩm cá nhân, tức mô hình dựng tay. Mô hình này chỉ để trang trí và không nằm trong 13 bản quét mà nhóm nêu. *Chỉ dùng câu này nếu chưa kịp gỡ trống hoặc thêm nhãn trước buổi.*

## C. Kỹ thuật và lập trình

**Q13. Vì sao chọn Unity, không chọn Unreal?**
Unity dùng C#, dễ học, có sẵn bộ kiểm thử, và URP chạy nhẹ trên máy tính phổ thông. Unreal cho hình ảnh đẹp hơn nhưng nặng hơn và dùng C++, không hợp với quy mô đồ án. *Đây là lý do của nhóm; nhóm chưa làm phép so sánh đo đạc.*

**Q14. Hệ thống biết người xem đang ở gần hiện vật nào bằng cách nào?**
Mỗi khung hình, hệ thống đo khoảng cách từ người xem đến từng hiện vật trên mặt sàn (bỏ qua chiều cao), rồi chọn hiện vật gần nhất còn nằm trong bán kính. Phần này do `NarrativeAnchorLogic` tính, và có kiểm thử riêng.

**Q15. Vì sao đo khoảng cách trên mặt sàn mà không đo trong không gian 3D?**
Vì chiều cao của camera và chiều cao của hiện vật khác nhau. Nếu tính cả chiều cao, một hiện vật cao như nhà bia sẽ có vùng đọc lệch so với các hiện vật nhỏ trên bệ. Bỏ chiều cao thì vùng đọc là một hình tròn trên sàn, dễ hiểu và đều cho mọi hiện vật.

**Q16. Khi hai vùng chồng nhau thì sao?**
Hệ thống chọn hiện vật gần nhất. Nếu người xem đang nhắm vào hiện vật khác, phím E sẽ mở hiện vật đang nhắm. Thoát ra thì bảng thuyết minh quay về hiện vật gần nhất.

**Q17. Raycast là gì?**
Là một tia vô hình bắn từ camera theo hướng nhìn. Tia chạm vào vật nào thì hệ thống biết người xem đang nhìn vật đó. Tầm của tia là 4 m, bật bút laze thì 20 m.

**Q18. Bút laze để làm gì?**
Để chỉ vào hiện vật ở xa, ví dụ khi thuyết minh cho nhóm người xem. Khi bật laze, tầm nhắm tăng lên 20 m, nên có thể mở xem gần một hiện vật từ xa.

**Q19. Làm sao hiện vật về đúng chỗ cũ sau khi xem gần?**
Trước khi đưa mô hình ra, hệ thống lưu vật chứa nó, vị trí, góc xoay và tỷ lệ. Khi thoát, hàm `Restore` gắn mô hình lại và trả đúng các giá trị đã lưu. Kiểu gán lại giá trị gốc này không cộng dồn sai số qua nhiều lần xem.

**Q20. Nếu người xem bấm thoát khi mô hình đang bay ra thì sao?**
Máy trạng thái xử lý trường hợp này. Hệ thống lấy vị trí hiện tại làm điểm xuất phát và đưa mô hình về ngay, không phải chờ mô hình bay ra xong.

**Q21. Vì sao khi xem gần phải khóa di chuyển?**
Vì lúc đó chuột dùng để xoay hiện vật. Nếu camera vẫn quay theo chuột, người xem không xoay hiện vật chính xác được. Trạng thái `Frozen` khóa việc đi và nhìn, và được mở lại khi thoát.

**Q22. Ánh sáng làm thế nào?**
Đèn trần là đèn vùng (area light) đã bake, tức ánh sáng được tính trước và lưu vào lightmap. Cách này đẹp và nhẹ. Mỗi hiện vật có một đèn spot chạy thời gian thực, để có thể làm sáng hơn khi người xem nhắm vào. Mô hình quét và đồ trang trí nhận sáng qua light probe (các điểm đo sáng đặt trong cảnh).

**Q23. Vì sao chữ tiếng Việt hiển thị đúng dấu?**
Nhóm tạo font asset của TextMesh Pro từ phông Be Vietnam Pro. Phông mặc định của TextMesh Pro không có dấu tiếng Việt.

**Q24. Nội dung thuyết minh có sửa được mà không cần sửa code không?**
Có. Mỗi hiện vật có một asset `ExhibitInfo` gồm 7 trường. Sửa asset là xong, không phải sửa code. Lưu ý: bản chạy đọc từ asset, nên nếu chỉ sửa file JSON thì bản chạy chưa đổi.

## D. Kiểm thử và chất lượng

**Q25. Kiểm thử những gì?**
Có 128 ca kiểm thử tự động, tất cả đều đạt:

- **EditMode, 66 ca:** kiểm tra phần tính toán thuần, như chọn hiện vật gần nhất, phép tính xem gần, bố cục phòng và việc nối các thành phần trong cảnh.
- **PlayMode, 62 ca:** chạy cảnh thật để kiểm tra hành vi như di chuyển, nhắm chọn, phím E, xem gần và trả về.

**Q26. Ứng dụng chạy bao nhiêu FPS? Cấu hình máy tối thiểu là gì?**
Nhóm chưa đo. Đây là hạn chế đầu tiên nêu ở slide 15. Nhóm chỉ xác nhận được bản v1.2 khởi động và chạy được trên máy của nhóm.

**Q27. Có lỗi nào nhóm đã gặp và sửa?**
Có. Sau khi dựng lại bảo tàng, một số phòng bị đen. Nguyên nhân là 53 trên 98 mô hình tạo bằng script bị lật thứ tự đỉnh tam giác, nên mặt bị quay ngược. Nhóm sửa thứ tự đỉnh, và từ đó kiểm tra hướng mặt mỗi khi tạo mô hình bằng script.

**Q28. Đã thử với người dùng thật chưa?**
Chưa có khảo sát người dùng chính thức. Đây là việc nên làm sau khi đo hiệu năng.

## E. Mở rộng và phân công

**Q29. Có chạy trên kính VR hoặc điện thoại được không?**
Hiện chưa. Hỗ trợ kính VR không chỉ là đổi camera. Cần thiết kế lại cách di chuyển (dịch chuyển tức thời thay vì đi bộ để tránh say), cách cầm hiện vật bằng tay cầm, và phải thử trên kính thật. Dự án đã có sẵn một mức đồ họa nhẹ cho thiết bị di động, nhưng chưa build và thử trên điện thoại.

**Q30. Mỗi thành viên làm phần nào?**
*Nhóm tự điền trước buổi. Mỗi người nói một câu cụ thể về phần mình làm, ví dụ "em xử lý mô hình", "em lập trình chế độ xem gần", "em viết thuyết minh và kiểm thử".*

**Q31. Nếu làm tiếp, nhóm làm gì đầu tiên?**
Đo hiệu năng trên một cấu hình cụ thể và thử thủ công các tình huống. Sau đó thêm thuyết minh bằng giọng nói. Hai việc này làm sản phẩm chắc chắn và dễ dùng hơn trước khi mở rộng sang VR.

---

## Điểm yếu cần biết trước

| Điểm yếu | Cách trả lời |
|---|---|
| Nhóm không tự chụp mô hình | Nói thẳng ngay ở slide 5. Nhấn mạnh phần xử lý và tích hợp mà nhóm làm (Q1, Q2). |
| Chưa đo FPS | Thừa nhận; nêu đây là việc đầu tiên (Q26). |
| Trống Đông Sơn không phải bản quét | Gỡ hoặc dán nhãn trước buổi. Nếu bị hỏi, dùng Q12. |
| Tượng vua Chăm chỉ có ghi chú "Scanned model" từ bảo tàng, không có tag photogrammetry | Dựa vào dấu vết trong mô hình: texture ảnh chụp, lưới không đều (Q3). |
| Kích thước trưng bày không phải kích thước thật | Nói rõ món nào đúng, món nào thu nhỏ (Q6). |
