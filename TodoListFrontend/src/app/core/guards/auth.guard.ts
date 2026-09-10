import { inject, PLATFORM_ID } from '@angular/core';
import { CanActivateFn, Router } from '@angular/router';
import { isPlatformBrowser } from '@angular/common';
import { AuthFacade } from '../services/auth.facade';
import { map } from 'rxjs';

export const authGuard: CanActivateFn = (route, state) => {
    const authFacade = inject(AuthFacade);
    const router = inject(Router);
    const platformId = inject(PLATFORM_ID);

    if (!isPlatformBrowser(platformId)) {
        return true;
    }

    if (authFacade.isAuthenticated()) return true;

    return authFacade.initialize().pipe(
        map(isAuthenticated => isAuthenticated
            ? true
            : router.createUrlTree(['/login'], { queryParams: { returnUrl: state.url } }))
    );
};
