# 📝 TodoList - Ứng dụng Quản lý Công việc Toàn diện

> Một ứng dụng quản lý công việc giúp bạn tổ chức công việc hàng ngày, tăng cường hiệu suất và không bao giờ bỏ lỡ deadline. Hỗ trợ quản lý công việc đa cấp độ.

## 🚀 Demo & Hình ảnh minh họa

**🔗 Trải nghiệm trực tiếp (Live Demo):** [Zen Garden](https://todo-list-git-main-killua7.vercel.app)

![Giao diện chính](https://res.cloudinary.com/dvsy4c7hu/image/upload/v1784528864/Demo_yllu1n.png)
_<center>Màn hình quản lý danh sách công việc</center>_

## ✨ Tính năng chính (Features)

- **Quản lý công việc:** Thêm mới, xem chi tiết, cập nhật và xóa công việc một cách dễ dàng.
- **Cấu trúc Subtasks:** Phân chia công việc lớn thành các công việc con để dễ dàng theo dõi và quản lý.
- **Trạng thái & Tiến độ:** Đánh dấu hoàn thành công việc nhanh chóng.
- **Phân loại & Lọc:** Lọc công việc theo trạng thái (Đã hoàn thành, Chưa hoàn thành), theo ngày hạn (Due Date).
- **Xác thực & Bảo mật:** Đăng nhập, đăng ký tài khoản an toàn.
- **Giao diện hiện đại (UI/UX):** Giao diện phong cách khu vườn.

## 🛠 Công nghệ sử dụng (Tech Stack)

### Frontend

- **Framework:** Angular
- **State Management & Data Flow:** RxJS
- **Styling:** CSS/SCSS (TailwindCSS)
- **Hosting/Deployment:** Vercel

### Backend

- **Framework:** ASP.NET Core Web API (C#)
- **Kiến trúc:** Clean Architecture & SOLID principles
- **Database ORM:** Entity Framework Core
- **Database:** SQL Server / PostgreSQL
- **Bảo mật:** JWT Authentication, Password Hashing, Rate Limiting bảo vệ API
- **Hosting/Deployment:** Render

## ⚙️ Hướng dẫn cài đặt (Installation)

Làm theo các bước dưới đây để clone và chạy dự án trên máy cục bộ của bạn.

### Yêu cầu hệ thống (Prerequisites)

- [Node.js](https://nodejs.org/) (phiên bản 18+ khuyến nghị)
- [Angular CLI](https://angular.io/cli)
- [.NET 8 SDK](https://dotnet.microsoft.com/download)
- Hệ quản trị CSDL (VD: SQL Server)

### 1. Cài đặt Backend

```bash
# Clone repository
git clone https://github.com/killua10264/TodoList.git

# Di chuyển vào thư mục Backend
cd TodoList/TodoListBackend

# Khôi phục các packages
dotnet restore

# Cập nhật Database (Chạy migrations)
# Lưu ý: Đảm bảo bạn đã cấu hình chuỗi kết nối 'DefaultConnection' trong file appsettings.Development.json
dotnet ef database update

# Chạy server Backend
dotnet run
```

### 2. Cài đặt Frontend

```bash
# Mở một terminal/command prompt mới và di chuyển vào thư mục Frontend
cd TodoList/TodoListFrontend

# Cài đặt các thư viện phụ thuộc
npm install

# Chạy server Frontend
npm run start
```

### Tác giả & Thông tin liên hệ

```
Trần Quang Trường
GitHub: github.com/killua10264
LinkedIn:
Email: tqtruong0210@gmail.com
Portfolio:
```
