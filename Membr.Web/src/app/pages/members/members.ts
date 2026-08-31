import { Component, inject, signal, OnInit } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { ActivatedRoute, RouterLink } from '@angular/router';

import { ZardAlertComponent } from '@/shared/components/alert/alert.component';
import { ZardButtonComponent } from '@/shared/components/button/button.component';
import { ZardCardImports } from '@/shared/components/card/card.imports';
import { ZardInputComponent } from '@/shared/components/input/input.component';
import { ZardTableImports } from '@/shared/components/table/table.imports';

import { MemberService, MemberDto, CreateMemberRequest } from '@/services/member.service';

@Component({
  selector: 'app-members',
  imports: [FormsModule, RouterLink, ZardCardImports, ZardTableImports, ZardInputComponent, ZardButtonComponent, ZardAlertComponent],
  templateUrl: './members.html',
})
export class MembersComponent implements OnInit {
  private memberService = inject(MemberService);
  private route = inject(ActivatedRoute);

  // State
  members = signal<MemberDto[]>([]);
  loading = signal(false);
  error = signal('');
  successMessage = signal('');

  // Create form
  showCreateForm = signal(false);
  newMember: CreateMemberRequest = {
    firstName: '',
    surname: '',
    dateOfBirth: '',
    contacts: [],
  };

  private currentQuery = '';

  ngOnInit(): void {
    this.route.queryParams.subscribe(params => {
      this.currentQuery = params['q'] ?? '';
      if (this.currentQuery) {
        this.searchMembers(this.currentQuery);
      } else {
        this.listMembers();
      }
    });
  }

  listMembers(): void {
    this.loading.set(true);
    this.error.set('');

    this.memberService.list().subscribe({
      next: (result) => {
        this.members.set(result.items);
        this.loading.set(false);
      },
      error: () => {
        this.error.set('Failed to load members');
        this.loading.set(false);
      }
    });
  }

  searchMembers(query: string): void {
    this.loading.set(true);
    this.error.set('');

    this.memberService.search(query).subscribe({
      next: (result) => {
        this.members.set(result.items);
        this.loading.set(false);
      },
      error: () => {
        this.error.set('Failed to search members');
        this.loading.set(false);
      }
    });
  }

  toggleCreateForm(): void {
    this.showCreateForm.update(v => !v);
    this.error.set('');
    this.successMessage.set('');
  }

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
        this.showCreateForm.set(false);
        this.newMember = { firstName: '', surname: '', dateOfBirth: '', contacts: [] };

        if (this.currentQuery) {
          this.searchMembers(this.currentQuery);
        } else {
          this.listMembers();
        }
      },
      error: (err) => {
        this.error.set(err.error?.title || 'Failed to create member');
        this.loading.set(false);
      }
    });
  }
}