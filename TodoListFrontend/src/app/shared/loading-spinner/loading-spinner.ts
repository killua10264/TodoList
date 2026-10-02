import { Component, input } from '@angular/core';

@Component({
  selector: 'app-loading-spinner',
  templateUrl: './loading-spinner.html',
  styleUrl: './loading-spinner.css',
})
export class LoadingSpinnerComponent {
  /** Hiển thị overlay toàn vùng cha (không phải toàn màn hình) */
  overlay = input<boolean>(false);

  /** Nhãn cho screen reader và hiển thị dưới spinner */
  label = input<string>('Đang tải...');

  /** Hiện text label dưới spinner hay không */
  showLabel = input<boolean>(false);
}
