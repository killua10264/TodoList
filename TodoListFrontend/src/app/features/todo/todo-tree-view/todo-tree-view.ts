import { Component, inject, signal, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { ActivatedRoute, Router, RouterLink } from '@angular/router';
import { FormsModule } from '@angular/forms';
import { TodoService } from '../../../core/services/todo.service';
import { SubTaskService } from '../../../core/services/subtask.service';
import { TodoResponse } from '../../../core/models/todo.model';
import { SubTaskResponse } from '../../../core/models/subtask.model';
import { LoadingSpinnerComponent } from '../../../shared/loading-spinner/loading-spinner';
import { ConfirmDialogComponent } from '../../../shared/confirm-dialog/confirm-dialog';

@Component({
  selector: 'app-todo-tree-view',
  imports: [CommonModule, RouterLink, FormsModule, LoadingSpinnerComponent, ConfirmDialogComponent],
  templateUrl: './todo-tree-view.html',
  styleUrl: './todo-tree-view.css'
})
export class TodoTreeViewComponent implements OnInit {
  private route = inject(ActivatedRoute);
  private router = inject(Router);
  private todoService = inject(TodoService);
  private subTaskService = inject(SubTaskService);

  todo = signal<TodoResponse | null>(null);
  subTasks = signal<SubTaskResponse[]>([]);
  isLoading = signal<boolean>(true);

  // Thêm lá mới (subtask)
  newLeafTitle = signal<string>('');
  isAdding = signal<boolean>(false);

  // Chỉnh sửa lá
  editingSubTaskId = signal<number | null>(null);
  editingTitle = signal<string>('');

  // Xác nhận xóa
  showDeleteConfirm = signal<boolean>(false);
  deleteTargetId = signal<number | null>(null);
  deleteTargetType = signal<'todo' | 'subtask'>('subtask');

  ngOnInit() {
    this.route.paramMap.subscribe(params => {
      const id = +(params.get('id') || 0);
      if (id > 0) {
        this.loadTreeData(id);
      }
    });

    this.subTaskService.refresh$.subscribe(() => {
      const currentTodo = this.todo();
      if (currentTodo) {
        this.loadSubTasks(currentTodo.id);
      }
    });
  }

  loadTreeData(todoId: number) {
    this.isLoading.set(true);
    this.todoService.getById(todoId).subscribe({
      next: (data) => {
        this.todo.set(data);
        this.loadSubTasks(todoId);
      },
      error: () => {
        this.isLoading.set(false);
        this.router.navigate(['/todos']);
      }
    });
  }

  loadSubTasks(todoId: number) {
    this.subTaskService.getByTodoId(todoId).subscribe({
      next: (list) => {
        this.subTasks.set(list);
        this.isLoading.set(false);
      },
      error: () => {
        this.isLoading.set(false);
      }
    });
  }

  // Gieo lá mới (Add Subtask)
  onAddLeaf() {
    const title = this.newLeafTitle().trim();
    const currentTodo = this.todo();
    if (!title || !currentTodo || this.isAdding()) return;

    this.isAdding.set(true);
    this.subTaskService.create({
      title: title,
      todoId: currentTodo.id
    }).subscribe({
      next: () => {
        this.newLeafTitle.set('');
        this.isAdding.set(false);
      },
      error: () => {
        this.isAdding.set(false);
      }
    });
  }

  // Toggle trạng thái hoàn thành lá (SubTask)
  onToggleLeaf(leaf: SubTaskResponse) {
    this.subTaskService.update(leaf.id, {
      title: leaf.title,
      isCompleted: !leaf.isCompleted,
      sortOrder: leaf.sortOrder
    }).subscribe();
  }

  // Bắt đầu sửa lá
  startEditingLeaf(leaf: SubTaskResponse) {
    this.editingSubTaskId.set(leaf.id);
    this.editingTitle.set(leaf.title);
  }

  // Lưu chỉnh sửa lá
  saveEditingLeaf(leaf: SubTaskResponse) {
    const newTitle = this.editingTitle().trim();
    if (!newTitle || newTitle === leaf.title) {
      this.editingSubTaskId.set(null);
      return;
    }

    this.subTaskService.update(leaf.id, {
      title: newTitle,
      isCompleted: leaf.isCompleted,
      sortOrder: leaf.sortOrder
    }).subscribe({
      next: () => {
        this.editingSubTaskId.set(null);
      }
    });
  }

  cancelEditingLeaf() {
    this.editingSubTaskId.set(null);
  }

  // Mở confirm dialog xóa
  openDeleteConfirm(type: 'todo' | 'subtask', id: number) {
    this.deleteTargetType.set(type);
    this.deleteTargetId.set(id);
    this.showDeleteConfirm.set(true);
  }

  onDeleteConfirmed(confirmed: boolean) {
    this.showDeleteConfirm.set(false);
    if (!confirmed) return;

    const id = this.deleteTargetId();
    const type = this.deleteTargetType();
    if (!id) return;

    if (type === 'subtask') {
      this.subTaskService.delete(id).subscribe();
    } else if (type === 'todo') {
      this.todoService.delete(id).subscribe({
        next: () => {
          this.router.navigate(['/todos']);
        }
      });
    }
  }

  // Cập nhật trạng thái hoàn thành của toàn bộ cây (Todo cha)
  onToggleParentTodo() {
    const current = this.todo();
    if (!current) return;

    this.todoService.update(current.id, {
      title: current.title,
      description: current.description,
      priority: current.priority,
      dueDate: current.dueDate,
      isCompleted: !current.isCompleted,
      categoryId: current.categoryId
    }).subscribe({
      next: (updated) => {
        this.todo.set(updated);
      }
    });
  }
}
