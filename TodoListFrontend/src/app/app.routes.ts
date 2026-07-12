import { Routes } from '@angular/router';

import { LandingLayoutComponent } from './layouts/landing-layout/landing-layout';
import { AuthLayoutComponent } from './layouts/auth-layout/auth-layout';
import { MainLayoutComponent } from './layouts/main-layout/main-layout';

import { authGuard } from './core/guards/auth.guard';

import { HomeComponent } from './features/home/home';
import { LoginComponent } from './features/auth/login/login';
import { RegisterComponent } from './features/auth/register/register';
import { TodoListComponent } from './features/todo/todo-list/todo-list';
import { ProjectDetailComponent } from './features/project/project-detail/project-detail';

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
      { path: 'projects/:id', redirectTo: 'todos' }
    ]
  },

  { path: '**', redirectTo: '' }
];
