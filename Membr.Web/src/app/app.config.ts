import { ApplicationConfig, inject, provideAppInitializer, provideBrowserGlobalErrorListeners } from '@angular/core';
import { provideAnimationsAsync } from '@angular/platform-browser/animations/async';
import { provideRouter } from '@angular/router';
import { provideHttpClient, withInterceptors } from '@angular/common/http';
import { firstValueFrom } from 'rxjs';
import { routes } from './app.routes';
import { provideZard } from '@/shared/core/provider/providezard';
import { authInterceptor } from '@/interceptors/auth.interceptor';
import { AuthService } from '@/services/auth.service';

export const appConfig: ApplicationConfig = {
  providers: [
    provideBrowserGlobalErrorListeners(),
    provideRouter(routes),
    provideHttpClient(withInterceptors([authInterceptor])),
    provideZard(),
    provideAnimationsAsync(),
    provideAppInitializer(() => firstValueFrom(inject(AuthService).restoreSession())),
  ]
};
