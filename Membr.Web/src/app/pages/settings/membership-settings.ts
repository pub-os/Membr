import { Component, inject, OnInit, signal } from '@angular/core';
import { FormsModule } from '@angular/forms';

import { ZardAlertComponent } from '@/shared/components/alert/alert.component';
import { ZardButtonComponent } from '@/shared/components/button/button.component';
import { ZardCardImports } from '@/shared/components/card/card.imports';

import { MembershipSettingsService } from '@/services/membership-settings.service';

@Component({
  selector: 'app-membership-settings',
  imports: [FormsModule, ZardCardImports, ZardButtonComponent, ZardAlertComponent],
  templateUrl: './membership-settings.html',
})
export class MembershipSettingsComponent implements OnInit {
  private membershipSettingsService = inject(MembershipSettingsService);

  loading = signal(false);
  saving = signal(false);
  error = signal('');
  successMessage = signal('');

  allowMultipleMemberships = false;

  ngOnInit(): void {
    this.loading.set(true);
    this.membershipSettingsService.get().subscribe({
      next: (settings) => {
        this.allowMultipleMemberships = settings.allowMultipleMemberships;
        this.loading.set(false);
      },
      error: () => {
        this.error.set('Failed to load membership settings');
        this.loading.set(false);
      }
    });
  }

  save(): void {
    this.saving.set(true);
    this.error.set('');
    this.successMessage.set('');

    this.membershipSettingsService.update({ allowMultipleMemberships: this.allowMultipleMemberships }).subscribe({
      next: () => {
        this.saving.set(false);
        this.successMessage.set('Settings saved');
      },
      error: () => {
        this.error.set('Failed to save membership settings');
        this.saving.set(false);
      }
    });
  }
}
