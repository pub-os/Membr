import { AfterViewInit, Component, ElementRef, signal, viewChild } from '@angular/core';
import { inject } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { Router } from '@angular/router';

import { ZardAlertComponent } from '@/shared/components/alert/alert.component';
import { ZardButtonComponent } from '@/shared/components/button/button.component';
import { ZardCardImports } from '@/shared/components/card/card.imports';
import { ZardInputComponent } from '@/shared/components/input/input.component';

import { MemberTokenService } from '@/services/member-token.service';

@Component({
  selector: 'app-token-scan',
  templateUrl: './token-scan.html',
  imports: [FormsModule, ZardCardImports, ZardButtonComponent, ZardAlertComponent, ZardInputComponent],
})
export class TokenScanComponent implements AfterViewInit {
  private memberTokenService = inject(MemberTokenService);
  private router = inject(Router);

  private tokenInput = viewChild<ElementRef<HTMLInputElement>>('tokenInput');

  value = '';
  loading = signal(false);
  error = signal('');

  ngAfterViewInit(): void {
    this.focusInput();
  }

  scan(): void {
    const value = this.value.trim();
    if (!value || this.loading()) return;

    this.error.set('');
    this.loading.set(true);
    this.memberTokenService.lookup(value).subscribe({
      next: (result) => {
        this.value = '';
        this.loading.set(false);
        this.router.navigate(['/member', result.memberId]);
      },
      error: () => {
        this.error.set('Token not recognized or has been revoked.');
        this.value = '';
        this.loading.set(false);
        this.focusInput();
      },
    });
  }

  private focusInput(): void {
    setTimeout(() => this.tokenInput()?.nativeElement.focus());
  }
}
