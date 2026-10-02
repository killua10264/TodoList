import { RenderMode, ServerRoute } from '@angular/ssr';

/**
 * Quy tắc render mode:
 * - landing (/), /login, /register → Prerender (SEO)
 * - /todos, /todos/:id/tree, /categories, /profile, /change-password → Client (protected)
 * - Wildcard → Prerender
 */
export const serverRoutes: ServerRoute[] = [
  { path: 'todos',              renderMode: RenderMode.Client },
  { path: 'todos/:id/tree',     renderMode: RenderMode.Client },
  { path: 'categories',         renderMode: RenderMode.Client },
  { path: 'profile',            renderMode: RenderMode.Client },
  { path: 'change-password',    renderMode: RenderMode.Client },
  { path: '**',                 renderMode: RenderMode.Prerender },
];
