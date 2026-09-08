# Bước 02 — Củng cố authentication bằng HttpOnly cookie và session refresh-token

## Mục tiêu

Bước này xử lý rủi ro lớn nhất của luồng đăng nhập hiện tại: refresh token được trả trong JSON response và lưu vào `localStorage`. Một script JavaScript bị XSS có thể đọc token dài hạn đó và duy trì quyền truy cập kể cả sau khi access token hết hạn.

Thiết kế sau bước này là:

1. Backend vẫn trả access token ngắn hạn trong response JSON.
2. Refresh token chỉ được gửi trong cookie `HttpOnly`, nên JavaScript frontend không đọc được.
3. Frontend chỉ giữ access token trong memory của `AuthService`; không lưu access token hoặc refresh token vào `localStorage`.
4. Mỗi lần refresh thành công, refresh token cũ bị revoke và token mới được tạo. Token cũ được dùng lại sẽ khiến các session đang hoạt động của user bị revoke.
5. Database quản lý nhiều session trên một user, thay vì chỉ có một cặp `RefreshToken`/`RefreshTokenExpiryTime` trong bảng `Users`.

## Đã thực hiện

### Backend authentication

- Thay đổi `AuthResponseDto` để chỉ trả `accessToken`.
- Login và register tạo refresh-token session mới, sau đó đặt raw refresh token vào cookie:
  - `HttpOnly = true`.
  - `Secure = true` ở production.
  - `SameSite = Lax` mặc định, phù hợp khi FE và API dùng cùng site hoặc các subdomain cùng site.
  - `Path = /api/auth`, giới hạn cookie chỉ được gửi đến các endpoint authentication.
  - Thời hạn cookie lấy từ cấu hình, mặc định 7 ngày.
- Endpoint refresh đọc token từ cookie trước. `TokenDto` vẫn được giữ ở mức tương thích tạm thời để client cũ có thể gửi body trong giai đoạn chuyển tiếp; client mới không còn gửi refresh token trong body.
- Endpoint logout được phép gọi khi access token đã hết hạn, revoke session tương ứng nếu cookie còn tồn tại và luôn xóa cookie phía trình duyệt.
- Login không còn trả message lỗi xác thực chi tiết; middleware bước 1 tiếp tục trả thông báo chung để giảm khả năng dò username/email.
- Các endpoint authentication đặt `Cache-Control: no-store` để access token và response nhạy cảm không bị proxy/browser cache lưu lại.

### Session rotation và replay protection

- Tạo `RefreshTokenSession` cho từng thiết bị/browser session.
- Chỉ lưu SHA-256 hash của refresh token trong database; raw token chỉ xuất hiện trong response nội bộ trước khi được ghi vào cookie.
- Refresh token đang hoạt động được đánh dấu `RevokedAt` và liên kết với session thay thế qua `ReplacedBySessionId`.
- `ConcurrencyToken` được dùng như optimistic concurrency token. Hai request refresh đồng thời không thể cùng xoay một session thành công.
- Nếu một refresh token đã bị revoke bị sử dụng lại, các session đang hoạt động của user bị revoke để giới hạn nguy cơ token bị sao chép.
- Lưu `UserAgent` và địa chỉ IP của session để phục vụ audit và quản lý session về sau.

### Frontend authentication

- Xóa việc đọc/ghi `accessToken` và `refreshToken` từ `localStorage`.
- `AuthService` giữ access token trong memory và tự xóa token khi logout hoặc refresh thất bại.
- Tất cả request authentication dùng `withCredentials: true` để browser gửi HttpOnly cookie.
- Interceptor vẫn tự gắn access token vào header `Authorization` và tự refresh khi nhận 401.
- Thêm `restoreSession()` để gọi refresh khi ứng dụng khởi động, nhờ đó reload trang không làm user bị logout ngay lập tức nếu cookie còn hợp lệ.
- Auth guard chờ restore session trước khi chuyển user về trang login; đồng thời giữ `returnUrl`.
- Dùng `provideAppInitializer` để khôi phục session sớm trong lifecycle của Angular.

### Database

- Thêm migration `20260907114157_AddRefreshTokenSessions`.
- Migration tạo bảng `RefreshTokenSessions` với:
  - khóa chính UUID;
  - khóa ngoại đến `Users` với cascade delete;
  - hash token, thời điểm tạo/hết hạn/revoke;
  - quan hệ token bị thay thế;
  - user agent, IP và concurrency token.
- Thêm unique index trên `TokenHash` để không thể tồn tại hai session cùng hash.
- Thêm index `(UserId, RevokedAt)` để truy vấn các session đang hoạt động nhanh hơn.
- Không xóa hai cột legacy `Users.RefreshToken` và `Users.RefreshTokenExpiryTime` trong bước này. Luồng cũ được hỗ trợ tạm thời và khi refresh thành công sẽ chuyển sang session table rồi xóa giá trị legacy.

## File đã thêm

- `TodoListBackend/Options/RefreshTokenSettings.cs`: contract cấu hình thời hạn và cookie.
- `TodoListBackend/Models/RefreshTokenSession.cs`: entity session refresh token.
- `TodoListBackend/Repositories/IRefreshTokenSessionRepository.cs`: abstraction truy vấn session.
- `TodoListBackend/Repositories/RefreshTokenSessionRepository.cs`: truy vấn session có tracking và chống truy cập token hash không tồn tại.
- `TodoListBackend/Services/AuthSessionContext.cs`: metadata user-agent/IP của session.
- `TodoListBackend/Services/AuthTokenResult.cs`: kết quả nội bộ chứa access token và raw refresh token; raw refresh token không được serialize ra API.
- `TodoListBackend/Migrations/20260907114157_AddRefreshTokenSessions.cs`: migration tạo bảng và index.
- `TodoListBackend/Migrations/20260907114157_AddRefreshTokenSessions.Designer.cs`: model snapshot của migration.
- `TodoListBackend.Tests/Controllers/AuthControllerTests.cs`: kiểm tra cookie HttpOnly, response không chứa refresh token, refresh đọc cookie và logout xóa cookie.
- `docs/STEP-02-AUTH-COOKIE-SESSION-ROTATION.md`: báo cáo chi tiết bước này.

## File đã sửa

- `TodoListBackend/Controllers/AuthController.cs`: đặt/xóa cookie, đọc cookie refresh, cho phép logout khi access token hết hạn, map response công khai.
- `TodoListBackend/DTOs/Auth/AuthResponseDto.cs`: loại bỏ `RefreshToken` khỏi response API.
- `TodoListBackend/Data/AppDbContext.cs`: thêm DbSet và mapping/index/foreign key cho session.
- `TodoListBackend/Models/User.cs`: thêm navigation collection; giữ cột legacy trong thời gian chuyển tiếp.
- `TodoListBackend/Program.cs`: đăng ký repository session, validate cấu hình refresh token và cho phép CORS credentials.
- `TodoListBackend/Repositories/IUnitOfWork.cs`: expose repository session.
- `TodoListBackend/Repositories/UnitOfWork.cs`: nhận và lưu repository session.
- `TodoListBackend/Services/IAuthService.cs`: đổi contract sang session context và token result nội bộ.
- `TodoListBackend/Services/AuthService.cs`: tạo session, rotation, replay detection, concurrency protection và compatibility path cho token legacy.
- `TodoListBackend/appsettings.json`: thêm section `RefreshToken` cho production.
- `TodoListBackend/appsettings.Development.json`: tắt `Secure` để cookie hoạt động khi chạy HTTP localhost.
- `TodoListFrontend/src/app/app.config.ts`: khôi phục session khi khởi động app.
- `TodoListFrontend/src/app/core/services/auth.service.ts`: chuyển token storage từ localStorage sang memory và dùng cookie credentials.
- `TodoListFrontend/src/app/core/models/auth.model.ts`: bỏ refresh token khỏi model response và bỏ request model không còn cần cho client mới.
- `TodoListFrontend/src/app/core/interceptors/token.interceptor.ts`: gửi credentials và chỉ dùng access token trong memory.
- `TodoListFrontend/src/app/core/guards/auth.guard.ts`: restore session bất đồng bộ trước khi redirect.

## File không xóa

- `TodoListBackend/DTOs/Auth/TokenDto.cs` vẫn được giữ vì backend còn nhận body refresh token trong giai đoạn chuyển tiếp. Khi toàn bộ client cũ đã được nâng cấp, có thể xóa DTO này và bỏ fallback body trong controller.
- `TodoListBackend/Models/User.cs` vẫn giữ các cột refresh-token cũ để tránh migration destructive. Chúng sẽ được xóa trong bước dọn schema sau khi xác nhận không còn client legacy.

## Tại sao cần thay đổi

### Nếu giữ refresh token trong localStorage

Bất kỳ XSS nào chạy được trong origin frontend đều có thể đọc token dài hạn bằng JavaScript. Việc đổi access token ngắn hạn không giải quyết được vấn đề nếu attacker vẫn dùng refresh token để xin token mới.

### Nếu tiếp tục lưu một refresh token trên User

User chỉ có thể có một session hiệu quả. Đăng nhập trên thiết bị thứ hai ghi đè token của thiết bị thứ nhất; logout hoặc refresh trên một thiết bị có thể làm thiết bị khác mất phiên. Ngoài ra không có lịch sử revoke, thông tin thiết bị hoặc cơ chế phát hiện token cũ bị replay.

### Nếu không xoay token khi refresh

Refresh token bị sao chép có thể được sử dụng lặp lại cho đến khi hết hạn. Rotation làm token cũ chỉ hợp lệ một lần; replay của token đã revoke trở thành tín hiệu để vô hiệu hóa các session còn lại của user.

### Nếu xóa ngay cột legacy

Migration sẽ làm mất hash refresh-token đang tồn tại và tất cả user đang giữ session cũ sẽ bị logout đột ngột. Bước này giữ cột cũ, hỗ trợ chuyển tiếp một lần, rồi mới dọn schema ở bước sau.

## Tác động API

- `POST /api/auth/login`: response chỉ còn `{ accessToken }`; refresh token nằm trong `Set-Cookie`.
- `POST /api/auth/register`: response chỉ còn `{ accessToken }`; refresh token nằm trong `Set-Cookie`.
- `POST /api/auth/refresh-token`: client mới không cần body; browser tự gửi cookie và response trả access token mới cùng cookie mới.
- `POST /api/auth/logout`: có thể gọi không cần access token để xóa cookie và revoke session theo cookie.
- Access token vẫn là Bearer token như trước; các endpoint business không đổi contract thành công.

## Cấu hình triển khai

- Production phải giữ `RefreshToken:Secure = true`.
- Khi FE và API là các subdomain cùng site, `SameSite = Lax` là mặc định phù hợp.
- Nếu triển khai FE và API ở hai site khác nhau, cần cấu hình `SameSite = None`, bắt buộc `Secure = true`, HTTPS và CORS origin cụ thể với credentials. Không dùng `AllowAnyOrigin` cùng credentials.
- Development HTTP localhost dùng `Secure = false` trong `appsettings.Development.json`; không copy giá trị này sang production.
- Cần chạy migration trên database thật bằng quy trình deploy của dự án trước khi dùng endpoint mới. Migration đã được tạo và compile, nhưng chưa tự động chạy trên database vì workspace không có connection string production.
- Không đưa connection string, JWT key, Cloudinary secret hoặc cookie domain nhạy cảm vào Git.

## Kiểm tra thực tế

- `dotnet build TodoListBackend/TodoListBackend.csproj --configuration Release --no-restore`: thành công, **0 warning, 0 error**.
- `dotnet build TodoListBackend.Tests/TodoListBackend.Tests.csproj --configuration Release --no-restore`: thành công, **0 warning, 0 error**.
- `dotnet test TodoListBackend.Tests/TodoListBackend.Tests.csproj --configuration Release --no-build --collect:"XPlat Code Coverage"`: **6/6 test đạt**, gồm 3 test error contract và 3 test cookie/controller.
- `npm run test:ci`: **2/2 test đạt**.
- `npm run build`: thành công; bundle đầu `539.15 kB`, vượt budget `500 kB` khoảng `39.15 kB`. Đây là cảnh báo hiệu năng đã ghi nhận, không phải lỗi authentication.
- Migration được tạo lại sau khi phát hiện lần chạy đầu dùng assembly Debug cũ và sinh migration rỗng. Artifact rỗng đã bị loại bỏ; migration hiện tại có đầy đủ `CreateTable` và index cho `RefreshTokenSessions`.
- `dotnet user-secrets list` xác nhận có các key `ConnectionStrings:DefaultConnection`, `Jwt:Key` và Cloudinary; giá trị không được in hoặc ghi vào repository.
- Kiểm tra DNS/TCP với hostname PostgreSQL hiện tại đã thành công: hostname phân giải tới IP `134.209.153.206` và cổng `23362` trả về `TcpTestSucceeded = True`. Điều này xác nhận đường mạng tới Aiven hoạt động, nhưng tự nó chưa xác nhận username/password.
- Lần chạy `dotnet ef migrations list` đầu tiên đã kết nối tới host nhưng bị PostgreSQL từ chối xác thực với lỗi `28P01` (`password authentication failed for user "avnadmin"`). Nguyên nhân là password trong `dotnet user-secrets` đã cũ hoặc không khớp với credential hiện tại của Aiven.
- Sau khi cập nhật lại connection string trong `dotnet user-secrets` từ Aiven, `dotnet ef migrations list --no-build` đã kết nối thành công và đọc được bảng `__EFMigrationsHistory`. Lệnh trả về ba migration trong source: `20260718072752_InitialPostgres`, `20260720044133_AddIsHiddenToTodo` và `20260907114157_AddRefreshTokenSessions`.
- Người dùng đã xác nhận database đích được cập nhật bằng `dotnet ef database update` và automatic backup trên Aiven ở trạng thái OK trước khi triển khai.
- Người dùng đã xác nhận backend trên Render deploy thành công sau khi cập nhật cấu hình database.
- Kiểm tra sau triển khai: backend test **6/6 đạt**; frontend test **2/2 đạt** khi chạy bằng `npm.cmd run test:ci`; production build thành công. Lần gọi `npm run test:ci` đầu tiên gặp lỗi launcher npm của môi trường (`npm-cli.js` không tìm thấy), không phải lỗi test ứng dụng; chạy lại bằng `npm.cmd` đã thành công.
- Production build vẫn phát cảnh báo bundle ban đầu `539.15 kB`, vượt budget `500 kB` khoảng `39.15 kB`. Đây là hạng mục tối ưu hiệu suất còn lại, không chặn deploy.

## Ngoài phạm vi bước 2 và điều kiện triển khai

- Migration database và deploy production đã hoàn tất theo xác nhận của người dùng. Chưa thực hiện tối ưu bundle frontend hoặc dọn các artifact legacy trong bước này.
- Chưa có màn hình quản lý và revoke từng thiết bị; bảng session đã lưu đủ metadata để làm ở bước sau.
- Chưa xóa cột legacy và `TokenDto`; chỉ xóa sau khi client cũ đã được nâng cấp.
- Chưa tối ưu bundle frontend và chưa self-host Google Fonts.
- Không sửa hoặc xóa file `ToDo.md` có sẵn của người dùng.
