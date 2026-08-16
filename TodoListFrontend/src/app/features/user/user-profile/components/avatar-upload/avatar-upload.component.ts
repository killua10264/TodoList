import { Component, input, output, inject } from '@angular/core';
import { CommonModule } from '@angular/common';
import { LanguageService } from '../../../../../core/services/language.service';

@Component({
  selector: 'app-avatar-upload',
  imports: [CommonModule],
  templateUrl: './avatar-upload.component.html',
  styleUrl: './avatar-upload.component.css'
})
export class AvatarUploadComponent {
  langService = inject(LanguageService);
  avatarUrl = input<string | null>(null);
  avatarInitials = input<string>('U');

  onFileSelected = output<Event>();
  onUseDefault = output<void>();

  handleFileSelected(event: Event) {
    this.onFileSelected.emit(event);
  }

  handleUseDefault() {
    this.onUseDefault.emit();
  }
}
