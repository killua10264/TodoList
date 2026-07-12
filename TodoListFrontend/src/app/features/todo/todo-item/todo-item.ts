import { Component, input, output } from '@angular/core';
import { TodoResponse } from '../../../core/models/todo.model';

@Component({
  selector: 'app-todo-item',
  imports: [],
  templateUrl: './todo-item.html',
  styleUrl: './todo-item.css'
})
export class TodoItemComponent {
  todo = input.required<TodoResponse>();

  toggled = output<TodoResponse>();
  edited = output<TodoResponse>();
  deleted = output<number>();

  onToggle(event: Event) {
    event.stopPropagation();
    this.toggled.emit(this.todo());
  }

  onCardClick(event: Event) {
    this.edited.emit(this.todo());
  }

  onDelete(event: Event) {
    event.stopPropagation();
    this.deleted.emit(this.todo().id);
  }
}
