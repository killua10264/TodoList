import { ApplicationConfig, inject, provideAppInitializer } from '@angular/core';
import { provideRouter } from '@angular/router';
import { provideHttpClient, withInterceptors, withFetch } from '@angular/common/http';
import { routes } from './app.routes';
import { tokenInterceptor } from './core/interceptors/token.interceptor';
import { errorInterceptor } from './core/interceptors/error.interceptor';
import { AuthFacade } from './core/services/auth.facade';

export const appConfig: ApplicationConfig = {
  providers: [
    provideAppInitializer(() => inject(AuthFacade).initialize()),
    provideRouter(routes),
    provideHttpClient(
      withInterceptors([
        tokenInterceptor,
        errorInterceptor
      ]),
      withFetch()
    )
  ]
};
