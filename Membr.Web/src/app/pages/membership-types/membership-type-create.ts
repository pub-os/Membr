import { Component, inject, signal } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { Router } from '@angular/router';

import { ZardAlertComponent } from '@/shared/components/alert/alert.component';
import { ZardButtonComponent } from '@/shared/components/button/button.component';
import { ZardCardImports } from '@/shared/components/card/card.imports';
import { ZardInputComponent } from '@/shared/components/input/input.component';

import { CreateMembershipTypeRequest, MembershipTypeService } from '@/services/membership-type.service';

@Component({
  selector: 'app-membership-type-create',
  imports: [FormsModule, ZardCardImports, ZardInputComponent, ZardButtonComponent, ZardAlertComponent],
  templateUrl: './membership-type-create.html',
})
export class MembershipTypeCreateComponent {
  private membershipTypeService = inject(MembershipTypeService);
  private router = inject(Router);

  loading = signal(false);
  error = signal('');

  name = '';
  description = '';
  isActive = true;
  renewalMode: CreateMembershipTypeRequest['renewalMode'] = 'Rolling';
  durationMonths: number | null = 12;
  fixedTermAnchorMonth: number | null = 1;
  fixedTermAnchorDay: number | null = 1;

  readonly months = [
    { value: 1, label: 'January' },
    { value: 2, label: 'February' },
    { value: 3, label: 'March' },
    { value: 4, label: 'April' },
    { value: 5, label: 'May' },
    { value: 6, label: 'June' },
    { value: 7, label: 'July' },
    { value: 8, label: 'August' },
    { value: 9, label: 'September' },
    { value: 10, label: 'October' },
    { value: 11, label: 'November' },
    { value: 12, label: 'December' },
  ];

  createMembershipType(): void {
    if (!this.name.trim()) {
      this.error.set('Name is required');
      return;
    }

    if (this.renewalMode === 'Rolling' && !Number.isInteger(this.durationMonths)) {
      this.error.set('Duration must be a whole number of months — memberships can\'t be measured in fractions of a month.');
      return;
    }

    if (this.renewalMode === 'FixedTerm' && !Number.isInteger(this.fixedTermAnchorDay)) {
      this.error.set('Expiry day must be a whole number.');
      return;
    }

    const request: CreateMembershipTypeRequest = {
      name: this.name,
      description: this.description.trim() || null,
      isActive: this.isActive,
      renewalMode: this.renewalMode,
      durationMonths: this.renewalMode === 'Rolling' ? this.durationMonths : null,
      fixedTermAnchorMonth: this.renewalMode === 'FixedTerm' ? this.fixedTermAnchorMonth : null,
      fixedTermAnchorDay: this.renewalMode === 'FixedTerm' ? this.fixedTermAnchorDay : null,
    };

    this.loading.set(true);
    this.error.set('');

    this.membershipTypeService.create(request).subscribe({
      next: () => {
        this.loading.set(false);
        this.router.navigate(['/membershiptype/list']);
      },
      error: (err) => {
        this.error.set(err.error?.errors?.renewalMode?.[0] ?? 'Failed to create membership type');
        this.loading.set(false);
      }
    });
  }
}
