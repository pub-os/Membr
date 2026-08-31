import { DatePipe } from '@angular/common';
import { Component, computed, inject, signal, OnInit } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { ActivatedRoute, RouterLink } from '@angular/router';

import { ZardAlertComponent } from '@/shared/components/alert/alert.component';
import { ZardBadgeComponent } from '@/shared/components/badge';
import { ZardButtonComponent } from '@/shared/components/button/button.component';
import { ZardCardImports } from '@/shared/components/card/card.imports';
import { ZardInputComponent } from '@/shared/components/input/input.component';
import { ZardTableImports } from '@/shared/components/table/table.imports';

import { MemberDto, MemberService } from '@/services/member.service';
import { MembershipDto, MembershipService } from '@/services/membership.service';
import { MembershipTypeDto, MembershipTypeService } from '@/services/membership-type.service';
import { MemberUdfFieldDto, MemberUdfValueService } from '@/services/member-udf-value.service';
import { ContactDto, MemberContactService } from '@/services/member-contact.service';
import { TokenDto, TokenType, MemberTokenService } from '@/services/member-token.service';
import { UdfValueInputComponent } from '@/shared/components/udf-value/udf-value-input.component';

type ContactType = ContactDto['contactType'];
type ContactDraft = { contactType: ContactType; contactDetail: string; isPrimary: boolean };
type TokenDraft = { tokenType: TokenType; value: string };

@Component({
  selector: 'app-member-overview',
  templateUrl: './member-overview.html',
  imports: [
    RouterLink, FormsModule, DatePipe, ZardCardImports, ZardTableImports, ZardButtonComponent, ZardAlertComponent,
    ZardBadgeComponent, ZardInputComponent, UdfValueInputComponent,
  ],
})
export class MemberOverviewComponent implements OnInit {
  private memberService = inject(MemberService);
  private membershipService = inject(MembershipService);
  private membershipTypeService = inject(MembershipTypeService);
  private memberUdfValueService = inject(MemberUdfValueService);
  private memberContactService = inject(MemberContactService);
  private memberTokenService = inject(MemberTokenService);
  private route = inject(ActivatedRoute);

  member = signal<MemberDto | null>(null);
  memberships = signal<MembershipDto[]>([]);
  membershipTypes = signal<MembershipTypeDto[]>([]);
  udfFields = signal<MemberUdfFieldDto[]>([]);
  contacts = signal<ContactDto[]>([]);
  tokens = signal<TokenDto[]>([]);

  loading = signal(false);
  membershipsLoading = signal(false);
  udfFieldsLoading = signal(false);
  contactsLoading = signal(false);
  tokensLoading = signal(false);
  error = signal('');
  membershipError = signal('');
  udfFieldsError = signal('');
  contactError = signal('');
  tokenError = signal('');
  membershipActionLoading = signal<MembershipDto['id'] | 'create' | null>(null);
  udfFieldSavingId = signal<MemberUdfFieldDto['definitionId'] | null>(null);
  contactActionLoading = signal<ContactDto['id'] | 'create' | null>(null);
  tokenActionLoading = signal<TokenDto['id'] | 'create' | null>(null);

  contactTypes: ContactType[] = ['Email', 'Phone'];
  showAddContactModal = signal(false);
  newContact: ContactDraft = { contactType: 'Email', contactDetail: '', isPrimary: false };
  editingContactId = signal<ContactDto['id'] | null>(null);
  editContactDraft: ContactDraft = { contactType: 'Email', contactDetail: '', isPrimary: false };

  tokenTypes: TokenType[] = ['Rfid'];
  showAddTokenModal = signal(false);
  newToken: TokenDraft = { tokenType: 'Rfid', value: '' };
  tokenPendingRevoke = signal<TokenDto | null>(null);

  selectedMembershipTypeId: number | null = null;
  membershipPendingRenewal = signal<MembershipDto | null>(null);
  showAddMembershipModal = signal(false);

  activeMembershipCount = computed(() =>
    this.memberships().filter(m => m.isActive).length,
  );
  hasMultipleActiveMemberships = computed(() => this.activeMembershipCount() > 1);

  private memberId: string | null = null;

  ngOnInit(): void {
    const id = this.route.snapshot.paramMap.get('id');
    if (!id) {
      this.error.set('No member specified');
      return;
    }
    this.memberId = id;

    this.loading.set(true);
    this.memberService.get(id).subscribe({
      next: (member) => {
        this.member.set(member);
        this.loading.set(false);
      },
      error: () => {
        this.error.set('Failed to load member');
        this.loading.set(false);
      }
    });

    this.loadMemberships();
    this.loadUdfFields();
    this.loadContacts();
    this.loadTokens();
    this.membershipTypeService.list().subscribe({
      next: (types) => this.membershipTypes.set(types.filter(t => t.isActive)),
    });
  }

  loadContacts(): void {
    if (!this.memberId) return;

    this.contactsLoading.set(true);
    this.memberContactService.listForMember(this.memberId).subscribe({
      next: (contacts) => {
        this.contacts.set(contacts);
        this.contactsLoading.set(false);
      },
      error: () => {
        this.contactError.set('Failed to load contact details');
        this.contactsLoading.set(false);
      },
    });
  }

  openAddContactModal(): void {
    this.contactError.set('');
    this.newContact = { contactType: 'Email', contactDetail: '', isPrimary: false };
    this.showAddContactModal.set(true);
  }

  closeAddContactModal(): void {
    this.showAddContactModal.set(false);
    this.contactError.set('');
  }

  addContact(): void {
    if (!this.memberId || !this.newContact.contactDetail.trim()) return;

    this.contactError.set('');
    this.contactActionLoading.set('create');
    this.memberContactService.create(this.memberId, { ...this.newContact }).subscribe({
      next: () => {
        this.contactActionLoading.set(null);
        this.showAddContactModal.set(false);
        this.loadContacts();
      },
      error: (err) => {
        this.contactError.set(err.error?.errors?.contactDetail?.[0] ?? 'Failed to add contact detail');
        this.contactActionLoading.set(null);
      },
    });
  }

  startEditContact(contact: ContactDto): void {
    this.contactError.set('');
    this.editingContactId.set(contact.id);
    this.editContactDraft = {
      contactType: contact.contactType,
      contactDetail: contact.contactDetail,
      isPrimary: contact.isPrimary,
    };
  }

  cancelEditContact(): void {
    this.editingContactId.set(null);
    this.contactError.set('');
  }

  saveEditContact(contact: ContactDto): void {
    if (!this.memberId || !this.editContactDraft.contactDetail.trim()) return;

    this.contactError.set('');
    this.contactActionLoading.set(contact.id);
    this.memberContactService.update(this.memberId, contact.id, { ...this.editContactDraft }).subscribe({
      next: () => {
        this.contactActionLoading.set(null);
        this.editingContactId.set(null);
        this.loadContacts();
      },
      error: (err) => {
        this.contactError.set(err.error?.errors?.contactDetail?.[0] ?? 'Failed to update contact detail');
        this.contactActionLoading.set(null);
      },
    });
  }

  deleteContact(contact: ContactDto): void {
    if (!this.memberId) return;

    this.contactError.set('');
    this.contactActionLoading.set(contact.id);
    this.memberContactService.delete(this.memberId, contact.id).subscribe({
      next: () => {
        this.contactActionLoading.set(null);
        this.loadContacts();
      },
      error: () => {
        this.contactError.set('Failed to delete contact detail');
        this.contactActionLoading.set(null);
      },
    });
  }

  loadUdfFields(): void {
    if (!this.memberId) return;

    this.udfFieldsLoading.set(true);
    this.memberUdfValueService.listForMember(this.memberId).subscribe({
      next: (fields) => {
        this.udfFields.set(fields);
        this.udfFieldsLoading.set(false);
      },
      error: () => {
        this.udfFieldsError.set('Failed to load custom fields');
        this.udfFieldsLoading.set(false);
      },
    });
  }

  updateUdfFieldValue(field: MemberUdfFieldDto, value: string | null): void {
    if (!this.memberId) return;

    this.udfFieldSavingId.set(field.definitionId);
    this.memberUdfValueService.updateForMember(this.memberId, field.definitionId, value).subscribe({
      next: () => {
        this.udfFields.update(fields =>
          fields.map(f => f.definitionId === field.definitionId ? { ...f, value } : f),
        );
        this.udfFieldSavingId.set(null);
      },
      error: () => {
        this.udfFieldsError.set('Failed to save custom field value');
        this.udfFieldSavingId.set(null);
      },
    });
  }

  loadMemberships(): void {
    if (!this.memberId) return;

    this.membershipsLoading.set(true);
    this.membershipService.list(this.memberId).subscribe({
      next: (memberships) => {
        this.memberships.set(memberships);
        this.membershipsLoading.set(false);
      },
      error: () => {
        this.membershipError.set('Failed to load memberships');
        this.membershipsLoading.set(false);
      }
    });
  }

  openAddMembershipModal(): void {
    this.membershipError.set('');
    this.selectedMembershipTypeId = null;
    this.showAddMembershipModal.set(true);
  }

  closeAddMembershipModal(): void {
    this.showAddMembershipModal.set(false);
    this.membershipError.set('');
    this.selectedMembershipTypeId = null;
  }

  addMembership(): void {
    if (!this.memberId || !this.selectedMembershipTypeId) return;

    this.membershipError.set('');
    this.membershipActionLoading.set('create');
    this.membershipService.create(this.memberId, { membershipTypeId: this.selectedMembershipTypeId }).subscribe({
      next: () => {
        this.selectedMembershipTypeId = null;
        this.membershipActionLoading.set(null);
        this.showAddMembershipModal.set(false);
        this.loadMemberships();
      },
      error: (err) => {
        this.membershipError.set(err.error?.errors?.membershipTypeId?.[0] ?? 'Failed to add membership');
        this.membershipActionLoading.set(null);
      }
    });
  }

  requestRenewMembership(membership: MembershipDto): void {
    this.membershipPendingRenewal.set(membership);
  }

  cancelRenewMembership(): void {
    this.membershipPendingRenewal.set(null);
  }

  confirmRenewMembership(): void {
    const membership = this.membershipPendingRenewal();
    if (!this.memberId || !membership) return;

    this.membershipPendingRenewal.set(null);
    this.membershipError.set('');
    this.membershipActionLoading.set(membership.id);
    this.membershipService.renew(this.memberId, membership.id).subscribe({
      next: () => {
        this.membershipActionLoading.set(null);
        this.loadMemberships();
      },
      error: () => {
        this.membershipError.set('Failed to renew membership');
        this.membershipActionLoading.set(null);
      }
    });
  }

  loadTokens(): void {
    if (!this.memberId) return;

    this.tokensLoading.set(true);
    this.memberTokenService.listForMember(this.memberId).subscribe({
      next: (tokens) => {
        this.tokens.set(tokens);
        this.tokensLoading.set(false);
      },
      error: () => {
        this.tokenError.set('Failed to load tokens');
        this.tokensLoading.set(false);
      },
    });
  }

  openAddTokenModal(): void {
    this.tokenError.set('');
    this.newToken = { tokenType: 'Rfid', value: '' };
    this.showAddTokenModal.set(true);
  }

  closeAddTokenModal(): void {
    this.showAddTokenModal.set(false);
    this.tokenError.set('');
  }

  addToken(): void {
    if (!this.memberId || !this.newToken.value.trim()) return;

    this.tokenError.set('');
    this.tokenActionLoading.set('create');
    this.memberTokenService.create(this.memberId, { ...this.newToken }).subscribe({
      next: () => {
        this.tokenActionLoading.set(null);
        this.showAddTokenModal.set(false);
        this.loadTokens();
      },
      error: (err) => {
        this.tokenError.set(err.error?.errors?.value?.[0] ?? 'Failed to add token');
        this.tokenActionLoading.set(null);
      },
    });
  }

  requestRevokeToken(token: TokenDto): void {
    this.tokenPendingRevoke.set(token);
  }

  cancelRevokeToken(): void {
    this.tokenPendingRevoke.set(null);
  }

  confirmRevokeToken(): void {
    const token = this.tokenPendingRevoke();
    if (!this.memberId || !token) return;

    this.tokenPendingRevoke.set(null);
    this.tokenError.set('');
    this.tokenActionLoading.set(token.id);
    this.memberTokenService.revoke(this.memberId, token.id).subscribe({
      next: () => {
        this.tokenActionLoading.set(null);
        this.loadTokens();
      },
      error: () => {
        this.tokenError.set('Failed to revoke token');
        this.tokenActionLoading.set(null);
      },
    });
  }
}
