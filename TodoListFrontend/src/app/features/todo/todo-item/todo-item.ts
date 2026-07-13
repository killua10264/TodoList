import { Component, input, output, inject } from '@angular/core';
import { Router } from '@angular/router';
import { TodoResponse } from '../../../core/models/todo.model';

@Component({
  selector: 'app-todo-item',
  imports: [],
  templateUrl: './todo-item.html',
  styleUrl: './todo-item.css'
})
export class TodoItemComponent {
  private router = inject(Router);
  todo = input.required<TodoResponse>();

  toggled = output<TodoResponse>();
  edited = output<TodoResponse>();
  deleted = output<number>();

  onToggle(event: Event) {
    event.stopPropagation();
    this.toggled.emit(this.todo());
  }

  onEdit(event: Event) {
    event.stopPropagation();
    this.edited.emit(this.todo());
  }

  onCardClick(event: Event) {
    this.router.navigate(['/todos', this.todo().id, 'tree']);
  }

  onDelete(event: Event) {
    event.stopPropagation();
    this.deleted.emit(this.todo().id);
  }
}
