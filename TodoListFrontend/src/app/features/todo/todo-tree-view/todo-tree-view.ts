import { Component, inject, signal, OnInit, DestroyRef } from '@angular/core';
import { CommonModule } from '@angular/common';
import { ActivatedRoute, Router, RouterLink } from '@angular/router';
import { FormsModule } from '@angular/forms';
import { takeUntilDestroyed } from '@angular/core/rxjs-interop';
import { TodoService } from '../../../core/services/todo.service';
import { SubTaskService } from '../../../core/services/subtask.service';
import { TodoResponse } from '../../../core/models/todo.model';
import { SubTaskResponse } from '../../../core/models/subtask.model';
import { LoadingSpinnerComponent } from '../../../shared/loading-spinner/loading-spinner';
import { ConfirmDialogComponent } from '../../../shared/confirm-dialog/confirm-dialog';
import { TodoTreeLeafComponent } from './components/todo-tree-leaf/todo-tree-leaf.component';

@Component({
  selector: 'app-todo-tree-view',
  imports: [CommonModule, RouterLink, FormsModule, LoadingSpinnerComponent, ConfirmDialogComponent, TodoTreeLeafComponent],
  templateUrl: './todo-tree-view.html',
  styleUrl: './todo-tree-view.css'
})
export class TodoTreeViewComponent implements OnInit {
  private route = inject(ActivatedRoute);
  private router = inject(Router);
  private todoService = inject(TodoService);
  private subTaskService = inject(SubTaskService);
  private destroyRef = inject(DestroyRef);

  todo = signal<TodoResponse | null>(null);
  subTasks = signal<SubTaskResponse[]>([]);
  isLoading = signal<boolean>(true);
  newLeafTitle = signal<string>('');
  isAdding = signal<boolean>(false);
  showDeleteConfirm = signal<boolean>(false);
  deleteTargetId = signal<number | null>(null);
  deleteTargetType = signal<'todo' | 'subtask'>('subtask');

  ngOnInit() {
    this.route.paramMap.pipe(takeUntilDestroyed(this.destroyRef)).subscribe(params => {
      const id = +(params.get('id') || 0);
      if (id > 0) {
        this.loadTreeData(id);
      }
    });

    this.subTaskService.refresh$.pipe(takeUntilDestroyed(this.destroyRef)).subscribe(() => {
      const currentTodo = this.todo();
      if (currentTodo) {
        this.loadSubTasks(currentTodo.id);
      }
    });
  }

  loadTreeData(todoId: number) {
    this.isLoading.set(true);
    this.todoService.getById(todoId).pipe(takeUntilDestroyed(this.destroyRef)).subscribe({
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
    this.subTaskService.getByTodoId(todoId).pipe(takeUntilDestroyed(this.destroyRef)).subscribe({
      next: (list) => {
        this.subTasks.set(list);
        this.isLoading.set(false);
      },
      error: () => {
        this.isLoading.set(false);
      }
    });
  }
  onAddLeaf() {
    const title = this.newLeafTitle().trim();
    const currentTodo = this.todo();
    if (!title || !currentTodo || this.isAdding()) return;

    this.isAdding.set(true);

    // Optimistic: add a temporary leaf to UI immediately
    const tempLeaf: SubTaskResponse = {
      id: -Date.now(),
      title,
      isCompleted: false,
      sortOrder: this.subTasks().length,
      leafShape: Math.floor(Math.random() * 5),
      createdAt: new Date().toISOString(),
      todoId: currentTodo.id
    };
    this.subTasks.update(list => [...list, tempLeaf]);
    this.newLeafTitle.set('');

    this.subTaskService.create({
      title: title,
      todoId: currentTodo.id
    }).pipe(takeUntilDestroyed(this.destroyRef)).subscribe({
      next: (created) => {
        // Replace temp leaf with real one from server
        this.subTasks.update(list =>
          list.map(s => s.id === tempLeaf.id ? created : s)
        );
        this.isAdding.set(false);
      },
      error: () => {
        // Rollback: remove temp leaf
        this.subTasks.update(list => list.filter(s => s.id !== tempLeaf.id));
        this.isAdding.set(false);
      }
    });
  }
  onToggleLeaf(leaf: SubTaskResponse) {
    const newCompleted = !leaf.isCompleted;

    // Optimistic: update UI immediately
    this.subTasks.update(list =>
      list.map(s => s.id === leaf.id ? { ...s, isCompleted: newCompleted } : s)
    );

    this.subTaskService.updateSilent(leaf.id, {
      title: leaf.title,
      isCompleted: newCompleted,
      sortOrder: leaf.sortOrder
    }).pipe(takeUntilDestroyed(this.destroyRef)).subscribe({
      error: () => {
        // Rollback on error
        this.subTasks.update(list =>
          list.map(s => s.id === leaf.id ? { ...s, isCompleted: !newCompleted } : s)
        );
      }
    });
  }

  saveEditingLeaf(leaf: SubTaskResponse, newTitle: string) {
    if (!newTitle || newTitle === leaf.title) return;

    const oldTitle = leaf.title;

    // Optimistic: update title in UI immediately
    this.subTasks.update(list =>
      list.map(s => s.id === leaf.id ? { ...s, title: newTitle } : s)
    );

    this.subTaskService.updateSilent(leaf.id, {
      title: newTitle,
      isCompleted: leaf.isCompleted,
      sortOrder: leaf.sortOrder
    }).pipe(takeUntilDestroyed(this.destroyRef)).subscribe({
      error: () => {
        // Rollback on error
        this.subTasks.update(list =>
          list.map(s => s.id === leaf.id ? { ...s, title: oldTitle } : s)
        );
      }
    });
  }
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
      // Optimistic: remove from UI immediately
      const previousSubTasks = this.subTasks();
      this.subTasks.update(list => list.filter(s => s.id !== id));

      this.subTaskService.deleteSilent(id).pipe(takeUntilDestroyed(this.destroyRef)).subscribe({
        error: () => {
          // Rollback on error
          this.subTasks.set(previousSubTasks);
        }
      });
    } else if (type === 'todo') {
      this.todoService.delete(id).pipe(takeUntilDestroyed(this.destroyRef)).subscribe({
        next: () => {
          this.router.navigate(['/todos']);
        }
      });
    }
  }

  onToggleParentTodo() {
    const current = this.todo();
    if (!current) return;

    const newCompleted = !current.isCompleted;

    // Optimistic: update UI immediately
    this.todo.set({ ...current, isCompleted: newCompleted });

    this.todoService.updateSilent(current.id, {
      title: current.title,
      description: current.description,
      priority: current.priority,
      dueDate: current.dueDate,
      isCompleted: newCompleted,
      categoryId: current.categoryId
    }).pipe(takeUntilDestroyed(this.destroyRef)).subscribe({
      error: () => {
        // Rollback on error
        this.todo.set({ ...current, isCompleted: !newCompleted });
      }
    });
  }
}

  
