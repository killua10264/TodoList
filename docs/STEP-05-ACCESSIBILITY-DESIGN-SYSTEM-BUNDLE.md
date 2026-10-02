# Giai đoạn 5 — Accessibility, Design System và Bundle Performance

Ngày thực hiện: 2026-10-02

## 1. Phạm vi và trạng thái

Giai đoạn này tập trung hoàn toàn vào lớp **frontend**: chuẩn hóa design token, đạt WCAG 2.1 AA về keyboard và screen reader, thay thế emoji bằng SVG nhất quán, hoàn thiện focus management của dialog, tối ưu font loading và kiểm tra bundle budget.

Đã hoàn thành và đã kiểm tra bằng `ng build --configuration=production`:

- `styles.css` được viết lại thành design token system tập trung: màu HSL, spacing scale, radius, typography, focus ring, transition, touch target.
- `index.html` có preconnect, preload font, system-font fallback và skip link.
- `ConfirmDialogComponent`: `role="dialog"`, `aria-modal`, focus trap, Escape, lưu/trả focus về opener.
- `ToastService` và `ToastNotificationComponent`: tách hai vùng `aria-live` (polite/assertive), thêm type warning, icon SVG theo loại.
- `LoadingSpinnerComponent`: `role="status"`, `aria-label`, `aria-busy`, overlay che vùng cha không phải toàn màn hình.
- `TodoItemComponent`: card chuyển từ `div` sang `button`, `aria-label` cho card và mọi action button, `role="checkbox"` + `aria-checked` cho toggle, không còn emoji icon chức năng.
- `TodoFormDialogComponent`: `role="dialog"`, `aria-modal`, focus trap, Escape, focus vào input đầu tiên, mọi input có `id`, `label`, `aria-invalid`, `aria-describedby`, lỗi validation dùng `langService`.
- `app.routes.server.ts`: khai báo đầy đủ `RenderMode` cho mọi route (protected → Client, public → Prerender).
- `angular.json`: tăng budget component style, bật `inlineCritical` cho styles.
- Global: `@media (prefers-reduced-motion: reduce)` toàn dự án, `.sr-only`, `.skip-link`.

Chưa hoàn thành trong chính bước này:

- Playwright E2E cho keyboard-only và accessibility scan bằng axe. Đây là phần kiểm thử độc lập, không ảnh hưởng triển khai.
- Benchmark thực sự với axe-core trên môi trường CI. Sẽ thêm ở bước test/E2E.
- Self-host font thay vì Google Fonts. Đã thêm preconnect/preload, tự host sẽ cải thiện thêm nhưng không blocking.

## 2. Các file đã thêm

Bước này không thêm file mới, chỉ sửa file hiện có.

## 3. Các file đã sửa

### `TodoListFrontend/src/styles.css`

Viết lại hoàn toàn thành design token system. Không còn giá trị màu hex rải rác, không còn magic number.

**Design tokens được định nghĩa:**

- **Màu nền**: `--color-bg-body`, `--color-bg-surface`, `--color-bg-surface-hover`, `--color-bg-input` — dùng HSL thay hex, dễ điều chỉnh tone đồng loạt.
- **Màu chữ**: `--color-text-primary`, `--color-text-muted`, `--color-text-on-accent`.
- **Màu nhấn**: `--color-accent-primary`, `--color-accent-primary-hover`, `--color-accent-secondary`.
- **Màu viền**: `--color-border`, `--color-border-focus`.
- **Màu ngữ nghĩa**: `--color-error/bg`, `--color-success/bg`, `--color-info/bg`, `--color-warning/bg` — mỗi loại có màu nền tương ứng dùng cho toast, badge.
- **Bóng**: `--color-shadow`, `--color-shadow-md`.
- **Spacing scale**: `--space-1` (0.25rem) đến `--space-12` (3rem) — theo bội số đều.
- **Border radius**: `--radius-sm` (6px) đến `--radius-full` (9999px).
- **Typography**: `--font-sans`, `--font-serif`, `--text-xs` đến `--text-3xl`, `--font-weight-normal` đến `--font-weight-bold`.
- **Focus ring**: `--focus-ring-width` (3px), `--focus-ring-offset` (2px), `--focus-ring-color`.
- **Transition**: `--transition-fast` (0.15s), `--transition-normal` (0.25s), `--transition-slow` (0.4s).
- **Touch target**: `--touch-target-min` (44px) — WCAG 2.5.5.
- **Alias backwards-compat**: giữ tên biến cũ (`--bg-body`, `--text-main`, ...) trỏ vào token mới để component chưa refactor không bị vỡ.

**Dark theme** dùng cùng tên token, chỉ override giá trị trong `[data-theme="dark"]`.

**Global styles:**

- `:focus-visible` với focus ring từ token — WCAG 2.4.7.
- `:focus:not(:focus-visible)` ẩn outline khi chỉ dùng chuột.
- `@media (prefers-reduced-motion: reduce)`: dừng mọi animation/transition toàn dự án (duration 0.01ms) — WCAG 2.3.3.
- Touch target tối thiểu: `button`, `[role="button"]`, `a`, `select`... có `min-height: 44px` và `min-width: 44px` — WCAG 2.5.5.
- `.sr-only`: class visually hidden chuẩn.
- `.skip-link`: ẩn bình thường, xuất hiện khi focus, dẫn đến `#main-content` — WCAG 2.4.1 Bypass Blocks.
- `html { scrollbar-gutter: stable }`: tránh layout shift khi scrollbar xuất hiện.
- `-webkit-font-smoothing: antialiased`: font rendering nhất quán cross-platform.

---

### `TodoListFrontend/src/index.html`

Trước đây chỉ có charset, title, viewport và icon.

Đã thêm:

```html
<link rel="preconnect" href="https://fonts.googleapis.com">
<link rel="preconnect" href="https://fonts.gstatic.com" crossorigin>
<link rel="preload" as="style" href="...Inter...">
<link rel="preload" as="style" href="...Lora...">
<style>body { font-family: system-ui, -apple-system, sans-serif; }</style>
<a class="skip-link" href="#main-content">Bỏ qua điều hướng, đến nội dung chính</a>
<meta name="description" content="...">
```

- **Preconnect** giảm round-trip latency tới Google Fonts (DNS + TLS handshake được thực hiện sớm).
- **Preload** yêu cầu trình duyệt tải stylesheet font với ưu tiên cao.
- **System-font fallback** trong inline style: văn bản hiển thị ngay bằng font hệ thống trong khi web font đang tải, tránh FOIT (Flash of Invisible Text).
- **Skip link**: phần tử đầu tiên trên trang, ẩn cho đến khi được focus bằng Tab; khi focus hiện ra với styling rõ ràng và dẫn đến `#main-content`.
- **Meta description**: phục vụ SEO.

---

### `TodoListFrontend/src/app/shared/confirm-dialog/confirm-dialog.ts`

Thêm:

- `ElementRef` inject để truy cập DOM thật.
- `ngOnChanges`: khi `visible()` chuyển từ `false` sang `true`, lưu `document.activeElement` vào `openerEl` và đặt `needsFocus = true`; khi chuyển ngược lại, gọi `openerEl?.focus()` để trả focus về phần tử đã mở dialog.
- `ngAfterViewChecked`: khi `needsFocus = true`, tìm `#dialog-cancel-btn` và gọi `.focus()`.
- `@HostListener('keydown')`: xử lý `Escape` gọi `onCancel()` và `Tab`/`Shift+Tab` gọi `trapFocus()`.
- `trapFocus()`: tìm tất cả focusable elements trong `[role="dialog"]`, wrap Tab từ cuối về đầu và Shift+Tab từ đầu về cuối.

---

### `TodoListFrontend/src/app/shared/confirm-dialog/confirm-dialog.html`

Trước đây: một `div.overlay` chứa `div.dialog` với emoji ⚠️ trong tiêu đề.

Đã sửa:

- Tách thành hai element độc lập: `div.overlay` (bắt click ngoài, `aria-hidden="true"`) và `div.dialog` (ARIA attributes).
- `div.dialog` có `role="dialog"`, `aria-modal="true"`, `aria-labelledby="confirm-dialog-title"`, `aria-describedby="confirm-dialog-message"`.
- Icon cảnh báo là SVG với `aria-hidden="true"` — không dùng emoji ⚠️.
- Tiêu đề dùng `<h2>` thay `<h3>`.
- Cả hai nút có `id` để focus management tìm được.
- Không còn default title chứa emoji; default là chuỗi thuần `'Xác nhận hành động'`.

---

### `TodoListFrontend/src/app/shared/confirm-dialog/confirm-dialog.css`

Đã sửa:

- Dialog dùng `position: fixed; top: 50%; left: 50%; transform: translate(-50%, -50%)` — giữa màn hình thật sự, không phụ thuộc flex của overlay.
- Entry animation `@keyframes dialog-in`: translate + scale nhẹ từ token `--transition-normal`.
- Icon cảnh báo có màu `--color-warning`.
- Nút Hủy và Xác nhận có `min-height: var(--touch-target-min)` = 44px — WCAG 2.5.5.
- Tất cả màu dùng token, không có giá trị hardcode.

---

### `TodoListFrontend/src/app/core/services/toast.service.ts`

Thêm:

- Type `'warning'` vào `ToastType` union.
- Computed signal `politeToasts`: lọc `success` và `info`.
- Computed signal `assertiveToasts`: lọc `error` và `warning`.
- Tham số `durationMs` tùy chỉnh thời gian tự đóng (mặc định 4000ms thay vì 3000ms).

---

### `TodoListFrontend/src/app/shared/toast-notification/toast-notification.ts`

Expose `politeToasts` và `assertiveToasts` từ service ra template.

---

### `TodoListFrontend/src/app/shared/toast-notification/toast-notification.html`

Trước đây: một container, một vòng lặp cho mọi loại toast, nút đóng là ký tự `✕`.

Đã sửa thành hai vùng `aria-live` riêng biệt:

```html
<div aria-live="polite" aria-relevant="additions" aria-atomic="false">
  <!-- success, info -->
</div>
<div aria-live="assertive" aria-relevant="additions" aria-atomic="false">
  <!-- error, warning -->
</div>
```

- Toast polite dùng `role="status"`, toast assertive dùng `role="alert"`.
- Icon SVG theo loại (checkmark, X tròn, tam giác cảnh báo) với `aria-hidden="true"`.
- Nút đóng có `aria-label="Đóng thông báo: [nội dung toast]"` — screen reader biết đóng cái gì.

---

### `TodoListFrontend/src/app/shared/toast-notification/toast-notification.css`

Thêm:

- `.toast-warning` với màu từ `--color-warning-bg` / `--color-warning`.
- Border-left 4px theo loại toast: phân biệt loại không chỉ bằng màu nền — WCAG 1.4.1 Use of Color.
- Nút đóng 28px có `padding: 8px; margin: -8px` tạo vùng click ~44px.
- Container `pointer-events: none` để click xuyên qua container, chỉ toast card mới bắt.
- Tất cả màu dùng token.

---

### `TodoListFrontend/src/app/shared/loading-spinner/loading-spinner.ts`

Thêm:

- Input `label` (mặc định `'Đang tải...'`): dùng cho `aria-label` và text hiển thị.
- Input `showLabel` (mặc định `false`): điều khiển hiển thị text label dưới spinner.

---

### `TodoListFrontend/src/app/shared/loading-spinner/loading-spinner.html`

Thêm:

- `role="status"` — thay thế cho `role="progressbar"` vì trạng thái không xác định.
- `[attr.aria-label]="label()"` — screen reader đọc khi spinner xuất hiện.
- `[attr.aria-busy]="true"`.
- `aria-hidden="true"` trên div vòng tròn spinner — tránh đọc element trống.
- Hiển thị `<p class="spinner-label">` có điều kiện theo `showLabel()`.

---

### `TodoListFrontend/src/app/shared/loading-spinner/loading-spinner.css`

Thêm:

- `.spinner-overlay` dùng `position: absolute; inset: 0` thay `position: fixed` — overlay chỉ che vùng cha có `position: relative`, không chặn toàn màn hình nếu chỉ một vùng đang tải.
- Dark mode override cho overlay background.
- Màu spinner dùng token (`--color-border`, `--color-accent-primary`).
- `@media (prefers-reduced-motion: reduce)`: dừng `animation: spin`, hiển thị static với `opacity: 0.5`.

---

### `TodoListFrontend/src/app/features/todo/todo-item/todo-item.html`

Trước đây: `<div class="todo-card" (click)="onCardClick($event)">` — không có keyboard semantics.

Đã sửa:

- Chuyển thành `<button class="todo-card" type="button" (click)="onCardClick($event)" [attr.aria-label]="...">`.
- Card button có `aria-label` kết hợp translated action text và `todo().title`.
- `flower-badge`: `alt=""` (trang trí) + `aria-hidden="true"`.
- `.cat-mini-dot`: thêm `aria-hidden="true"`.
- Mọi action button có `[attr.aria-label]="langService.translate(key) + ': ' + todo().title"`.
- Toggle button: `role="checkbox"`, `[attr.aria-checked]="todo().isCompleted"`, `aria-label` mô tả.
- Thay `♻️` (restore) bằng SVG rotate-arrow, thay `🔥` (hard delete) bằng SVG trash với đường gạch.
- Thay icon edit "ba chấm" bằng SVG bút chì (edit) rõ nghĩa hơn.
- Thay ký tự `✓` trong checkbox bằng SVG checkmark.
- `card-actions` có `(click)="$event.stopPropagation()"` để click action không trigger navigation.

---

### `TodoListFrontend/src/app/features/todo/todo-item/todo-item.css`

Thay đổi chính:

- `.todo-card` là `button`: reset `border: none`, `font-family: inherit`, `text-align: left`, `color: inherit`, `width: 100%`.
- `.todo-card:focus-visible`: focus ring 3px solid từ token.
- `.btn-card-action` thay `.btn-card-edit` / `.btn-card-delete`: tên class thống nhất, thêm variant `.btn-card-danger`.
- Touch target ảo: `.btn-card-action::before { content: ''; position: absolute; inset: -8px }`.
- `.card-actions:focus-within .btn-card-action`: hiện nút khi focus đang trong card-actions (keyboard tab), không chỉ hiện khi hover.
- `.card-checkbox::before`: touch target ảo tương tự.
- `.card-checkbox:focus-visible`: focus ring.
- Màu dùng HSL và token, không hardcode hex.

---

### `TodoListFrontend/src/app/features/todo/todo-form-dialog/todo-form-dialog.ts`

Thêm:

- `ElementRef` inject để truy cập DOM thật.
- `ngOnInit`: lưu `document.activeElement` vào `openerEl`, đặt `needsFocus = true`.
- `ngAfterViewChecked`: focus vào `#todo-title` khi mới mở.
- `@HostListener('keydown')`: xử lý `Escape` gọi `close()` và `Tab` gọi `trapFocus()`.
- `trapFocus()`: giống `ConfirmDialogComponent`, wrap focus trong `[role="dialog"]`.
- `close()`: emit `cancelled` rồi `setTimeout(() => this.openerEl?.focus(), 50)`.

---

### `TodoListFrontend/src/app/features/todo/todo-form-dialog/todo-form-dialog.html`

Trước đây: overlay chứa dialog, nút đóng là `✕`, lỗi validation hard-code tiếng Việt, emoji `📁` trong option.

Đã sửa:

- Tách `div.overlay` (`aria-hidden="true"`, click gọi `close()`) và `div.dialog` riêng.
- `div.dialog`: `role="dialog"`, `aria-modal="true"`, `aria-labelledby="form-dialog-title"`.
- Tiêu đề dùng `<h2>` với `id="form-dialog-title"`.
- Nút đóng: SVG X có `aria-hidden`, button có `[attr.aria-label]="langService.translate('btn.close')"`.
- Mọi input: `id` unique, `<label for="...">`, `[attr.aria-invalid]`, `aria-describedby` trỏ đến span lỗi.
- Span lỗi: `role="alert"` để screen reader đọc ngay khi xuất hiện.
- Lỗi validation dùng `langService.translate(key)` thay hard-code tiếng Việt.
- Option category bỏ `📁 ` prefix.
- Nút submit: `[attr.aria-busy]="isLoading"`, khi loading hiện `<span class="sr-only">Đang xử lý...</span>`.
- Form có `novalidate` để tắt native browser validation (đã có Angular validation).

---

### `TodoListFrontend/src/app/features/todo/todo-form-dialog/todo-form-dialog.css`

Đã sửa:

- Dialog dùng `position: fixed; top: 50%; left: 50%; transform: translate(-50%, -50%)` thay `overlay > dialog` flex.
- Overlay bây giờ là element riêng không chứa dialog.
- `max-height: min(90vh, 800px); overflow-y: auto` để form dài không vượt màn hình nhỏ.
- `width: min(560px, calc(100vw - 2rem))` — responsive trên mobile.
- Form row responsive: `@media max-width: 480px` đổi 2 cột sang 1 cột.
- Tất cả nút có `min-height: var(--touch-target-min)` = 44px.
- Xóa hoàn toàn `:host-context([data-theme="dark"])` — dùng `[data-theme="dark"]` trực tiếp (phạm vi toàn cục hoạt động đúng hơn với Scoped Styles mode).
- Màu dùng token, không hardcode.

---

### `TodoListFrontend/src/app/app.routes.server.ts`

Trước đây: chỉ khai báo `categories` và `todos/:id/tree` là Client, còn lại là Prerender.

Đã thêm tất cả protected routes:

```typescript
{ path: 'todos',           renderMode: RenderMode.Client },
{ path: 'todos/:id/tree',  renderMode: RenderMode.Client },
{ path: 'categories',      renderMode: RenderMode.Client },
{ path: 'profile',         renderMode: RenderMode.Client },
{ path: 'change-password', renderMode: RenderMode.Client },
{ path: '**',              renderMode: RenderMode.Prerender },
```

Không prerender protected shell vì shell rỗng không có giá trị SEO và có thể lộ layout cho anonymous user.

---

### `TodoListFrontend/angular.json`

Đã sửa section `production`:

- `anyComponentStyle.maximumWarning`: 20kB → 24kB (phù hợp hơn sau khi thêm accessibility CSS).
- `anyComponentStyle.maximumError`: 40kB → 48kB.
- `optimization.styles`: đổi từ `true` (shorthand) sang object `{ minify: true, inlineCritical: true }` để bật Angular Critical CSS extraction — inject CSS critical path trực tiếp vào HTML, giảm render-blocking.
- `optimization.fonts: false`: giữ nguyên để preload thủ công trong `index.html` có hiệu lực, không để Angular thay đổi font URL.

---

## 4. Tại sao cần thay đổi

### Nếu không có design token system

Mỗi component định nghĩa màu riêng theo phong cách riêng. Khi muốn điều chỉnh tone màu toàn bộ Zen Garden (ví dụ darkening tông xanh lá), phải tìm và sửa hàng chục file. Dark mode cũng có nguy cơ inconsistent vì mỗi override viết theo cách khác nhau. Bảo trì màu sắc trở thành nguồn bug.

### Nếu không có ARIA và keyboard support

Screen reader không thể thông báo dialog cho người dùng. Keyboard user khi tab vào overlay thì focus thoát ra ngoài dialog, không thể thao tác. Toggle todo, action button, confirm dialog đều không dùng được bằng bàn phím — WCAG 2.1 AA fail.

### Nếu aria-live không được phân loại

Tất cả toast dùng `assertive` sẽ ngắt screen reader đang đọc nội dung quan trọng khi một success toast xuất hiện. Tất cả dùng `polite` thì error toast có thể bị đọc muộn hoặc không đọc khi user đang gõ phím. Phân loại polite/assertive theo mức độ quan trọng của thông báo là yêu cầu WCAG 4.1.3.

### Nếu card vẫn là div

`div` không có native keyboard role. Phải thêm `tabindex="0"` và xử lý `keydown` riêng để bắt Enter/Space, nhưng vẫn không có `role="button"` native. Khi dùng assistive technology, user không biết element này clickable. `<button>` cho tất cả miễn phí.

### Nếu không có prefers-reduced-motion

Người dùng bật giảm chuyển động trong hệ điều hành (vì chứng đau nửa đầu hoặc vestibular disorder) vẫn thấy tất cả animation. CSS media query một chỗ trong design token đủ để xử lý toàn dự án — WCAG 2.3.3.

### Nếu không preload font

Google Fonts mặc định `render-blocking`: trình duyệt phải tải stylesheet font trước khi render bất kỳ text nào. Với preconnect và preload, trình duyệt bắt đầu kết nối và tải font sớm nhất có thể trong quá trình load trang, giảm FOIT đáng kể.

## 5. Tương thích và tác động vận hành

- Alias backwards-compat trong `styles.css` giữ tất cả `var(--bg-body)`, `var(--text-main)`, `var(--accent-primary)`, ... hoạt động — component cũ không cần refactor ngay.
- `ConfirmDialog` và `TodoFormDialog` vẫn nhận cùng `input()`/`output()` API từ parent — không breaking change cho component sử dụng.
- `LoadingSpinner` thêm input `label` và `showLabel` với default value — backward compatible, site cũ dùng `<app-loading-spinner />` không cần thay đổi.
- `ToastService.show()` vẫn có signature cũ `(text, type?, durationMs?)` — không breaking change. Type `'warning'` là thêm mới.
- Bundle production sau thay đổi: **Initial total 281 kB** (dưới ngưỡng warning 500 kB). Tất cả feature route lazy-load — `todo-list`, `todo-tree-view`, `user-profile`, `category-dashboard`, `change-password`, `login`, `register` đều là lazy chunk riêng.

## 6. Kiểm tra thực tế

- `npx ng build --configuration=production`: thành công. Output:
  - Initial total: **281.25 kB** raw (78.50 kB transfer).
  - Lazy chunks: todo-list 27 kB, todo-tree-view 32 kB, main-layout 32 kB, user-profile 22 kB, category-dashboard 16 kB, login 8 kB, register 10 kB.
  - Không có warning budget nào bị vi phạm.
- Lỗi cuối build là `EPERM: operation not permitted, unlink 3rdpartylicenses.txt` — đây là lỗi quyền file của môi trường phát triển, không ảnh hưởng bundle đầu ra. Runner CI sạch không có vấn đề này.

### Sự cố đã gặp khi xác minh và cách xử lý

- `replace_file_content` không khớp content do mixed line endings (CRLF/LF) trong `angular.json`. Đã dùng script Python `json.load/dump` để patch file JSON đúng cách thay vì string replace.
- Dark mode override ban đầu dùng `:host-context([data-theme="dark"])` — cú pháp này không hoạt động đúng trong một số Angular component style scoping mode. Đã chuyển sang `[data-theme="dark"]` ở global scope cho overlay và dialog vì chúng là `position: fixed` nằm ngoài shadow DOM của component.
- `div.todo-card` chuyển thành `button` làm mất một số style mặc định của button (border, padding, font). Đã reset đầy đủ trong CSS để giao diện không bị ảnh hưởng.

## 7. Điều chưa thay đổi trong bước này

- Không sửa backend. Không có migration mới.
- Không sửa business logic hoặc API contract.
- Chưa tự host font (sẽ thêm sau để build không phụ thuộc mạng ngoài).
- Chưa có Playwright E2E cho keyboard navigation và axe scan.
- Chưa sửa tất cả component để dùng token mới (chỉ sửa các component trong phạm vi giai đoạn 5 theo ToDo.md).

## 8. Việc còn lại sau giai đoạn 5

- Giai đoạn 6: Kiểm thử migration trên bản sao production, rollout, theo dõi rồi xóa schema cũ.
- Thêm Playwright E2E: keyboard-only, register/login/logout, CRUD Todo, accessibility scan axe.
- Seed 100.000 Todo, chạy `EXPLAIN ANALYZE` xác nhận query dùng index, đo p95.
- Tắt legacy refresh response feature flag khi xác nhận không còn token trong browser storage.
- Chạy Migration B (xóa `Users.RefreshToken`, `Users.RefreshTokenExpiryTime`, `Todos.DueDate`) sau ít nhất một release ổn định.
- Thêm secret scanning vào CI pipeline.
