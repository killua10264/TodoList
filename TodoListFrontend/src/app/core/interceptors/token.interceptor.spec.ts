import { TestBed } from '@angular/core/testing';
import { HttpHandlerFn, HttpRequest, HttpResponse } from '@angular/common/http';
import { of } from 'rxjs';
import { beforeEach, describe, expect, it, vi } from 'vitest';
import { environment } from '../../../environments/environment';
import { AuthService } from '../services/auth.service';
import { tokenInterceptor } from './token.interceptor';

describe('tokenInterceptor', () => {
  let getAccessToken: ReturnType<typeof vi.fn>;
  let next: HttpHandlerFn;
  let nextSpy: ReturnType<typeof vi.fn>;

  beforeEach(() => {
    getAccessToken = vi.fn().mockReturnValue('memory-access-token');
    nextSpy = vi.fn((_request: HttpRequest<unknown>) => of(new HttpResponse({ status: 200 })));
    next = nextSpy as HttpHandlerFn;

    TestBed.configureTestingModule({
      providers: [
        { provide: AuthService, useValue: { getAccessToken } }
      ]
    });
  });

  it('does not attach credentials or bearer token to third-party requests', () => {
    const request = new HttpRequest('GET', 'https://cdn.example.test/assets/font.woff2');

    TestBed.runInInjectionContext(() => tokenInterceptor(request, next).subscribe());

    const forwardedRequest = nextSpy.mock.calls[0][0] as HttpRequest<unknown>;
    expect(forwardedRequest.headers.has('Authorization')).toBe(false);
    expect(forwardedRequest.withCredentials).toBe(false);
  });

  it('attaches the memory token only to the trusted API URL', () => {
    const request = new HttpRequest('GET', `${environment.apiUrl}/api/todos`);

    TestBed.runInInjectionContext(() => tokenInterceptor(request, next).subscribe());

    const forwardedRequest = nextSpy.mock.calls[0][0] as HttpRequest<unknown>;
    expect(forwardedRequest.headers.get('Authorization')).toBe('Bearer memory-access-token');
    expect(forwardedRequest.withCredentials).toBe(true);
  });
});
