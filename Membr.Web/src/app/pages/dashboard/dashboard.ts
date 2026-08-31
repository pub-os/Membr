import { Component, OnInit, computed, inject, signal } from '@angular/core';
import { Color, LegendPosition, NgxChartsModule, ScaleType } from '@swimlane/ngx-charts';

import { ZardAlertComponent } from '@/shared/components/alert/alert.component';
import { ZardCardImports } from '@/shared/components/card/card.imports';

import { DashboardService, DashboardStatsDto } from '@/services/dashboard.service';

const MONTH_LABELS = [
  'Jan', 'Feb', 'Mar', 'Apr', 'May', 'Jun', 'Jul', 'Aug', 'Sep', 'Oct', 'Nov', 'Dec',
] as const;

interface CategoryDatum {
  readonly name: string;
  readonly value: number;
}

interface SeriesDatum {
  readonly name: string;
  readonly series: CategoryDatum[];
}

@Component({
  selector: 'app-dashboard',
  templateUrl: './dashboard.html',
  styleUrl: './dashboard.scss',
  imports: [ZardCardImports, ZardAlertComponent, NgxChartsModule],
})
export class DashboardComponent implements OnInit {
  private dashboardService = inject(DashboardService);

  readonly legendPosition = LegendPosition.Below;

  readonly breakdownScheme: Color = {
    name: 'membershipTypeBreakdown',
    selectable: false,
    group: ScaleType.Ordinal,
    domain: ['#0F6E56'],
  };

  readonly activityScheme: Color = {
    name: 'monthlyActivity',
    selectable: true,
    group: ScaleType.Ordinal,
    domain: ['#0F6E56', '#9AD9C4'],
  };

  stats = signal<DashboardStatsDto | null>(null);
  loading = signal(false);
  error = signal('');

  readonly totalMembers = computed(() => this.stats()?.totalMembers ?? 0);
  readonly activeMembers = computed(() => this.stats()?.activeMembers ?? 0);

  readonly membershipTypeBreakdown = computed<CategoryDatum[]>(() =>
    (this.stats()?.membershipTypeBreakdown ?? []).map(t => ({
      name: t.membershipTypeName,
      value: Number(t.activeCount),
    })),
  );

  readonly monthlyActivity = computed<SeriesDatum[]>(() => {
    const activity = this.stats()?.monthlyActivity ?? [];
    return activity.map(m => ({
      name: `${MONTH_LABELS[Number(m.month) - 1]} ${String(m.year).slice(2)}`,
      series: [
        { name: 'New memberships', value: Number(m.newMemberships) },
        { name: 'Renewals', value: Number(m.renewals) },
      ],
    }));
  });

  ngOnInit(): void {
    this.loading.set(true);

    this.dashboardService.getStats().subscribe({
      next: stats => {
        this.stats.set(stats);
        this.loading.set(false);
      },
      error: () => {
        this.error.set('Failed to load dashboard');
        this.loading.set(false);
      },
    });
  }
}
