import { HttpInterceptorFn } from '@angular/common/http';
import { inject } from '@angular/core';
import { Router } from '@angular/router';
import { catchError, switchMap, throwError } from 'rxjs';
import { AuthService } from '../services/auth.service';

export const errorInterceptor: HttpInterceptorFn = (req, next) => {
    const authService = inject(AuthService);
    const router = inject(Router);

    return next(req).pipe(
        catchError(error => {
            if (error.status === 401) {
                if (req.url.includes('/auth/')) {
                    authService.logout();
                    return throwError(() => error);
                }

                return authService.refreshToken().pipe(
                    switchMap(() => {
                        const newToken = authService.getAccessToken();
                        const retryReq = req.clone({
                            setHeaders: { Authorization: `Bearer ${newToken}` }
                        });
                        return next(retryReq);
                    }),
                    catchError(refreshError => {
                        authService.logout();
                        router.navigate(['/login']);
                        return throwError(() => refreshError);
                    })
                );
            }

            return throwError(() => error);
        })
    );
};
