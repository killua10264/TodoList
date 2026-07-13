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

    // 1. CHỐT CHẶN SSR: Nếu đang chạy trên Server, bỏ qua luôn Interceptor này
    // để tránh kẹt Node.js và tránh rò rỉ token giữa các người dùng
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

                            // 2. CHỐT CHẶN GIẢI TÁN PHÒNG CHỜ:
                            // Phóng lỗi vào Subject để giết chết các request đang đứng đợi
                            refreshTokenSubject.error(refreshError);
                            // Tạo lại Subject mới để sẵn sàng cho lần đăng nhập sau
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