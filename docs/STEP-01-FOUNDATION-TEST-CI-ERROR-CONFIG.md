# Bước 01 — Nền tảng test/CI và chuẩn hóa error/config

## Mục tiêu

Bước này tạo lớp bảo vệ trước khi sửa authentication, database và business logic ở các bước sau. Trọng tâm là phát hiện regression tự động, làm lỗi API có cấu trúc ổn định và buộc ứng dụng báo thiếu cấu hình ngay khi khởi động.

## Đã thực hiện

### Backend

- Thay error response tự tạo bằng chuẩn `application/problem+json`.
- Mọi lỗi có `status`, `title`, `detail`, `code`, `traceId` và `instance`.
- Giữ tạm field `message` để frontend hiện tại không bị hỏng trong giai đoạn chuyển đổi.
- Lỗi không mong đợi không còn trả nội dung exception thật cho client.
- Tách log lỗi dự kiến ở mức Warning và lỗi hệ thống ở mức Error.
- Thêm typed options cho JWT và CORS; bổ sung tên section cố định cho Cloudinary.
- Validate connection string, JWT, Cloudinary và CORS lúc startup với thông báo cấu hình cụ thể.
- JWT validation có clock skew 30 giây và thời gian access token lấy từ cấu hình.
- CORS chỉ nhận origin khai báo rõ trong config; không còn tự động cho phép mọi port localhost.
- Xóa file `test.cs` vốn không phải unit test và đang tạo cảnh báo entry point phụ.

### Test và CI

- Thêm project xUnit riêng cho backend.
- Thêm test cho ProblemDetails, chống lộ nội dung exception và xử lý unauthorized.
- Sửa Angular scaffold test đã kiểm tra một tiêu đề không còn tồn tại.
- Thêm lệnh frontend `test:ci` chạy không watch.
- Thêm GitHub Actions chạy backend build/test/coverage và frontend test/production build.
- CI có timeout, quyền chỉ đọc và tự hủy run cũ trên cùng branch.

## File đã thêm

- `TodoListBackend/Options/JwtSettings.cs`: contract typed cho cấu hình JWT.
- `TodoListBackend/Options/CorsSettings.cs`: contract typed cho danh sách origin tin cậy.
- `TodoListBackend.Tests/TodoListBackend.Tests.csproj`: project test backend.
- `TodoListBackend.Tests/Middlewares/ExceptionMiddlewareTests.cs`: test error contract và chống lộ dữ liệu nội bộ.
- `.github/workflows/ci.yml`: pipeline CI cho backend và frontend.
- `docs/STEP-01-FOUNDATION-TEST-CI-ERROR-CONFIG.md`: báo cáo chi tiết của bước hiện tại.

## File đã sửa

- `TodoListBackend/Program.cs`: đăng ký ProblemDetails, options validation, CORS rõ ràng và JWT options.
- `TodoListBackend/Middlewares/ExceptionMiddleware.cs`: chuẩn hóa error contract và logging.
- `TodoListBackend/Exceptions/BusinessException.cs`: thêm error code ổn định.
- `TodoListBackend/Services/AuthService.cs`: dùng `IOptions<JwtSettings>` thay cho truy cập config bằng chuỗi.
- `TodoListBackend/Models/CloudinarySettings.cs`: thêm tên section dùng chung.
- `TodoListBackend/TodoListBackend.csproj`: khai báo trực tiếp EF Core và EF Core Relational `10.0.9` để đồng bộ dependency runtime, loại bỏ xung đột assembly `10.0.4`/`10.0.9` khi build test project.
- `TodoListBackend/appsettings.json`: thêm access-token lifetime và CORS section.
- `TodoListBackend/appsettings.Development.json`: chỉ cho phép Angular localhost ở port 4200.
- `TodoListFrontend/src/app/app.spec.ts`: kiểm tra application shell thật thay cho title scaffold; mô phỏng `window.matchMedia` vì DOM test không cung cấp browser API này.
- `TodoListFrontend/package.json`: thêm script test dành cho CI.

## File đã xóa

- `TodoListBackend/test.cs`: đây chỉ là chương trình in `Test`, không phải test project. Vì nằm trong web project nên compiler phát hiện một entry point phụ và phát cảnh báo ở mỗi lần build.

## Tại sao cần thay đổi

### Nếu không có test và CI

Các thay đổi lớn ở auth, refresh-token, soft-delete và migration có thể làm hỏng chức năng cũ mà không được phát hiện trước khi deploy. File test backend cũ không thực thi assertion nào; test frontend cũ gần như chắc chắn thất bại vì kiểm tra giao diện scaffold đã bị thay thế.

### Nếu giữ error response cũ

Frontend phải đoán nhiều shape lỗi khác nhau. `UnauthorizedAccessException` còn có thể trả thẳng message nội bộ. Không có mã lỗi ổn định nên client phải phụ thuộc vào câu tiếng Việt, khiến đổi wording hoặc dịch ngôn ngữ dễ làm hỏng logic.

### Nếu giữ cách đọc config cũ

Ứng dụng có thể khởi động với connection string, JWT key hoặc Cloudinary credential rỗng rồi chỉ lỗi ở request đầu tiên. Lỗi xuất hiện muộn khó chẩn đoán và có thể tạo deployment nhìn như thành công dù thực tế không dùng được.

### Nếu giữ CORS cũ

Mọi website chạy trên bất kỳ port nào của `localhost` đều được tin cậy, kể cả trong production. Việc tạo `new Uri(origin)` trực tiếp cũng làm cấu hình/giá trị origin không hợp lệ có thể phát sinh exception. Danh sách origin tường minh giúp hành vi development và production dự đoán được.

## Tương thích và tác động vận hành

- Frontend hiện tại vẫn đọc được `error.message` vì backend giữ field tương thích. Các bước frontend sau sẽ chuyển sang `detail` và `code`, rồi mới xóa `message`.
- Khi chạy backend, giờ đây bắt buộc cấu hình:
  - `ConnectionStrings__DefaultConnection`.
  - `Jwt__Key` tối thiểu 32 byte.
  - `Jwt__Issuer`, `Jwt__Audience`.
  - `CloudinarySettings__CloudName`, `CloudinarySettings__ApiKey`, `CloudinarySettings__ApiSecret`.
  - Production cần ít nhất một `Cors__AllowedOrigins__N`.
- Các secret không được ghi vào appsettings hoặc repository; dùng user-secrets ở local và secret manager/environment variables khi deploy.
- Bước này chưa thay đổi API auth, cách lưu token, database schema hoặc business behavior. Các phần đó thuộc các bước sau.

## Kiểm tra thực tế

- `dotnet restore TodoListBackend.Tests/TodoListBackend.Tests.csproj`: thành công.
- `dotnet build ... --configuration Release --no-restore`: thành công, **0 warning, 0 error**.
- `dotnet test ... --configuration Release --no-build --collect:"XPlat Code Coverage"`: thành công, **3/3 test đạt**, có sinh báo cáo Cobertura; artifact kết quả test đã được `.gitignore` loại khỏi Git.
- `npm run test:ci`: thành công, **1 test file và 2/2 test đạt**.
- `npm run build`: thành công. Bundle đầu là `537.08 kB`, vượt budget hiện tại `37.08 kB`; Angular chỉ cảnh báo, không làm build thất bại. Tối ưu bundle sẽ được xử lý ở bước hiệu năng frontend thay vì mở rộng phạm vi bước nền tảng này.
- `git diff --check`: không phát hiện whitespace error; cảnh báo LF/CRLF của Git trên Windows không phải lỗi nội dung.

### Sự cố đã gặp khi xác minh và cách xử lý

- Lần chạy test frontend đầu tiên cho thấy `window.matchMedia` không tồn tại trong môi trường DOM giả lập. Đây không phải lỗi khi chạy trên trình duyệt thật, nhưng làm test không ổn định. Test setup đã được bổ sung mock đúng contract `MediaQueryList`; chạy lại đạt 2/2.
- Production build ban đầu không tải được Google Fonts do môi trường thực thi chặn mạng. Chạy lại với quyền truy cập mạng cho tác vụ build đã thành công. Điều này cũng cho thấy build hiện phụ thuộc tài nguyên Google Fonts bên ngoài; có thể self-host font ở bước tối ưu frontend để build tái lập tốt hơn.
- Lệnh `npm` mặc định trên máy đang trỏ nhầm tới một bản npm không đầy đủ trong hồ sơ người dùng. Việc xác minh đã dùng npm `11.11.0` đi kèm Node.js `24.14.1` tại thư mục cài đặt hệ thống. Đây là lỗi môi trường máy, không phải thay đổi của repository và không ảnh hưởng runner CI sạch.

## Điều chưa thay đổi trong bước này

- Không đổi database schema, không tạo migration và không chỉnh dữ liệu hiện có.
- Không đổi endpoint, request DTO hay response thành công của API.
- Chưa đổi refresh token sang cookie và chưa đổi access token trên frontend; đó là bước bảo mật authentication tiếp theo.
- Chưa xử lý cảnh báo kích thước bundle hoặc phụ thuộc Google Fonts; chúng được ghi nhận để xử lý trong bước hiệu năng/frontend.
- Không sửa hoặc xóa file `ToDo.md` có sẵn của người dùng.

## Việc còn lại sau bước 01

- Mở rộng test sang validator, service và integration test PostgreSQL.
- Chuyển refresh token sang HttpOnly cookie và session table.
- Thêm migration constraints/index và sửa pagination/timezone.
- Chuyển toàn bộ frontend sang ProblemDetails `detail/code` rồi xóa field `message` tương thích.
