import { HttpInterceptorFn } from '@angular/common/http';
import { inject } from '@angular/core';
import { AuthService } from '../services/auth.service';
import { environment } from '../../../environments/environment';

export const tokenInterceptor: HttpInterceptorFn = (req, next) => {
    const authService = inject(AuthService);
    const token = authService.getAccessToken();
    const isTrustedApiUrl = req.url === environment.apiUrl || req.url.startsWith(`${environment.apiUrl}/`);

    if (!isTrustedApiUrl) {
        return next(req);
    }

    if (token) {
        const clonedReq = req.clone({
            withCredentials: true,
            setHeaders: {
                Authorization: `Bearer ${token}`
            }
        });
        return next(clonedReq);
    }

    return next(req.clone({ withCredentials: true }));
};
