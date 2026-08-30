import { Component, inject, signal, OnInit } from '@angular/core';
import { ActivatedRoute, RouterLink } from '@angular/router';

import { ZardAlertComponent } from '@/shared/components/alert/alert.component';
import { ZardButtonComponent } from '@/shared/components/button/button.component';
import { ZardCardImports } from '@/shared/components/card/card.imports';
import { ZardTableImports } from '@/shared/components/table/table.imports';

import { MemberDto, MemberService } from '@/services/member.service';

const PAGE_SIZE = 25;

@Component({
  selector: 'app-member-list',
  templateUrl: './member-list.html',
  imports: [RouterLink, ZardCardImports, ZardTableImports, ZardButtonComponent, ZardAlertComponent],
})
export class MemberListComponent implements OnInit {
  private memberService = inject(MemberService);
  private route = inject(ActivatedRoute);

  members = signal<MemberDto[]>([]);
  loading = signal(false);
  error = signal('');

  page = signal(1);
  totalCount = signal(0);
  readonly pageSize = PAGE_SIZE;

  private currentQuery = '';

  totalPages(): number {
    return Math.max(1, Math.ceil(this.totalCount() / this.pageSize));
  }

  ngOnInit(): void {
    this.route.queryParams.subscribe(params => {
      this.currentQuery = params['q'] ?? '';
      this.page.set(1);
      this.load();
    });
  }

  goToPage(page: number): void {
    if (page < 1 || page > this.totalPages()) return;
    this.page.set(page);
    this.load();
  }

  private load(): void {
    if (this.currentQuery) {
      this.searchMembers(this.currentQuery);
    } else {
      this.listMembers();
    }
  }

  listMembers(): void {
    this.loading.set(true);

    this.memberService.list(this.page(), this.pageSize).subscribe({
      next: (result) => {
        this.members.set(result.items);
        this.totalCount.set(Number(result.totalCount));
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

    this.memberService.search(query, this.page(), this.pageSize).subscribe({
      next: (result) => {
        this.members.set(result.items);
        this.totalCount.set(Number(result.totalCount));
        this.loading.set(false);
      },
      error: () => {
        this.error.set('Failed to search members');
        this.loading.set(false);
      }
    });
  }
}
