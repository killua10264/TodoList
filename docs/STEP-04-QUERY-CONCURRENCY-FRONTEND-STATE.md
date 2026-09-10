# Giai đoạn 4 — Tối ưu query, concurrency và frontend state

Ngày thực hiện: 2026-09-08

## 1. Phạm vi và trạng thái

Giai đoạn này được thực hiện song song với việc bổ sung integration test ownership của backend.

Đã hoàn thành và đã kiểm tra bằng build/test:

- Backend đổi tìm kiếm Todo sang PostgreSQL `ILIKE` và có migration thêm index cho các truy vấn danh sách, category, trạng thái, refresh session và tìm kiếm trigram.
- Backend có test harness `WebApplicationFactory` + PostgreSQL Testcontainers; test ownership của hai user đã được viết, không dùng Aiven production.
- Frontend có `AuthFacade` dùng signals với ba trạng thái `initializing`, `authenticated`, `anonymous`.
- Access token vẫn chỉ ở memory; interceptor chỉ gắn Bearer token và credentials cho URL API tin cậy.
- Route feature được lazy-load.
- Theme khôi phục đúng `light`, `dark`, `system`; listener `matchMedia` được tháo khi service bị dispose.
- Event bus `Subject` của Todo/Category/SubTask được thay bằng signal version/state.
- Search có `switchMap` sau debounce để request cũ bị hủy khi người dùng nhập tiếp.
- Toggle Todo chặn request trùng cùng item, giữ version hiện tại và xử lý 409 bằng reload/thông báo.
- Todo form và change-password form đã bỏ `as any`.
- Bộ test frontend hiện chạy được bằng Angular CLI local.

Chưa hoàn thành trong chính bước này:

- Máy phát triển hiện không có Docker, vì vậy ownership test chưa được chạy với PostgreSQL thật; test được đánh dấu `Skipped` khi chưa bật opt-in.
- Migration index mới chưa được chạy lên Aiven. Không tự động chạy vì đây là thay đổi schema trên database production.
- Chưa triển khai đầy đủ accessibility dialog/focus trap, Playwright E2E, bundle analyzer và benchmark 10.000 Todo; đây là các phần tiếp theo của checklist Stage 4/5.

## 2. Các file đã thêm

### Backend integration test

`TodoListBackend.Tests/Integration/PostgresTestFixture.cs`

Đã thêm:

- PostgreSQL container dùng image `postgres:16-alpine`.
- `WebApplicationFactory<Program>` chạy app trong environment `Testing`.
- Override connection string, JWT, cookie, CORS và Cloudinary bằng giá trị test giả; không đọc hoặc ghi secret production.
- Tự chạy `Database.MigrateAsync()` trên database container rỗng.
- Custom `PostgresFactAttribute`: mặc định test được báo `Skipped`; chỉ chạy khi có:

```powershell
$env:RUN_POSTGRES_INTEGRATION_TESTS = "1"
```

Container chỉ được tạo sau khi biến này được bật. Điều này quan trọng vì nếu container được khởi tạo ngay trong constructor thì mọi `dotnet test` thông thường sẽ phụ thuộc Docker.

### Ownership test

`TodoListBackend.Tests/Integration/OwnershipIntegrationTests.cs`

Đã thêm một scenario với hai user:

1. User A tạo category, Todo và SubTask.
2. User B thử list, đọc chi tiết, update, soft-delete, restore và hard-delete Todo.
3. User B thử đọc, update và delete SubTask.
4. User B thử đọc, update và delete Category.
5. Tất cả thao tác xuyên ownership phải trả `404`, không để lộ việc bản ghi tồn tại.
6. User A vẫn đọc được dữ liệu sau mọi request của User B.

Nếu giữ nguyên code mà không có test này, một endpoint mới rất dễ vô tình query theo `id` mà quên `userId`; unit test service không bảo đảm được toàn bộ route, middleware, EF query filter và serialization hoạt động cùng nhau.

## 3. Các file backend đã sửa

### `TodoListBackend.Tests/TodoListBackend.Tests.csproj`

Đã thêm:

- `Microsoft.AspNetCore.Mvc.Testing` 10.0.10 để boot API thật trong `WebApplicationFactory`.
- `Testcontainers` 4.13.0 để chạy PostgreSQL cô lập.

Nếu giữ nguyên project test hiện tại, test chỉ kiểm tra controller/service bằng mock hoặc object trực tiếp; nó không chứng minh ownership filtering trong EF, migration, PostgreSQL `xmin` concurrency token và pipeline authorization thực sự hoạt động.

### `TodoListBackend/Program.cs`

Đã thêm environment guard để bỏ HTTPS redirect trong `Testing`:

```csharp
if (!app.Environment.IsEnvironment("Testing"))
{
    app.UseHttpsRedirection();
}
```

`TestServer` không cung cấp HTTPS endpoint như server production. Nếu giữ redirect trong test, request HTTP sẽ bị 307 redirect và test có thể kiểm tra nhầm redirect thay vì API. Guard chỉ tác động environment `Testing`; production vẫn bắt HTTPS như trước.

Đã thêm `public partial class Program { }` để `WebApplicationFactory<Program>` truy cập entry point của ứng dụng.

### `TodoListBackend/Repositories/TodoRepository.cs`

Đã đổi tìm kiếm từ:

```csharp
t.Title.ToLower().Contains(searchLower)
```

sang PostgreSQL `ILIKE`:

```csharp
EF.Functions.ILike(t.Title, searchPattern)
```

Lý do:

- `ILIKE` biểu đạt rõ tìm kiếm không phân biệt hoa thường trên PostgreSQL.
- Kết hợp được với trigram GIN index trong migration mới.
- Tránh gọi `ToLower()` lên từng cột theo cách dễ làm query khó dùng index.
- Ký tự `\\`, `%` và `_` từ input được escape trước khi tạo pattern để người dùng không vô tình biến toàn bộ query thành wildcard.

Nếu giữ nguyên, danh sách nhỏ vẫn chạy được nhưng khi dữ liệu lên hàng chục nghìn Todo, search có nguy cơ full scan và latency tăng mạnh.

### `TodoListBackend/Data/AppDbContext.cs`

Đã thêm các index model:

- `Todos(UserId, CategoryId, IsDeleted)` cho filter category.
- `Todos(UserId, IsDeleted, IsCompleted)` cho filter trạng thái.
- `Todos(UserId, IsDeleted, IsHidden, DueDate, Id)` cho list/filter ngày/sort chính.
- `RefreshTokenSessions(UserId, RevokedAt, ExpiresAt)` cho session còn hoạt động/hết hạn.

Index cũ không bị xóa trong bước này để tránh thay đổi ngoài phạm vi và để rollback migration đơn giản hơn. Sau khi có số liệu `EXPLAIN ANALYZE` production-like mới quyết định có cần bỏ index trùng hay không.

### `TodoListBackend/Migrations/20260908091644_AddStage4QueryIndexes.cs`

Đây là migration mới. `Up` thực hiện:

- Tạo `pg_trgm` nếu chưa có.
- Tạo bốn composite index ở trên.
- Tạo GIN trigram index cho `Todos.Title` và `Todos.Description`.

`Down` xóa đúng các index Stage 4 nhưng không xóa extension `pg_trgm`, vì extension có thể được dùng bởi query/index khác sau này.

Migration này chưa được chạy trên Aiven. Không tự chạy production migration chỉ vì file đã được tạo; cần backup/checksum và xác nhận trạng thái `__EFMigrationsHistory` trước.

### File migration sinh kèm

- `TodoListBackend/Migrations/20260908091644_AddStage4QueryIndexes.Designer.cs`: metadata target model do EF Core sinh, giúp EF biết model tại thời điểm migration.
- `TodoListBackend/Migrations/AppDbContextModelSnapshot.cs`: snapshot được cập nhật để các migration sau nhìn thấy bốn composite index mới.

Hai file này không được viết lại thủ công ngoài phần migration `Up`/`Down` cần thêm SQL `pg_trgm`; không xóa chúng vì EF Core sẽ mất lịch sử model và có thể sinh migration sai ở bước sau.

## 4. Các file frontend đã thêm

### `TodoListFrontend/src/app/core/services/auth.facade.ts`

Đã thêm facade quản lý trạng thái auth bằng signals:

- `initializing`: app chưa kết luận session.
- `authenticated`: refresh cookie thành công hoặc login/register thành công.
- `anonymous`: refresh thất bại, logout hoặc session bị invalid.

Facade cũng xóa một lần các key legacy `accessToken`/`refreshToken` khỏi localStorage và sessionStorage. Access token mới vẫn do `AuthService` giữ trong memory.

Nếu giữ cách kiểm tra `accessToken !== null` ở nhiều component, UI có thể quyết định route trước khi refresh cookie hoàn tất hoặc vô tình khởi động nhiều refresh request.

### `TodoListFrontend/src/app/app.config.ts`

App initializer hiện gọi `AuthFacade.initialize()` thay vì gọi trực tiếp service. Vì vậy router chờ kết luận auth ban đầu trước khi guard quyết định.

### `TodoListFrontend/src/app/core/guards/auth.guard.ts`

Guard dựa trên auth state. Trạng thái `anonymous` không tự refresh lại vô hạn; người dùng được chuyển về login với `returnUrl`.

### `TodoListFrontend/src/app/core/interceptors/token.interceptor.ts`

Interceptor chỉ gửi:

- `Authorization: Bearer ...` nếu URL đúng API base URL.
- `withCredentials: true` cho API.

Request tới Cloudinary, Google Fonts hoặc host bên thứ ba không nhận token và không nhận cookie credentials từ interceptor này. Nếu giữ interceptor cũ, chỉ cần một request third-party đi qua `HttpClient` khi access token tồn tại là token có thể bị gửi nhầm ra ngoài boundary API.

### `TodoListFrontend/src/app/core/interceptors/error.interceptor.ts`

Khi refresh thất bại, interceptor gọi facade để chuyển state về `anonymous`, xóa token memory và kết thúc luồng request lỗi. Single-flight observable hiện vẫn dùng `shareReplay`/`finalize` để nhiều request 401 cùng chờ một lần refresh.

### `TodoListFrontend/src/app/app.routes.ts`

Layout và feature route đã chuyển sang `loadComponent`. Kết quả build sau thay đổi:

- Initial bundle: khoảng `271.94 kB` raw.
- Feature được tách thành lazy chunks như `todo-list`, `todo-tree-view`, `category-dashboard`, `profile`, `login`, `register`.

Trước đó initial bundle khoảng `520.75 kB` raw và vượt warning 500 kB. Lazy loading làm phần landing/auth ban đầu nhẹ hơn đáng kể.

### `TodoListFrontend/src/app/core/services/theme.service.ts` và `app.ts`

Đã sửa:

- Khôi phục cả `light`, `dark`, `system`, không biến `system` thành `light`.
- `system` dùng `matchMedia(...).matches` để chọn active mode.
- Dùng một callback cố định cho `addEventListener`/`removeEventListener`.
- `ThemeService` implement `OnDestroy` để tháo listener.
- App gọi `initTheme()` một lần thay vì tự quyết định chỉ dark/light.

Nếu giữ code cũ, lựa chọn system sẽ hiển thị light và listener không được tháo, gây sai giao diện hoặc giữ reference lâu hơn cần thiết.

### `category.service.ts`, `todo.service.ts`, `subtask.service.ts`

Event bus `Subject` đã được thay bằng signal:

- `refreshVersion` là counter bất biến theo hướng read-only.
- Todo có thêm `updatedTodo` và `publishUpdated()`.
- Component dùng `effect()`/`untracked()` để invalidate hoặc cập nhật cache.

Mục tiêu là tránh cho component bên ngoài tự gọi `.next()` vào Subject service. Nếu giữ Subject public, bất kỳ component nào cũng có thể phát event sai thứ tự hoặc giữ subscription sống lâu hơn cần thiết.

### `features/todo/todo-list/todo-list.ts`

Đã sửa:

- Search dùng `debounceTime` + `distinctUntilChanged` + `switchMap`; request search cũ bị hủy khi có từ khóa mới.
- Toggle Todo có `pendingTodoMutations` theo `id`, chặn double-click gửi đồng thời.
- Response thành công được publish qua signal để cập nhật item.
- 409 rollback trạng thái lạc quan, reload list và báo người dùng dữ liệu đã thay đổi.

Nếu giữ code cũ, hai click nhanh có thể gửi cùng version; một request trả 409 nhưng response còn lại có khả năng cập nhật UI theo thứ tự không đoán trước.

### `features/todo/todo-form-dialog/todo-form-dialog.ts`

Đã dùng non-nullable typed reactive form và tạo request create/update tường minh. Đã bỏ `as any` khi gửi form.

Nếu giữ `as any`, thay đổi DTO backend có thể không tạo lỗi compile ở FE và chỉ phát hiện khi request chạy ở production.

### `features/user/change-password/change-password.ts`

Đã dùng non-nullable form control và `getRawValue()` thay cho `value as any`.

### `features/auth/login/login.ts`, `features/auth/register/register.ts`, `home.ts`, `main-layout.ts`

Các luồng login/register/logout và thao tác tạo Todo trên landing dùng `AuthFacade`, đồng bộ với state trung tâm. Debug log lỗi login raw đã bỏ để tránh đưa chi tiết response vào console người dùng.

### `src/app/app.spec.ts`

Đã import rõ các hàm `describe`, `beforeAll`, `beforeEach`, `it`, `expect` từ Vitest. Test không còn phụ thuộc implicit globals khi chạy qua Angular CLI.

## 5. File không xóa

Không có file ứng dụng nào bị xóa trong Giai đoạn 4.

- Không xóa `AuthService`: interceptor và facade vẫn cần service này làm lớp HTTP/token memory.
- Không xóa các test unit hiện có: integration test bổ sung, không thay thế kiểm tra controller/validator/middleware.
- Không xóa các index cũ: chưa có `EXPLAIN ANALYZE` đủ để kết luận index nào thừa.
- Không sửa `ToDo.md`: đây là checklist nguồn do chủ dự án quản lý.

## 6. Kiểm tra đã thực hiện

### Backend

```powershell
dotnet build TodoListBackend.Tests/TodoListBackend.Tests.csproj --configuration Release --no-restore
dotnet test TodoListBackend.Tests/TodoListBackend.Tests.csproj --configuration Release --no-restore
```

Kết quả tại máy hiện tại:

- Build: pass, 0 error.
- Unit/middleware/validator: 11 passed.
- PostgreSQL ownership integration: 1 skipped vì Docker chưa cài/chưa chạy.
- Warning tồn tại: `SSH.NET 2025.1.0` có advisory mức high (`NU1903`); chưa tự nâng version vì cần kiểm tra compatibility của Cloudinary/provider trước.

### Frontend

```powershell
cd TodoListFrontend
.\node_modules\.bin\ng.cmd test --watch=false --no-progress
.\node_modules\.bin\ng.cmd build --configuration production
```

Kết quả:

- Frontend tests: 6 passed.
- Production build: pass.
- Initial bundle sau lazy-load: khoảng 271.94 kB raw.

Lệnh `npm` toàn cục trên máy đang trỏ tới npm CLI bị thiếu file; vì vậy dùng Angular CLI local trong `node_modules` là cách chạy đã xác minh được.

## 7. Cách chạy ownership test thật

Điều kiện:

- Cài và bật Docker Desktop.
- Không dùng connection string Aiven; Testcontainers tự tạo database tạm.

Chạy từ PowerShell tại root repository:

```powershell
$env:RUN_POSTGRES_INTEGRATION_TESTS = "1"
dotnet test TodoListBackend.Tests/TodoListBackend.Tests.csproj --configuration Release --filter FullyQualifiedName~OwnershipIntegrationTests
Remove-Item Env:RUN_POSTGRES_INTEGRATION_TESTS
```

Nếu Docker không chạy, test phải fail rõ ràng thay vì chuyển thành pass giả. Khi CI được cấu hình Docker service, biến môi trường này phải được bật ở job integration.

## 8. Cách áp dụng migration index lên Aiven sau khi xác nhận

Không chạy phần này tự động trong bước hiện tại.

### Bước 1 — kiểm tra migration hiện có

Trong Aiven Query Editor chọn đúng database `defaultdb`, schema `public`, chạy:

```sql
SELECT "MigrationId"
FROM "__EFMigrationsHistory"
ORDER BY "MigrationId" DESC
LIMIT 5;
```

Migration trước Stage 4 phải là migration DateOnly mới nhất của repository. Nếu chưa khớp, dừng lại và hoàn tất migration trước; không chạy SQL index rời rạc.

### Bước 2 — kiểm tra extension và index hiện tại

```sql
SELECT extname
FROM pg_extension
WHERE extname = 'pg_trgm';

SELECT indexname, indexdef
FROM pg_indexes
WHERE schemaname = 'public'
  AND tablename IN ('Todos', 'RefreshTokenSessions')
ORDER BY tablename, indexname;
```

### Bước 3 — tạo script SQL, không copy nhầm script toàn bộ từ migration đầu

Ở máy phát triển chạy script từ migration DateOnly tới migration Stage 4:

```powershell
dotnet ef migrations script `
  20260907134859_UseDateOnlyForTodoDueDate `
  20260908091644_AddStage4QueryIndexes `
  --project TodoListBackend/TodoListBackend.csproj `
  --startup-project TodoListBackend/TodoListBackend.csproj `
  --configuration Release `
  --output stage4-query-indexes.sql
```

Mở file SQL và xác nhận chỉ có:

- `CREATE EXTENSION IF NOT EXISTS pg_trgm`.
- Bốn composite index.
- Hai GIN trigram index.
- Insert migration history tương ứng.

Không đưa file SQL chứa connection string hoặc secret vào commit.

### Bước 4 — chạy trên Aiven theo cửa sổ bảo trì

1. Xác nhận Aiven automated backup đang có trạng thái OK.
2. Chọn đúng `defaultdb`/`public` trong Query Editor.
3. Chạy script một lần, không chạy từng đoạn tùy tiện.
4. Kiểm tra `__EFMigrationsHistory` đã có `20260908091644_AddStage4QueryIndexes`.
5. Kiểm tra bốn composite index và hai trigram index tồn tại.
6. Nếu `CREATE EXTENSION pg_trgm` bị từ chối quyền, dừng; không bỏ qua migration bằng cách tự chèn migration history. Khi đó phải xác nhận extension Aiven hỗ trợ hoặc đổi thiết kế index.

### Bước 5 — kiểm tra query plan

Chỉ chạy `EXPLAIN (ANALYZE, BUFFERS)` trên truy vấn đại diện sau khi đã thay placeholder bằng dữ liệu test an toàn. Không đưa JWT, email thật hoặc connection string vào log.

Mục tiêu kiểm tra:

- Todo list dùng composite index phù hợp.
- Search có thể dùng trigram index với pattern đủ chọn lọc.
- `COUNT` và page query không trả quá 100 Todo.

## 9. Nếu giữ nguyên toàn bộ trước Stage 4 thì rủi ro còn lại

- Search tiếp tục dùng biểu thức hạ chữ trên cột và dễ full scan.
- List theo category/status/ngày chỉ dựa vào index rời rạc, kém hiệu quả hơn khi một user có nhiều Todo.
- Không có kiểm thử route-to-PostgreSQL cho ownership, nên regression authorization có thể lọt qua unit test.
- Token interceptor có thể gửi credentials/token sang request không thuộc API.
- `system` theme hoạt động sai và listener không có lifecycle cleanup.
- Lazy loading chưa tách feature, initial bundle vượt budget cảnh báo.
- Subject public và optimistic mutation không chặn request trùng, dễ gây 409 hoặc UI rollback sai.
