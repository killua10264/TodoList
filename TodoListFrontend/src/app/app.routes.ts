import { Routes } from '@angular/router';

import { authGuard } from './core/guards/auth.guard';

export const routes: Routes = [
  {
    path: '',
    loadComponent: () => import('./layouts/landing-layout/landing-layout')
      .then(m => m.LandingLayoutComponent),
    children: [
      {
        path: '',
        loadComponent: () => import('./features/home/home').then(m => m.HomeComponent)
      }
    ]
  },

  {
    path: '',
    loadComponent: () => import('./layouts/auth-layout/auth-layout')
      .then(m => m.AuthLayoutComponent),
    children: [
      {
        path: 'login',
        loadComponent: () => import('./features/auth/login/login').then(m => m.LoginComponent)
      },
      {
        path: 'register',
        loadComponent: () => import('./features/auth/register/register').then(m => m.RegisterComponent)
      }
    ]
  },

  {
    path: '',
    loadComponent: () => import('./layouts/main-layout/main-layout')
      .then(m => m.MainLayoutComponent),
    canActivate: [authGuard],
    children: [
      {
        path: 'todos',
        loadComponent: () => import('./features/todo/todo-list/todo-list').then(m => m.TodoListComponent)
      },
      {
        path: 'todos/:id/tree',
        loadComponent: () => import('./features/todo/todo-tree-view/todo-tree-view')
          .then(m => m.TodoTreeViewComponent)
      },
      {
        path: 'categories',
        loadComponent: () => import('./features/category/category-dashboard/category-dashboard')
          .then(m => m.CategoryDashboardComponent)
      },
      {
        path: 'profile',
        loadComponent: () => import('./features/user/user-profile/user-profile')
          .then(m => m.UserProfileComponent)
      },
      {
        path: 'change-password',
        loadComponent: () => import('./features/user/change-password/change-password')
          .then(m => m.ChangePasswordComponent)
      }
    ]
  },

  { path: '**', redirectTo: '' }
];
