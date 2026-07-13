import { Routes } from '@angular/router';

import { LandingLayoutComponent } from './layouts/landing-layout/landing-layout';
import { AuthLayoutComponent } from './layouts/auth-layout/auth-layout';
import { MainLayoutComponent } from './layouts/main-layout/main-layout';

import { authGuard } from './core/guards/auth.guard';

import { HomeComponent } from './features/home/home';
import { LoginComponent } from './features/auth/login/login';
import { RegisterComponent } from './features/auth/register/register';
import { TodoListComponent } from './features/todo/todo-list/todo-list';
import { TodoTreeViewComponent } from './features/todo/todo-tree-view/todo-tree-view';
import { ProjectDetailComponent } from './features/project/project-detail/project-detail';
import { UserProfileComponent } from './features/user/user-profile/user-profile';
import { ChangePasswordComponent } from './features/user/change-password/change-password';

export const routes: Routes = [
  {
    path: '',
    component: LandingLayoutComponent,
    children: [
      { path: '', component: HomeComponent }
    ]
  },

  {
    path: '',
    component: AuthLayoutComponent,
    children: [
      { path: 'login', component: LoginComponent },
      { path: 'register', component: RegisterComponent }
    ]
  },

  {
    path: '',
    component: MainLayoutComponent,
    canActivate: [authGuard],
    children: [
      { path: 'todos', component: TodoListComponent },
      { path: 'todos/:id/tree', component: TodoTreeViewComponent },
      { path: 'projects/:id', redirectTo: 'todos' },
      { path: 'profile', component: UserProfileComponent },
      { path: 'change-password', component: ChangePasswordComponent }
    ]
  },

  { path: '**', redirectTo: '' }
];
