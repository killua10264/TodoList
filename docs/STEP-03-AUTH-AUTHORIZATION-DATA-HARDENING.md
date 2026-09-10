# Bước 3 — Auth, authorization và data integrity

## Trạng thái

Đã hoàn tất Giai đoạn 3: code đã build/test, dữ liệu đích đã được kiểm tra, hai migration đã áp dụng trên Aiven, BE/FE đã deploy lên Render và smoke test production đã hoàn tất theo xác nhận của người dùng ngày 2026-09-08.

Integration test ownership bằng PostgreSQL riêng/Testcontainers vẫn là việc tăng cường test còn lại trước khi đóng hoàn toàn technical debt; không dùng Aiven production làm test database.

## Đã làm gì và tại sao

### 1. Auth, cookie và refresh session

- Access token mặc định còn 10 phút.
- Register/login/refresh chỉ trả accessToken, expiresAt và user; refresh token không còn ở JSON.
- Refresh endpoint không nhận token trong body, chỉ đọc HttpOnly cookie.
- Refresh, logout và logout-all bắt buộc header X-CSRF-Protection: 1.
- Refresh session được rotate trong transaction. Session cũ lưu LastUsedAt, RevokedAt, RevocationReason=rotated và ReplacedBySessionId.
- Refresh token đã revoke mà bị dùng lại sẽ thu hồi các session còn hoạt động với reason refresh_replay.
- Logout ghi reason logout; logout-all ghi logout_all; đổi mật khẩu ghi password_changed và thu hồi toàn bộ session cùng token legacy.
- Production dùng cookie __Host-todolist-refresh, Secure, SameSite=Lax, Path=/, không khai báo Domain. Development giữ cookie HTTP riêng.
- JWT vẫn validate key, issuer, audience, lifetime, signing key; clock skew là 30 giây.
- Auth endpoints có rate limit; avatar được partition theo user ID sau authentication. Login hiện giới hạn 5 lần/phút/IP, nghiêm ngặt hơn giới hạn IP 20 lần/phút trong plan. Partition chính xác đồng thời theo username/email chưa thêm vì rate limiter chạy trước model binding và không nên đọc request body ở đó.

Nếu giữ code cũ, refresh token có thể bị log trong body, session cũ còn dùng được sau đổi mật khẩu, không có logout-all và endpoint cookie có nguy cơ CSRF.

### 2. Authorization và ownership

- Thêm fallback policy yêu cầu authenticated user cho mọi endpoint không có AllowAnonymous.
- Todo query/mutation luôn nhận userId từ claim và lọc UserId trong query.
- Category query/mutation lọc UserId trong query.
- SubTask query kiểm tra SubTask.Id, TodoId và Todo.UserId trong cùng query; không lấy ID trước rồi kiểm tra quyền ở memory.
- Resource không tồn tại và resource thuộc user khác không bị phân biệt thành 403 để tránh dò ID.
- Sửa side effect của GET Todo: lọc theo category không còn gọi logic tạo category mặc định.

Integration test hai user cho toàn bộ read/update/soft-delete/restore/hard-delete chưa chạy vì workspace không có PostgreSQL test instance riêng. Đây là test gate còn lại, không phải lý do nới lỏng ownership trong code.

### 3. Soft-delete

- Todo có global query filter !IsDeleted.
- Query thông thường và SubTask navigation tự loại Todo đã xóa mềm.
- Trash dùng IgnoreQueryFilters rồi lọc lại theo user và IsDeleted=true.
- Restore/hard-delete dùng query includeDeleted=true, tương đương IgnoreQueryFilters.
- Category delete đọc cả Todo đã xóa mềm để reassign trước khi xóa category, tránh foreign key còn trỏ tới category cũ.

Nếu chỉ lọc thủ công, endpoint mới dễ quên IsDeleted; nếu chỉ dùng global filter, restore/hard-delete sẽ không tìm thấy bản ghi. Hai đường query đã được tách riêng.

### 4. Typed query và validation

TodoQueryDto giới hạn Page 1–1.000.000, PageSize 1–100, Search tối đa 200 ký tự; Filter, Status, SortBy là enum. Tham số sai đi qua ValidationProblemDetails, không để Skip/Take nhận giá trị nguy hiểm.

Giới hạn được đồng bộ theo model: Todo title 200, description 1000, SubTask title 200, Category name 100, Username 30, Email 150, DisplayName 100, Bio 300, Timezone 100. Profile validator đã null-safe để request gửi setting null không làm server ném NullReferenceException.

### 5. DateOnly

Todo.DueDate, request create/update và response đã chuyển từ DateTime sang DateOnly. JSON giữ dạng YYYY-MM-DD; filter Today/Upcoming so sánh ngày lịch trực tiếp. FE tạo ngày hiện tại bằng local calendar, không dùng toISOString(); tree view format chuỗi ngày thủ công, không parse thành UTC instant.

Nếu giữ DateTime, user ở múi giờ khác có thể thấy ngày trước/sau ngày đã chọn. Migration dùng AT TIME ZONE UTC vì dữ liệu cũ được lưu ở UTC midnight.

### 6. Profile và avatar

PUT /api/users/profile chỉ nhận username, bio, timezone, theme, language và firstDayOfWeek. Email chỉ đọc; avatar dùng endpoint riêng.

POST /api/users/profile/avatar và DELETE /api/users/profile/avatar đã được thêm/cập nhật. Upload:

- giới hạn 5 MB;
- chỉ nhận JPG/JPEG/PNG/WEBP;
- kiểm tra extension, MIME và magic bytes;
- server sinh Cloudinary public ID;
- Cloudinary decode ảnh, width/height tối đa 8000 x 8000;
- lưu cả URL và public ID;
- DB update thất bại thì xóa asset mới;
- thay/xóa ảnh thì best-effort xóa asset cũ;
- không trả nguyên lỗi provider cho client.

Nếu chỉ lưu URL hoặc chỉ kiểm tra extension, asset cũ có thể bị orphan và file giả có thể đi qua validation.

### 7. SubTask và optimistic concurrency

- SubTaskUpdateDto chỉ còn title, completion state và version; SortOrder chỉ thay đổi qua endpoint reorder.
- Thêm PUT /api/todos/{todoId}/subtasks/order với toàn bộ subTaskId, sortOrder; API kiểm tra đủ ID, không duplicate, order liên tục và cùng ownership.
- Todo/SubTask response có version map từ PostgreSQL system column xmin.
- Update/delete/restore/hard-delete yêu cầu version hiện tại; delete thiếu version bị model binding từ chối.
- Mismatch version hoặc DbUpdateConcurrencyException trả ProblemDetails 409 code concurrency_conflict.
- FE truyền version cho mutation, rollback optimistic SubTask khi lỗi và reload Todo khi gặp 409.

Nếu giữ code cũ, hai tab có thể ghi đè thay đổi của nhau mà không báo lỗi. xmin là system column PostgreSQL nên không tạo cột vật lý.

## Database migrations

### 20260907133801_AddStage3DataIntegrity

Migration additive, không xóa dữ liệu:

- thêm Users.AvatarPublicId nullable;
- thêm RefreshTokenSessions.LastUsedAt nullable;
- thêm RefreshTokenSessions.RevocationReason nullable, tối đa 100 ký tự.

Todo.Version/SubTask.Version dùng xmin; script Npgsql đã được kiểm tra và không có ADD COLUMN xmin.

### 20260907134859_UseDateOnlyForTodoDueDate

- fail rõ ràng nếu có username dài hơn 30;
- đổi Users.Username từ varchar(50) xuống varchar(30);
- đổi Todos.DueDate từ timestamp with time zone sang date bằng UTC conversion;
- Down migration đổi date ngược về UTC timestamp.

Migration không tự cắt username dài để tránh mất dữ liệu.

### Kiểm tra trước khi chạy Aiven

Chạy read-only trên bản sao/SQL console Aiven:

    SELECT COUNT(*) AS usernames_over_30
    FROM "Users"
    WHERE char_length("Username") > 30;

    SELECT COUNT(*) AS todos_with_non_midnight_due_date
    FROM "Todos"
    WHERE "DueDate" <> (("DueDate" AT TIME ZONE 'UTC')::date::timestamp AT TIME ZONE 'UTC');

Nếu kết quả không phải 0, dừng lại để review dữ liệu. Sau khi backup và kiểm tra, chạy:

    cd D:\ToDoList\TodoListBackend
    dotnet ef database update --configuration Release

Lệnh phải dùng ConnectionStrings__DefaultConnection hoặc dotnet user-secrets đúng DB đích. Theo xác nhận sau đó, hai migration đã được áp dụng thành công trên Aiven.

## File đã sửa

### Backend

- Auth: Controllers/AuthController.cs, Services/AuthService.cs, Services/UserService.cs, Services/IAuthService.cs, Services/IUserService.cs, Services/AuthTokenResult.cs, DTOs/Auth/AuthResponseDto.cs, Models/RefreshTokenSession.cs, Options/JwtSettings.cs, Options/RefreshTokenSettings.cs, Program.cs, appsettings.json, appsettings.Development.json.
- Profile/avatar: Controllers/UserController.cs, Services/PhotoService.cs, Services/IPhotoService.cs, Models/User.cs, Data/AppDbContext.cs.
- Todo/SubTask: Models/Todo.cs, Models/SubTask.cs, DTOs Todo/SubTask, mapper, controller, service, interface, repository và validator tương ứng.
- Category: Repositories/CategoryRepository.cs và phần category filter trong TodoService.cs.
- Migration snapshot: Migrations/AppDbContextModelSnapshot.cs.

### Frontend

- Auth/session: core/services/auth.service.ts, core/interceptors/error.interceptor.ts, core/models/auth.model.ts, features/user/change-password/change-password.ts.
- Contract/service: core/models/user.model.ts, todo.model.ts, subtask.model.ts, core/services/user.service.ts, todo.service.ts, subtask.service.ts.
- UI/state: features/user/user-profile/user-profile.ts, features/home/home.ts, todo-form-dialog/todo-form-dialog.ts, todo-list/todo-list.ts, todo-tree-view/todo-tree-view.ts, todo-tree-view.html.
- Build: angular.json tắt font inlining lúc build để CI không phụ thuộc network Google Fonts; font vẫn được browser tải runtime.

### Tests/docs

- TodoListBackend.Tests/Controllers/AuthControllerTests.cs cập nhật cookie/CSRF/logout-all.
- TodoListBackend.Tests/Validators/ValidationTests.cs thêm profile null-safety và version validation.
- docs/STEP-02-AUTH-COOKIE-SESSION-ROTATION.md đã được cập nhật từ bước trước.

## File thêm mới

- TodoListBackend/DTOs/Todo/TodoQueryDto.cs
- TodoListBackend/DTOs/SubTask/SubTaskOrderDto.cs
- TodoListBackend/DTOs/User/ProfileUpdateDto.cs
- TodoListBackend/Services/AvatarUpdateResult.cs
- TodoListBackend/Services/PhotoUploadResult.cs
- TodoListBackend/Validators/ProfileUpdateDtoValidator.cs
- TodoListBackend/Migrations/20260907133801_AddStage3DataIntegrity.cs và Designer.cs
- TodoListBackend/Migrations/20260907134859_UseDateOnlyForTodoDueDate.cs và Designer.cs
- TodoListBackend.Tests/Validators/ValidationTests.cs

## File xóa

- TodoListBackend/DTOs/Auth/TokenDto.cs: refresh token không còn nhận từ body.
- TodoListBackend/DTOs/User/UserUpdateDto.cs: thay bằng profile request không có email/avatar.
- TodoListBackend/Validators/UserUpdateDtoValidator.cs: thay bằng validator cho DTO mới.

Nếu giữ file cũ, contract dễ bị dùng nhầm: refresh token có thể quay lại JSON body hoặc profile endpoint tiếp tục nhận field không thuộc trách nhiệm của nó.

## File cố ý không sửa

- ToDo.md: plan gốc do người dùng sở hữu.
- dotnet user-secrets, connection string, JWT key và Cloudinary secret: không đọc/ghi secret thật và không đưa vào commit.
- Aiven database: đã áp dụng hai migration; không đưa credential vào repository.
- docs/STEP-01-FOUNDATION-TEST-CI-ERROR-CONFIG.md: giữ lịch sử Bước 1.

## Kiểm thử đã chạy

- Backend Release build: Pass, 0 warning, 0 error.
- Backend tests: Pass, 11/11.
- Angular unit test bằng CLI cục bộ: Pass, 2/2.
- Angular production build: Pass, initial 520.75 kB.
- Migration SQL script: Pass; không tạo cột vật lý xmin.
- git diff --check: không có lỗi nội dung; chỉ còn cảnh báo line ending LF/CRLF của Git.

Production bundle còn warning khoảng 20.75 kB trên budget 500 kB; đây là debt hiệu suất của Giai đoạn 5, không chặn chức năng Giai đoạn 3.

## Nếu giữ nguyên code cũ thì sao?

- Refresh token có thể nằm trong body/log và bị replay sau đổi password.
- Không có logout-all, revocation reason và replay detection.
- Endpoint mới có thể public nếu quên Authorize.
- GET category filter có thể tạo dữ liệu ngoài ý muốn.
- Query mới có thể lộ soft-deleted Todo.
- Due date có thể lệch ngày theo timezone.
- Profile có thể nhận email/avatar; avatar cũ có thể thành orphan Cloudinary asset.
- SubTask có hai đường sửa order.
- Concurrent update có thể ghi đè im lặng.
- Pagination/search không có giới hạn cứng.

## Bước vận hành tiếp theo

1. Đã smoke test auth, refresh, logout-all, đổi password, avatar, Todo trash/restore/hard-delete, SubTask reorder và 409 trên production.
2. Đã xác nhận migration history sau deploy; migration cuối là 20260907134859_UseDateOnlyForTodoDueDate.
3. Việc tùy chọn còn lại: tạo PostgreSQL test instance riêng và thêm integration test hai user để tăng coverage trước hoặc song song với Giai đoạn 4.

Không đưa password, JWT key, Cloudinary secret hoặc connection string thật vào Markdown/git.
