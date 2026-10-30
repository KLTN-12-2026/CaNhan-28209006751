USE [Atomos];
GO

SET IDENTITY_INSERT dbo.BaiHoc ON;

MERGE dbo.BaiHoc AS target
USING (VALUES
    (1, 1, N'Bài 1. Sử dụng một số hóa chất, thiết bị cơ bản trong phòng thí nghiệm', N'# MỤC TIÊU
* Nhận biết một số dụng cụ, hóa chất và nêu được quy tắc sử dụng hóa chất an toàn.
* Nhận biết các thiết bị điện trong môn Khoa học tự nhiên 8 và trình bày cách sử dụng điện an toàn.

# I - Nhận biết hóa chất và quy tắc sử dụng hóa chất an toàn trong phòng thí nghiệm

## 1. Nhận biết hóa chất
Hóa chất trong phòng thí nghiệm được đựng trong chai hoặc lọ kín, thường làm bằng thủy tinh hoặc nhựa. Nhãn phải ghi tên, công thức hóa học, khối lượng hoặc thể tích, độ tinh khiết, nhà sản xuất, kí hiệu cảnh báo và điều kiện bảo quản. Dung dịch pha sẵn cần ghi rõ nồng độ chất tan.

## 2. Quy tắc sử dụng hóa chất an toàn
* Không sử dụng hóa chất trong vật chứa không có nhãn hoặc nhãn bị mờ. Đọc kĩ nhãn, tính chất và cảnh báo trước khi làm thí nghiệm.
* Không dùng tay lấy trực tiếp hóa chất. Dùng thìa cho chất rắn dạng bột hoặc hạt nhỏ; dùng panh cho hạt lớn, dây hoặc thanh; dùng phễu, cốc có mỏ, ống đong hoặc ống nhỏ giọt cho chất lỏng.
* Khi rót hóa chất lỏng, hướng nhãn chai lên trên để hóa chất không làm hỏng nhãn.
* Khi hóa chất dính vào người hoặc bị đổ, báo ngay cho giáo viên để được hướng dẫn xử lí.
* Không đổ hóa chất thừa trở lại bình chứa; xử lí theo hướng dẫn của giáo viên.

# II - Một số dụng cụ thí nghiệm và cách sử dụng

## 1. Dụng cụ thông dụng
Ống nghiệm, cốc thủy tinh, bình nón, phễu lọc, ống đong, ống nhỏ giọt và kẹp gỗ.

## 2. Sử dụng ống nghiệm
Giữ ống nghiệm bằng tay không thuận, dùng tay thuận để thêm hóa chất. Khi đun, kẹp ống nghiệm tại vị trí cách miệng khoảng một phần ba chiều dài ống; hướng miệng ống về phía không có người. Làm nóng đều đáy ống trước khi đun trực tiếp phần chứa hóa chất.', 1),
    (2, 1, N'Bài 2. Phản ứng hóa học', N'# MỤC TIÊU
* Phân biệt được biến đổi vật lí và biến đổi hóa học.
* Nêu được khái niệm phản ứng hóa học, chất tham gia và sản phẩm.
* Nhận biết được một số dấu hiệu chứng tỏ phản ứng hóa học xảy ra.
* Nêu được phản ứng tỏa nhiệt, phản ứng thu nhiệt và một số ứng dụng trong đời sống.

# I - Biến đổi vật lí và biến đổi hóa học
## 1. Biến đổi vật lí
Biến đổi vật lí là sự biến đổi về trạng thái, hình dạng hoặc kích thước nhưng không tạo ra chất mới. Nước đá tan thành nước, nước bay hơi và đường tan trong nước là các biến đổi vật lí.

## 2. Biến đổi hóa học
Biến đổi hóa học là sự biến đổi có tạo thành chất mới. Đốt cháy carbon tạo carbon dioxide, sắt để lâu trong không khí ẩm tạo gỉ sắt và đun đường tạo chất màu nâu là các biến đổi hóa học.

# II - Phản ứng hóa học
Phản ứng hóa học là quá trình biến đổi từ chất này thành chất khác. Chất ban đầu bị biến đổi gọi là chất tham gia; chất mới tạo thành gọi là sản phẩm.

Trong phản ứng, các nguyên tử không sinh ra và không mất đi. Liên kết giữa các nguyên tử thay đổi, làm các phân tử ban đầu biến đổi thành phân tử mới.

Ví dụ: magnesium cháy trong oxygen tạo magnesium oxide.

Magnesium + Oxygen -> Magnesium oxide

# III - Dấu hiệu có phản ứng hóa học
Có thể nhận biết phản ứng qua một hoặc nhiều dấu hiệu:
* Có chất khí thoát ra, thường quan sát được dưới dạng bọt khí.
* Xuất hiện chất rắn không tan gọi là kết tủa.
* Màu sắc hoặc mùi của hệ thay đổi.
* Nhiệt độ thay đổi dù không được đun nóng hoặc làm lạnh từ bên ngoài.
* Có ánh sáng hoặc âm thanh phát ra.

Không nên chỉ dựa vào một dấu hiệu riêng lẻ. Chất tan, nóng chảy hoặc bay hơi chưa chắc là phản ứng hóa học vì có thể chỉ là biến đổi vật lí.

# IV - Năng lượng trong phản ứng hóa học
## 1. Phản ứng tỏa nhiệt
Phản ứng tỏa nhiệt giải phóng năng lượng ra môi trường, thường làm nhiệt độ môi trường tăng. Phản ứng cháy nhiên liệu và phản ứng trung hòa giữa acid với base là các ví dụ thường gặp.

## 2. Phản ứng thu nhiệt
Phản ứng thu nhiệt nhận năng lượng từ môi trường. Một số phản ứng phân hủy chỉ xảy ra khi được cung cấp nhiệt liên tục, ví dụ nhiệt phân calcium carbonate.

Năng lượng của phản ứng được ứng dụng trong đun nấu, sản xuất, túi chườm nóng và túi làm lạnh.', 2),
    (3, 1, N'Bài 3. Mol và tỉ khối chất khí', N'# MỤC TIÊU
* Nêu được khái niệm mol và hằng số Avogadro.
* Tính được số mol, khối lượng và số hạt của chất.
* Tính được thể tích mol của chất khí ở 1 bar và 25 °C.
* So sánh được khối lượng của hai chất khí bằng tỉ khối.

# I - Mol
Mol là lượng chất chứa 6,022 x 10^23 hạt vi mô của chất đó. Các hạt có thể là nguyên tử, phân tử hoặc ion. Số 6,022 x 10^23 mol^-1 được gọi là hằng số Avogadro, kí hiệu N_A.

Số hạt được tính theo công thức:

N = n x N_A

Trong đó N là số hạt, n là số mol và N_A = 6,022 x 10^23 mol^-1.

# II - Khối lượng mol
Khối lượng mol, kí hiệu M, là khối lượng của một mol chất, đơn vị g/mol. Giá trị khối lượng mol nguyên tử hoặc phân tử bằng trị số nguyên tử khối hoặc phân tử khối của chất đó.

Các công thức chuyển đổi:
* n = m / M
* m = n x M

Ví dụ: M(H2O) = 2 x 1 + 16 = 18 g/mol. Có 18 g nước tương ứng với 1 mol H2O.

# III - Thể tích mol của chất khí
Ở cùng nhiệt độ và áp suất, một mol các chất khí khác nhau chiếm cùng một thể tích. Ở 1 bar và 25 °C, thể tích mol gần bằng 24,79 L/mol.

* V = n x 24,79
* n = V / 24,79

Trong đó V tính bằng lít. Khi điều kiện nhiệt độ hoặc áp suất thay đổi, thể tích mol khí cũng thay đổi nên phải đọc đúng điều kiện đề bài.

# IV - Tỉ khối chất khí
Tỉ khối của khí A đối với khí B cho biết khí A nặng hay nhẹ hơn khí B bao nhiêu lần.

d(A/B) = M_A / M_B

Tỉ khối của khí A đối với không khí được tính gần đúng:

d(A/không khí) = M_A / 29

Nếu tỉ khối lớn hơn 1, khí A nặng hơn khí được so sánh; nếu nhỏ hơn 1, khí A nhẹ hơn.', 3),
    (4, 1, N'Bài 4. Dung dịch và nồng độ', N'# MỤC TIÊU
* Nêu được dung dịch là hỗn hợp đồng nhất của các chất đã tan trong nhau.
* Nêu được định nghĩa độ tan, nồng độ phần trăm và nồng độ mol.
* Tính được độ tan và nồng độ dung dịch theo công thức.
* Pha được dung dịch theo nồng độ cho trước.

# I - Dung dịch, chất tan và dung môi
Dung dịch là hỗn hợp đồng nhất của chất tan và dung môi. Chất tan có thể ở thể rắn, lỏng hoặc khí. Ở nhiệt độ và áp suất xác định, dung dịch còn hòa tan thêm được chất tan là dung dịch chưa bão hòa; dung dịch không thể hòa tan thêm là dung dịch bão hòa.

# II - Độ tan
Độ tan của một chất trong nước là số gam chất đó hòa tan trong 100 gam nước để tạo thành dung dịch bão hòa ở nhiệt độ và áp suất xác định.

S = (m chất tan / m nước) x 100

Độ tan của phần lớn chất rắn tăng khi nhiệt độ tăng. Độ tan của phần lớn chất khí giảm khi nhiệt độ tăng hoặc áp suất giảm.

# III - Nồng độ dung dịch
## 1. Nồng độ phần trăm
Nồng độ phần trăm cho biết số gam chất tan có trong 100 gam dung dịch.

C% = (m chất tan / m dung dịch) x 100%

Khối lượng dung dịch bằng tổng khối lượng chất tan và dung môi.

## 2. Nồng độ mol
Nồng độ mol cho biết số mol chất tan có trong một lít dung dịch.

C_M = n / V

Trong đó n tính bằng mol, V tính bằng lít và C_M có đơn vị mol/L.

# IV - Pha chế dung dịch
Tính lượng chất tan và dung môi cần dùng theo nồng độ yêu cầu, sau đó cân hoặc đo đúng lượng, hòa tan và bổ sung dung môi đến thể tích cần thiết.', 4),
    (5, 1, N'Bài 5. Định luật bảo toàn khối lượng và phương trình hóa học', N'# MỤC TIÊU
* Phát biểu và vận dụng được định luật bảo toàn khối lượng.
* Nêu được khái niệm và các bước lập phương trình hóa học.
* Trình bày được ý nghĩa của phương trình hóa học.

# I - Định luật bảo toàn khối lượng
Trong một phản ứng hóa học, tổng khối lượng các chất sản phẩm bằng tổng khối lượng các chất tham gia phản ứng. Trong phản ứng, liên kết giữa các nguyên tử thay đổi nhưng số nguyên tử của mỗi nguyên tố được giữ nguyên.

Nếu phản ứng có n chất và biết khối lượng của n - 1 chất thì có thể tính khối lượng chất còn lại.

Ví dụ: barium chloride + sodium sulfate tạo barium sulfate + sodium chloride.

m barium chloride + m sodium sulfate = m barium sulfate + m sodium chloride

# II - Phương trình hóa học
## 1. Các bước lập phương trình
* Bước 1: Viết sơ đồ phản ứng bằng công thức hóa học của chất tham gia và sản phẩm.
* Bước 2: Đặt hệ số thích hợp để số nguyên tử mỗi nguyên tố ở hai vế bằng nhau.
* Bước 3: Viết phương trình hóa học hoàn chỉnh.

Không được thay đổi chỉ số trong công thức hóa học để cân bằng phương trình.

## 2. Ý nghĩa
Phương trình hóa học cho biết chất tham gia, sản phẩm và tỉ lệ số mol giữa các chất. Tỉ lệ số mol đúng bằng tỉ lệ hệ số trong phương trình đã cân bằng.

Ví dụ: 4Al + 3O2 -> 2Al2O3 cho tỉ lệ Al : O2 : Al2O3 bằng 4 : 3 : 2.', 5),
    (6, 1, N'Bài 6. Tính theo phương trình hóa học', N'# MỤC TIÊU
* Tính lượng chất theo số mol, khối lượng hoặc thể tích khí ở 1 bar và 25 độ C.
* Nêu được khái niệm hiệu suất phản ứng.
* Tính được hiệu suất từ lượng sản phẩm lí thuyết và thực tế.

# I - Tính lượng chất theo phương trình hóa học
* Bước 1: Viết và cân bằng phương trình hóa học.
* Bước 2: Đổi dữ kiện khối lượng hoặc thể tích về số mol.
* Bước 3: Dựa vào tỉ lệ hệ số để tính số mol chất cần tìm.
* Bước 4: Đổi số mol vừa tìm thành khối lượng hoặc thể tích theo yêu cầu.

Các công thức thường dùng:
* n = m / M
* Ở 1 bar và 25 độ C: V = n x 24,79 L

# II - Hiệu suất phản ứng
Trong thực tế, phản ứng có thể không hoàn toàn hoặc xảy ra hao hụt nên lượng sản phẩm thực tế thường nhỏ hơn lượng sản phẩm lí thuyết.

H = (lượng sản phẩm thực tế / lượng sản phẩm lí thuyết) x 100%

Khi hiệu suất nhỏ hơn 100%, lượng chất tham gia thực tế cần dùng lớn hơn lượng lí thuyết hoặc lượng sản phẩm thực tế thu được nhỏ hơn lượng lí thuyết.', 6),
    (7, 1, N'Bài 7. Tốc độ phản ứng và chất xúc tác', N'# MỤC TIÊU
* Nêu được khái niệm tốc độ phản ứng.
* Trình bày được các yếu tố ảnh hưởng đến tốc độ phản ứng.
* Nêu được khái niệm và vai trò của chất xúc tác.

# I - Tốc độ phản ứng
Tốc độ phản ứng đặc trưng cho mức độ nhanh hoặc chậm của phản ứng hóa học. Có thể theo dõi tốc độ qua sự thay đổi thể tích khí, khối lượng chất rắn hoặc nồng độ chất tan trong một khoảng thời gian.

# II - Các yếu tố ảnh hưởng
## 1. Nồng độ
Tăng nồng độ chất tham gia thường làm tăng tốc độ phản ứng vì số va chạm hiệu quả giữa các tiểu phân tăng.

## 2. Nhiệt độ
Tăng nhiệt độ thường làm phản ứng nhanh hơn. Hạ nhiệt độ giúp làm chậm các phản ứng gây hỏng thực phẩm.

## 3. Diện tích bề mặt tiếp xúc
Nghiền hoặc chia nhỏ chất rắn làm tăng diện tích tiếp xúc và thường làm phản ứng nhanh hơn. Bột đá vôi phản ứng với acid nhanh hơn đá vôi dạng viên cùng khối lượng.

## 4. Chất xúc tác
Chất xúc tác làm tăng tốc độ phản ứng nhưng không bị tiêu hao và không thay đổi tính chất hóa học sau phản ứng. Ví dụ MnO2 xúc tác sự phân hủy H2O2; enzyme amylase hỗ trợ chuyển hóa tinh bột.', 7),
    (8, 2, N'Bài 8. Acid', N'# MỤC TIÊU
* Nêu được khái niệm acid và cách tạo ion H+ trong nước.
* Trình bày được tính chất hóa học chung của dung dịch acid.
* Nêu được tính chất và ứng dụng của một số acid thông dụng.

# I - Khái niệm acid
Acid là hợp chất mà phân tử có nguyên tử hydrogen liên kết với gốc acid. Khi tan trong nước, acid phân li tạo ion H+.

Ví dụ:
* HCl -> H+ + Cl-
* H2SO4 -> 2H+ + SO4^2-

Tên acid thường gồm từ acid và tên gốc acid tương ứng. Công thức acid có thể nhận biết qua một hoặc nhiều nguyên tử H có khả năng phân li thành H+.

# II - Tính chất hóa học của acid
## 1. Làm đổi màu chất chỉ thị
Dung dịch acid làm giấy quỳ tím chuyển đỏ. Giá trị pH của dung dịch acid nhỏ hơn 7.

## 2. Tác dụng với kim loại
Một số kim loại đứng trước hydrogen trong dãy hoạt động hóa học tác dụng với acid loãng tạo muối và khí hydrogen.

Mg + H2SO4 -> MgSO4 + H2

Cu, Ag và Au không đẩy được hydrogen khỏi các acid loãng thông thường. Acid nitric và sulfuric acid đặc có tính oxi hóa mạnh nên không áp dụng trực tiếp quy tắc trên.

## 3. Tác dụng với base
Acid tác dụng với base tạo muối và nước. Đây là phản ứng trung hòa.

2HCl + Ca(OH)2 -> CaCl2 + 2H2O

## 4. Tác dụng với oxide base
Acid tác dụng với oxide base tạo muối và nước.

CuO + 2HCl -> CuCl2 + H2O

## 5. Tác dụng với muối
Phản ứng xảy ra khi tạo kết tủa, chất khí hoặc chất điện li yếu như nước.

CaCO3 + 2HCl -> CaCl2 + CO2 + H2O

# III - Một số acid thông dụng
## 1. Hydrochloric acid
HCl có trong dịch vị dạ dày với nồng độ thấp, được dùng để tẩy gỉ kim loại và điều chỉnh pH. Dung dịch đậm đặc có tính ăn mòn, cần dùng đồ bảo hộ.

## 2. Sulfuric acid
H2SO4 được dùng nhiều trong sản xuất phân bón, chất tẩy rửa, sơn và ắc quy. Khi pha loãng phải rót từ từ acid vào nước, không làm ngược lại vì quá trình tỏa nhiệt mạnh.

## 3. Acetic acid
CH3COOH là acid yếu có trong giấm ăn. Acetic acid được dùng trong chế biến thực phẩm và sản xuất một số vật liệu.', 1),
    (9, 2, N'Bài 9. Base - thang pH', N'# MỤC TIÊU
* Nêu được khái niệm base và kiềm.
* Trình bày được tính chất hóa học chung của base.
* Sử dụng được thang pH để mô tả môi trường acid, trung tính và base.

# I - Khái niệm và phân loại base
Base là hợp chất có nguyên tử kim loại liên kết với một hoặc nhiều nhóm hydroxide OH. Base tan trong nước tạo ion OH- được gọi là kiềm.

Ví dụ base tan: NaOH, KOH, Ba(OH)2. Ví dụ base không tan: Cu(OH)2, Fe(OH)3, Mg(OH)2.

Tên base gồm tên kim loại, hóa trị nếu kim loại có nhiều hóa trị, và từ hydroxide. Fe(OH)2 là iron(II) hydroxide; Fe(OH)3 là iron(III) hydroxide.

# II - Tính chất hóa học của base
## 1. Tác dụng với chất chỉ thị
Dung dịch kiềm làm quỳ tím chuyển xanh và làm phenolphthalein không màu chuyển hồng.

## 2. Tác dụng với acid
Base tác dụng với acid tạo muối và nước.

NaOH + HCl -> NaCl + H2O

## 3. Tác dụng với oxide acid
Dung dịch kiềm có thể tác dụng với oxide acid tạo muối và nước.

2NaOH + CO2 -> Na2CO3 + H2O

Khi tỉ lệ chất thay đổi, acid nhiều nấc hoặc oxide acid có thể tạo muối acid, ví dụ NaOH + CO2 -> NaHCO3.

## 4. Base không tan bị nhiệt phân
Một số base không tan phân hủy khi đun nóng tạo oxide kim loại và nước.

Cu(OH)2 -> CuO + H2O

# III - Thang pH
Thang pH thường có giá trị từ 0 đến 14:
* pH < 7: môi trường acid.
* pH = 7: môi trường trung tính.
* pH > 7: môi trường base.

pH càng nhỏ thì tính acid thường càng mạnh; pH càng lớn thì tính base thường càng mạnh. Có thể dùng giấy chỉ thị pH hoặc bút đo pH để xác định gần đúng hoặc chính xác giá trị pH.

# IV - Ứng dụng và an toàn
NaOH dùng trong sản xuất xà phòng, giấy và chất tẩy rửa. Ca(OH)2 dùng xử lí nước, khử chua đất và sản xuất vật liệu xây dựng. Nhiều base có tính ăn mòn nên phải tránh tiếp xúc trực tiếp với da và mắt.', 2),
    (10, 2, N'Bài 10. Oxide', N'# MỤC TIÊU
* Nêu được khái niệm oxide và gọi tên một số oxide thông dụng.
* Phân loại được oxide base, oxide acid, oxide lưỡng tính và oxide trung tính.
* Trình bày được tính chất hóa học tiêu biểu của từng loại oxide.

# I - Khái niệm và cách gọi tên
Oxide là hợp chất của hai nguyên tố, trong đó có một nguyên tố là oxygen. Công thức tổng quát có dạng MxOy.

Tên oxide thường gồm tên nguyên tố và từ oxide. Với nguyên tố có nhiều hóa trị, cần nêu hóa trị hoặc dùng tiền tố chỉ số nguyên tử phù hợp.

Ví dụ: Na2O là sodium oxide, Fe2O3 là iron(III) oxide, CO2 là carbon dioxide.

# II - Phân loại oxide
## 1. Oxide base
Oxide base thường là oxide của kim loại. Oxide base tác dụng với acid tạo muối và nước. Một số oxide base mạnh tác dụng với nước tạo base.

CaO + H2O -> Ca(OH)2

CuO + 2HCl -> CuCl2 + H2O

## 2. Oxide acid
Oxide acid thường là oxide của phi kim ở số oxi hóa cao. Nhiều oxide acid tác dụng với nước tạo acid và tác dụng với base tạo muối cùng nước.

SO3 + H2O -> H2SO4

CO2 + 2NaOH -> Na2CO3 + H2O

## 3. Oxide lưỡng tính
Oxide lưỡng tính tác dụng được với cả acid và base mạnh. Al2O3 và ZnO là các ví dụ thường gặp.

Al2O3 + 6HCl -> 2AlCl3 + 3H2O

Al2O3 + 2NaOH -> 2NaAlO2 + H2O

## 4. Oxide trung tính
Oxide trung tính hầu như không thể hiện tính acid hoặc base trong điều kiện thường. CO, NO và N2O là các ví dụ.

# III - Điều chế và ứng dụng
Oxide có thể tạo thành khi nguyên tố phản ứng với oxygen hoặc khi nhiệt phân một số hợp chất.

2Mg + O2 -> 2MgO

CaCO3 -> CaO + CO2

CaO dùng trong sản xuất xi măng và xử lí đất; SiO2 là thành phần chính của cát và thủy tinh; CO2 dùng trong chữa cháy và sản xuất nước giải khát có gas.', 3),
    (11, 2, N'Bài 11. Muối', N'# MỤC TIÊU
* Nêu được khái niệm, thành phần và cách gọi tên muối.
* Phân biệt được muối tan, ít tan và không tan.
* Trình bày được tính chất hóa học cơ bản của muối và điều kiện phản ứng trao đổi.

# I - Khái niệm và gọi tên muối
Muối là hợp chất được tạo thành khi ion H+ của acid được thay bằng ion kim loại hoặc ion ammonium NH4+. Phân tử muối gồm cation kim loại hoặc NH4+ và anion gốc acid.

Tên muối gồm tên kim loại, hóa trị nếu cần, và tên gốc acid. NaCl là sodium chloride, FeSO4 là iron(II) sulfate.

Muối trung hòa không còn hydrogen có khả năng phân li thành H+. Muối acid vẫn còn hydrogen có khả năng phân li, ví dụ NaHCO3 và NaHSO4.

# II - Tính tan của muối
Muối có thể tan, ít tan hoặc không tan trong nước. Có thể dùng bảng tính tan để dự đoán trạng thái sản phẩm.

Một số quy tắc thường dùng:
* Phần lớn muối nitrate tan.
* Phần lớn muối của Na+, K+ và NH4+ tan.
* Nhiều muối carbonate và phosphate không tan, trừ muối của Na+, K+ và NH4+.
* AgCl, BaSO4 và nhiều muối sulfide là chất không tan.

# III - Tính chất hóa học của muối
## 1. Tác dụng với kim loại
Kim loại hoạt động mạnh hơn có thể đẩy kim loại yếu hơn ra khỏi dung dịch muối.

Zn + FeSO4 -> ZnSO4 + Fe

## 2. Tác dụng với acid
Phản ứng xảy ra khi tạo kết tủa, khí hoặc acid yếu ít điện li.

CaCO3 + 2HCl -> CaCl2 + CO2 + H2O

## 3. Tác dụng với base
Muối tác dụng với base khi tạo sản phẩm kết tủa hoặc chất điện li yếu.

FeSO4 + 2NaOH -> Fe(OH)2 + Na2SO4

## 4. Tác dụng với muối
Hai dung dịch muối có thể phản ứng trao đổi nếu tạo chất không tan.

AgNO3 + NaCl -> AgCl + NaNO3

## 5. Nhiệt phân
Một số muối bị phân hủy khi đun nóng. Sản phẩm phụ thuộc loại muối và kim loại.

CaCO3 -> CaO + CO2

# IV - Điều kiện phản ứng trao đổi
Phản ứng trao đổi trong dung dịch chỉ xảy ra rõ rệt khi tạo ít nhất một chất kết tủa, chất khí hoặc chất điện li yếu như nước. Nếu tất cả chất vẫn tồn tại dưới dạng ion tan thì coi như không có phản ứng.', 4),
    (12, 2, N'Bài 12. Phân bón hóa học', N'# MỤC TIÊU
* Nêu được vai trò của các nguyên tố dinh dưỡng chính đối với cây trồng.
* Phân loại được phân đạm, phân lân, phân kali và phân hỗn hợp.
* Trình bày được nguyên tắc sử dụng phân bón hợp lí, an toàn và bảo vệ môi trường.

# I - Vai trò của phân bón
Phân bón cung cấp các nguyên tố dinh dưỡng cần thiết cho cây. Ba nguyên tố đa lượng quan trọng là nitrogen, phosphorus và potassium, thường kí hiệu N, P, K.

* Nitrogen giúp cây phát triển thân, cành và lá.
* Phosphorus hỗ trợ phát triển bộ rễ, ra hoa và tạo hạt.
* Potassium giúp cây cứng khỏe, tăng khả năng chống chịu và nâng cao chất lượng nông sản.

Cây còn cần các nguyên tố trung lượng và vi lượng như Ca, Mg, S, Fe, Zn và B với lượng phù hợp.

# II - Các loại phân bón hóa học
## 1. Phân đạm
Phân đạm cung cấp nitrogen. Các loại thường gặp gồm urea CO(NH2)2, ammonium nitrate NH4NO3 và ammonium sulfate (NH4)2SO4.

Phân đạm thường dễ tan, thích hợp bón thúc. Không nên bón phân ammonium cùng lúc với vôi vì có thể giải phóng ammonia làm mất đạm.

## 2. Phân lân
Phân lân cung cấp phosphorus. Superphosphate và phosphate tự nhiên là các nguồn phân lân phổ biến. Phân lân thường được bón lót vì phosphorus di chuyển chậm trong đất.

## 3. Phân kali
Phân kali cung cấp potassium. KCl và K2SO4 là hai loại thông dụng. Cần chọn loại phù hợp vì một số cây nhạy cảm với ion chloride.

## 4. Phân hỗn hợp và phân phức hợp
Phân NPK chứa đồng thời nitrogen, phosphorus và potassium theo tỉ lệ ghi trên bao bì. Tỉ lệ N-P-K phải được chọn theo loại cây và giai đoạn sinh trưởng.

# III - Sử dụng phân bón hợp lí
Tuân thủ nguyên tắc đúng loại, đúng liều, đúng lúc và đúng cách. Đọc nhãn, dùng đồ bảo hộ và bảo quản phân ở nơi khô ráo, xa thực phẩm và nguồn nước.

Bón quá ít làm cây thiếu dinh dưỡng; bón quá nhiều có thể làm cháy rễ, tồn dư nitrate, chua hóa đất và gây ô nhiễm nước. Phân bón bị rửa trôi xuống ao hồ có thể gây phú dưỡng, làm tảo phát triển mạnh và giảm oxygen hòa tan.

Kết hợp phân hóa học với phân hữu cơ, theo dõi tính chất đất và nhu cầu cây giúp tăng hiệu quả, giảm chi phí và hạn chế tác động môi trường.', 5)
) AS source (IDBaiHoc, IDChuong, TenBaiHoc, NoiDung, ThuTu)
ON target.IDBaiHoc = source.IDBaiHoc
WHEN MATCHED THEN UPDATE SET
    IDChuong = source.IDChuong,
    TenBaiHoc = source.TenBaiHoc,
    NoiDung = source.NoiDung,
    ThuTu = source.ThuTu
WHEN NOT MATCHED THEN
    INSERT (IDBaiHoc, IDChuong, TenBaiHoc, NoiDung, ThuTu)
    VALUES (source.IDBaiHoc, source.IDChuong, source.TenBaiHoc, source.NoiDung, source.ThuTu);

SET IDENTITY_INSERT dbo.BaiHoc OFF;
GO
