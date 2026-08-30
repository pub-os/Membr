import { DatePipe } from '@angular/common';
import { Component, computed, inject, signal, OnInit } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { ActivatedRoute, RouterLink } from '@angular/router';

import { ZardAlertComponent } from '@/shared/components/alert/alert.component';
import { ZardBadgeComponent } from '@/shared/components/badge';
import { ZardButtonComponent } from '@/shared/components/button/button.component';
import { ZardCardImports } from '@/shared/components/card/card.imports';
import { ZardTableImports } from '@/shared/components/table/table.imports';

import { MemberDto, MemberService } from '@/services/member.service';
import { MembershipDto, MembershipService } from '@/services/membership.service';
import { MembershipTypeDto, MembershipTypeService } from '@/services/membership-type.service';
import { MemberUdfFieldDto, MemberUdfValueService } from '@/services/member-udf-value.service';
import { UdfValueInputComponent } from '@/shared/components/udf-value/udf-value-input.component';

@Component({
  selector: 'app-member-overview',
  templateUrl: './member-overview.html',
  imports: [
    RouterLink, FormsModule, DatePipe, ZardCardImports, ZardTableImports, ZardButtonComponent, ZardAlertComponent,
    ZardBadgeComponent, UdfValueInputComponent,
  ],
})
export class MemberOverviewComponent implements OnInit {
  private memberService = inject(MemberService);
  private membershipService = inject(MembershipService);
  private membershipTypeService = inject(MembershipTypeService);
  private memberUdfValueService = inject(MemberUdfValueService);
  private route = inject(ActivatedRoute);

  member = signal<MemberDto | null>(null);
  memberships = signal<MembershipDto[]>([]);
  membershipTypes = signal<MembershipTypeDto[]>([]);
  udfFields = signal<MemberUdfFieldDto[]>([]);

  loading = signal(false);
  membershipsLoading = signal(false);
  udfFieldsLoading = signal(false);
  error = signal('');
  membershipError = signal('');
  udfFieldsError = signal('');
  membershipActionLoading = signal<MembershipDto['id'] | 'create' | null>(null);
  udfFieldSavingId = signal<MemberUdfFieldDto['definitionId'] | null>(null);

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
    this.membershipTypeService.list().subscribe({
      next: (types) => this.membershipTypes.set(types.filter(t => t.isActive)),
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
}
