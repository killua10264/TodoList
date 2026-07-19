import { Component, input, output, signal, HostListener } from '@angular/core';
import { RouterLink } from '@angular/router';
import { CommonModule } from '@angular/common';

@Component({
  selector: 'app-main-header',
  imports: [RouterLink, CommonModule],
  templateUrl: './main-header.component.html',
  styleUrl: './main-header.component.css'
})
export class MainHeaderComponent {
  pageTitle = input.required<string>();
  headerAvatarUrl = input<string | null>(null);
  headerAvatarInitials = input<string>('AVT');

  onLogout = output<void>();

  showUserMenu = signal<boolean>(false);

  toggleUserMenu(event: Event) {
    event.stopPropagation();
    this.showUserMenu.set(!this.showUserMenu());
  }

  closeUserMenu() {
    this.showUserMenu.set(false);
  }

  @HostListener('document:click')
  onDocumentClick() {
    this.closeUserMenu();
  }

  handleLogout() {
    this.closeUserMenu();
    this.onLogout.emit();
  }
}
