# SQA Automation Testing

D? án "sqa-automation-testing" là b? test t? ??ng cho website Parabank (ví d?) s? d?ng Selenium WebDriver cùng NUnit.

## T?ng quan
- Ngôn ng?: C#
- .NET: .NET 8
- Test framework: NUnit
- Trình duy?t: Selenium WebDriver (ChromeDriver/EdgeDriver tu? thi?t l?p)

## C?u trúc th? m?c chính
- `Tests/` - các l?p test (ví d? `TestFindTransaction.cs`, `TestRegister.cs`)
- `Pages/` - Page Object Model (ví d? `LoginPage`, `FindTransactionPage`)
- `Utilities/` - helper (ví d? `DriverFactory`, `ExcelHelper`, `ScreenshotHelper`)
- `sqa-automation-testing.csproj` - project file

## Yêu c?u cài ??t
1. .NET 8 SDK
2. Trình duy?t Chrome ho?c Edge và driver t??ng ?ng (ChromeDriver/EdgeDriver) ??t trong PATH ho?c c?u hình thông qua `DriverFactory`.
3. (N?u c?n) file d? li?u test (Excel) ??t theo c?u hình `ExcelHelper`.

## Ch?y test
1. Khôi ph?c packages:

   `dotnet restore`

2. Ch?y t?t c? test:

   `dotnet test`

3. Ch?y m?t test c? th? (ví d?):

   `dotnet test --filter "TestName=Test_Find_Transaction_Flow"`

L?u ý: N?u dùng driver ngo?i lai, ??m b?o service driver ?ang ch?y ho?c driver ???c c?u hình ?úng trong `DriverFactory`.

## C?u hình và b?o m?t
- Thông tin ??ng nh?p, m?t kh?u ho?c d? li?u nh?y c?m không nên l?u tr?c ti?p trong mã. N?u hi?n có, hãy thay b?ng bi?n môi tr??ng ho?c file c?u hình an toàn.
- Ki?m tra `ExcelHelper` ?? bi?t n?i l?u/??c d? li?u test và `ScreenshotHelper` cho n?i l?u ?nh ch?p khi test fail.

## Ghi chú
- Test s? d?ng Page Object Pattern; thêm page m?i thì m? r?ng `Pages/` và vi?t test trong `Tests/`.
- N?u mu?n ch?y song song, c?u hình NUnit và driver ?? h? tr? song song an toàn.

## Liên h?
Mô t? thêm ho?c báo l?i, m? issue trên repository.
