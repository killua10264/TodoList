import { Routes } from '@angular/router';

import { LandingLayoutComponent } from './layouts/landing-layout/landing-layout';
import { AuthLayoutComponent } from './layouts/auth-layout/auth-layout';

import { HomeComponent } from './features/home/home';
import { LoginComponent } from './features/auth/login/login';
import { RegisterComponent } from './features/auth/register/register';

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

  { path: '**', redirectTo: '' }
];
