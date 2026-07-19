import { HttpInterceptorFn } from '@angular/common/http';
import { inject, PLATFORM_ID } from '@angular/core';
import { Router } from '@angular/router';
import { BehaviorSubject, catchError, filter, switchMap, take, throwError } from 'rxjs';
import { AuthService } from '../services/auth.service';
import { isPlatformBrowser } from '@angular/common'; // Thêm import này

let isRefreshing = false;
let refreshTokenSubject = new BehaviorSubject<string | null>(null);

export const errorInterceptor: HttpInterceptorFn = (req, next) => {
    const authService = inject(AuthService);
    const router = inject(Router);
    const platformId = inject(PLATFORM_ID); // Hứng môi trường hiện tại
    if (!isPlatformBrowser(platformId)) {
        return next(req);
    }

    return next(req).pipe(
        catchError(error => {
            if (error.status === 401) {

                if (req.url.includes('/auth/login') || req.url.includes('/auth/refresh-token')) {
                    authService.clearTokens();
                    router.navigate(['/login']);
                    return throwError(() => error);
                }

                if (!isRefreshing) {
                    isRefreshing = true;
                    refreshTokenSubject.next(null);

                    return authService.refreshToken().pipe(
                        switchMap((response) => {
                            isRefreshing = false;
                            refreshTokenSubject.next(response.accessToken);

                            const retryReq = req.clone({
                                setHeaders: { Authorization: `Bearer ${response.accessToken}` }
                            });
                            return next(retryReq);
                        }),
                        catchError(refreshError => {
                            isRefreshing = false;
                            authService.clearTokens();
                            router.navigate(['/login']);
                            refreshTokenSubject.error(refreshError);
                            refreshTokenSubject = new BehaviorSubject<string | null>(null);

                            return throwError(() => refreshError);
                        })
                    );
                } else {
                    return refreshTokenSubject.pipe(
                        filter(token => token !== null),
                        take(1),
                        switchMap((token) => {
                            const retryReq = req.clone({
                                setHeaders: { Authorization: `Bearer ${token}` }
                            });
                            return next(retryReq);
                        })
                    );
                }
            }

            return throwError(() => error);
        })
    );
};
