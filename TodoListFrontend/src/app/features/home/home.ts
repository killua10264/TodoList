import { Component, inject, signal } from '@angular/core';
import { Router } from '@angular/router';
import { ThemeService } from '../../core/services/theme.service';
import { AuthService } from '../../core/services/auth.service';
import { TodoService } from '../../core/services/todo.service';
import { ToastService } from '../../core/services/toast.service';

interface TemplateCard {
  icon: string;
  title: string;
  description: string;
}

@Component({
    selector: 'app-home',
    templateUrl: './home.html',
    styleUrl: './home.css'
})
export class HomeComponent {
  themeService = inject(ThemeService);
  private router = inject(Router);
  private authService = inject(AuthService);
  private todoService = inject(TodoService);
  private toast = inject(ToastService);

  activeTab = signal<string>('Công việc');

  templateTabs = ['Công việc', 'Cá nhân', 'Học tập', 'Khác'];

  templateCards: Record<string, TemplateCard[]> = {
    'Công việc': [
      { icon: '💰', title: 'Sổ Sách Gọn Gàng', description: 'Hệ thống hóa đơn, chứng từ và thu chi ngăn nắp, chính xác.' },
      { icon: '🤝', title: 'Quản Lý Đối Tác', description: 'Sắp xếp và theo dõi tiến độ công việc với khách hàng từ A đến Z.' },
      { icon: '📋', title: 'Sprint Hiệu Quả', description: 'Lên kế hoạch sprint hàng tuần, theo dõi tiến độ và ước lượng công sức.' },
      { icon: '📧', title: 'Hộp Thư Gọn Gàng', description: 'Phân loại và xử lý email theo mức độ ưu tiên, không bỏ sót việc quan trọng.' }
    ],
    'Cá nhân': [
      { icon: '🧳', title: 'Sửa Soạn Hành Trình', description: 'Danh mục chuẩn bị hành lý chu đáo, không lo quên đồ đạc quan trọng.' },
      { icon: '🏡', title: 'Việc Nhà Gọn Gàng', description: 'Lên lịch dọn dẹp, sửa chữa và bảo trì nhà cửa đều đặn.' },
      { icon: '🎯', title: 'Mục Tiêu Năm Mới', description: 'Theo dõi các mục tiêu cá nhân và chia nhỏ thành bước thực hiện cụ thể.' },
      { icon: '💪', title: 'Thói Quen Lành Mạnh', description: 'Xây dựng và duy trì thói quen tốt mỗi ngày với tracker đơn giản.' }
    ],
    'Học tập': [
      { icon: '📚', title: 'Lịch Ôn Thi', description: 'Phân bổ thời gian ôn thi hợp lý, không dồn bài phút cuối.' },
      { icon: '🔬', title: 'Dự Án Nghiên Cứu', description: 'Quản lý các giai đoạn nghiên cứu từ đề xuất đến báo cáo kết quả.' },
      { icon: '📖', title: 'Kế Hoạch Đọc Sách', description: 'Theo dõi sách đang đọc, ghi chú và đặt mục tiêu đọc sách hàng tháng.' },
      { icon: '🎓', title: 'Lộ Trình Kỹ Năng', description: 'Xây dựng lộ trình học kỹ năng mới theo từng cấp độ từ cơ bản đến nâng cao.' }
    ],
    'Khác': [
      { icon: '🧘', title: 'Tập Trung Sâu', description: 'Loại bỏ xao nhãng để tối ưu hiệu suất làm việc trong không gian tĩnh lặng.' },
      { icon: '🌿', title: 'Chăm Sóc Bản Thân', description: 'Lên kế hoạch nghỉ ngơi, thiền định và chăm sóc sức khỏe tinh thần.' },
      { icon: '🎨', title: 'Dự Án Sáng Tạo', description: 'Quản lý ý tưởng sáng tạo và theo dõi tiến độ dự án cá nhân.' },
      { icon: '🌍', title: 'Sự Kiện & Hẹn', description: 'Ghi nhớ và sắp xếp các cuộc hẹn, sự kiện quan trọng không bị trùng lịch.' }
    ]
  };

  get currentTemplateCards(): TemplateCard[] {
    return this.templateCards[this.activeTab()] || [];
  }

  setActiveTab(tab: string) {
    this.activeTab.set(tab);
  }

  onHeroInputKeydown(event: KeyboardEvent) {
    if (event.key !== 'Enter') return;

    const input = event.target as HTMLInputElement;
    const title = input.value.trim();

    if (!this.authService.isLoggedIn()) {
      this.router.navigate(['/register']);
      return;
    }

    if (!title) return;

    const now = new Date();
    const today = `${now.getFullYear()}-${String(now.getMonth() + 1).padStart(2, '0')}-${String(now.getDate()).padStart(2, '0')}`;
    this.todoService.create({
      title,
      description: '',
      priority: 2,
      dueDate: today,
      categoryId: 3
    }).subscribe({
      next: () => {
        this.toast.show('Đã gieo hạt giống thành công! 🌱', 'success');
        input.value = '';
        this.router.navigate(['/todos']);
      },
      error: () => {
        this.toast.show('Không thể tạo công việc. Vui lòng thử lại!', 'error');
      }
    });
  }
}
