import { Component, input, output, signal } from '@angular/core';
import { SubTaskResponse } from '../../../../../core/models/subtask.model';
import { FormsModule } from '@angular/forms';
import { CommonModule } from '@angular/common';

@Component({
  selector: 'app-todo-tree-leaf',
  imports: [CommonModule, FormsModule],
  templateUrl: './todo-tree-leaf.component.html',
  styleUrl: './todo-tree-leaf.component.css'
})
export class TodoTreeLeafComponent {
  sub = input.required<SubTaskResponse>();
  index = input.required<number>();

  onToggle = output<SubTaskResponse>();
  onUpdateTitle = output<{ sub: SubTaskResponse, newTitle: string }>();
  onDeleteRequest = output<SubTaskResponse>();

  isEditing = signal<boolean>(false);
  editTitle = signal<string>('');

  onToggleLeaf() {
    this.onToggle.emit(this.sub());
  }

  startEdit() {
    this.isEditing.set(true);
    this.editTitle.set(this.sub().title);
  }

  saveEdit() {
    const newTitle = this.editTitle().trim();
    if (!newTitle || newTitle === this.sub().title) {
      this.isEditing.set(false);
      return;
    }
    
    this.onUpdateTitle.emit({ sub: this.sub(), newTitle });
    this.isEditing.set(false);
  }

  cancelEdit() {
    this.isEditing.set(false);
  }

  onDelete() {
    this.onDeleteRequest.emit(this.sub());
  }

  getRotation(): string {
    const rot = [ '-3deg', '2.5deg', '-1.5deg', '3.5deg', '-2.5deg' ];
    return `rotate(${rot[this.index() % 5]})`;
  }
}
