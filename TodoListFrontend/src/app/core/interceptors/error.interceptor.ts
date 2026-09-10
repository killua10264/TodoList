import { HttpInterceptorFn } from '@angular/common/http';
import { inject, PLATFORM_ID } from '@angular/core';
import { Router } from '@angular/router';
import { Observable, catchError, finalize, map, shareReplay, switchMap, take, throwError } from 'rxjs';
import { AuthService } from '../services/auth.service';
import { AuthFacade } from '../services/auth.facade';
import { isPlatformBrowser } from '@angular/common';

let refreshInFlight$: Observable<string> | null = null;

export const errorInterceptor: HttpInterceptorFn = (req, next) => {
    const authService = inject(AuthService);
    const authFacade = inject(AuthFacade);
    const router = inject(Router);
    const platformId = inject(PLATFORM_ID);
    if (!isPlatformBrowser(platformId)) {
        return next(req);
    }

    return next(req).pipe(
        catchError(error => {
            if (error.status === 401) {

                if (isAuthEndpoint(req.url)) {
                    authFacade.markAnonymous();
                    router.navigate(['/login']);
                    return throwError(() => error);
                }

                if (!refreshInFlight$) {
                    refreshInFlight$ = authService.refreshToken().pipe(
                        map(response => response.accessToken),
                        finalize(() => {
                            refreshInFlight$ = null;
                        }),
                        shareReplay({ bufferSize: 1, refCount: false })
                    );
                }

                return refreshInFlight$.pipe(
                    take(1),
                    switchMap(token => next(req.clone({
                        withCredentials: true,
                        setHeaders: { Authorization: `Bearer ${token}` }
                    }))),
                    catchError(refreshError => {
                        authFacade.markAnonymous();
                        router.navigate(['/login']);
                        return throwError(() => refreshError);
                    })
                );
            }

            return throwError(() => error);
        })
    );
};

function isAuthEndpoint(url: string): boolean {
    return ['/auth/login', '/auth/register', '/auth/refresh-token', '/auth/logout', '/auth/logout-all']
        .some(path => url.includes(path));
}
