import { Component, inject, signal } from '@angular/core';
import { FormsModule } from '@angular/forms';

import { ZardAlertComponent } from '@/shared/components/alert/alert.component';
import { ZardButtonComponent } from '@/shared/components/button/button.component';
import { ZardCardImports } from '@/shared/components/card/card.imports';
import { ZardInputComponent } from '@/shared/components/input/input.component';

import { MemberService, CreateMemberRequest } from '@/services/member.service';

@Component({
  selector: 'app-member-create',
  imports: [FormsModule, ZardCardImports, ZardInputComponent, ZardButtonComponent, ZardAlertComponent],
  templateUrl: './member-create.html',
})
export class CreateMembersComponent {
  private memberService = inject(MemberService);
  loading = signal(false);
  error = signal('');
  successMessage = signal('');

  newMember: CreateMemberRequest = {
    firstName: '',
    surname: '',
    dateOfBirth: ''
  };

  createMember(): void {
    if (!this.newMember.firstName.trim() || !this.newMember.surname.trim() || !this.newMember.dateOfBirth) {
      this.error.set('All fields are required');
      return;
    }

    this.loading.set(true);
    this.error.set('');
    this.successMessage.set('');

    this.memberService.create(this.newMember).subscribe({
      next: (created) => {
        this.successMessage.set(`Member ${created.firstName} ${created.surname} created`);
        this.loading.set(false);
        this.newMember = { firstName: '', surname: '', dateOfBirth: '' };
      },
      error: (err) => {
        this.error.set(err.error?.title || 'Failed to create member');
        this.loading.set(false);
      }
    });
  }
}
