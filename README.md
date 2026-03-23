# SQA Automation Testing

Dự án `sqa-automation-testing` là bộ kiểm thử tự động cho website Parabank (ví dụ) sử dụng Selenium WebDriver cùng NUnit.

## Tổng quan
- Ngôn ngữ: C#
- .NET: .NET 8
- Test framework: NUnit
- Trình duyệt: Selenium WebDriver (ChromeDriver/EdgeDriver tuỳ cấu hình)

## Cấu trúc thư mục chính
- `Tests/` - các lớp test (ví dụ `TestFindTransaction.cs`, `TestRegister.cs`)
- `Pages/` - Page Object Model (ví dụ `LoginPage`, `FindTransactionPage`)
- `Utilities/` - helper (ví dụ `DriverFactory`, `ExcelHelper`, `ScreenshotHelper`)
- `sqa-automation-testing.csproj` - file project

## Yêu cầu cài đặt
1. .NET 8 SDK
2. Trình duyệt Chrome hoặc Edge và driver tương ứng (ChromeDriver/EdgeDriver) đặt trong `PATH` hoặc cấu hình thông qua `DriverFactory`.
3. (Nếu cần) file dữ liệu test (Excel) đặt theo cấu hình `ExcelHelper`.

## Chạy test
1. Khôi phục packages:

   `dotnet restore`

2. Chạy tất cả test:

   `dotnet test`

3. Chạy một test cụ thể (ví dụ):

   `dotnet test --filter "TestName=Test_Find_Transaction_Flow"`

Lưu ý: Nếu dùng driver ngoài, đảm bảo service driver đang chạy hoặc driver được cấu hình đúng trong `DriverFactory`.

## Cấu hình và bảo mật
- Thông tin đăng nhập, mật khẩu hoặc dữ liệu nhạy cảm không nên lưu trực tiếp trong mã nguồn. Nên sử dụng biến môi trường hoặc file cấu hình an toàn.
- Kiểm tra `ExcelHelper` để biết vị trí lưu/đọc dữ liệu test và `ScreenshotHelper` cho vị trí lưu ảnh chụp màn hình khi test thất bại.

## Ghi chú
- Test sử dụng Page Object Pattern; để thêm page mới, mở rộng thư mục `Pages/` và viết test trong `Tests/`.
- Nếu muốn chạy song song, cấu hình NUnit và driver để hỗ trợ chạy song song an toàn.

## Liên hệ
Nếu cần mở rộng nội dung hoặc báo lỗi, vui lòng mở issue trên repository.
