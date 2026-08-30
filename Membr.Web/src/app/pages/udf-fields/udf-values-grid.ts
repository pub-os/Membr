import { Component, inject, OnInit, signal } from '@angular/core';
import { RouterLink } from '@angular/router';

import { ZardAlertComponent } from '@/shared/components/alert';
import { ZardButtonComponent } from '@/shared/components/button';
import { ZardCardImports } from '@/shared/components/card/card.imports';
import { ZardTableImports } from '@/shared/components/table';
import { UdfValueInputComponent } from '@/shared/components/udf-value/udf-value-input.component';

import { MemberUdfValueService, UdfValuesGridDto } from '@/services/member-udf-value.service';

@Component({
  selector: 'app-udf-values-grid',
  imports: [RouterLink, ZardAlertComponent, ZardButtonComponent, ZardCardImports, ZardTableImports, UdfValueInputComponent],
  templateUrl: './udf-values-grid.html',
})
export class UdfValuesGridComponent implements OnInit {
  private memberUdfValueService = inject(MemberUdfValueService);

  grid = signal<UdfValuesGridDto>({ definitions: [], members: [], values: [] });
  loading = signal(false);
  error = signal('');
  savingKey = signal<string | null>(null);

  private values = new Map<string, string | null>();

  ngOnInit(): void {
    this.load();
  }

  private key(memberId: number | string, definitionId: number | string): string {
    return `${memberId}:${definitionId}`;
  }

  load(): void {
    this.loading.set(true);
    this.memberUdfValueService.listGrid().subscribe({
      next: (grid) => {
        this.grid.set(grid);
        this.values = new Map(grid.values.map(v => [this.key(v.memberId, v.udfDefinitionId), v.value]));
        this.loading.set(false);
      },
      error: () => {
        this.error.set('Failed to load field values');
        this.loading.set(false);
      },
    });
  }

  getValue(memberId: number | string, definitionId: number | string): string | null {
    return this.values.get(this.key(memberId, definitionId)) ?? null;
  }

  setValue(memberId: number | string, definitionId: number | string, value: string | null): void {
    const key = this.key(memberId, definitionId);
    this.values.set(key, value);
    this.savingKey.set(key);
    this.memberUdfValueService.updateForMember(memberId, definitionId, value).subscribe({
      next: () => this.savingKey.set(null),
      error: () => {
        this.error.set('Failed to save value');
        this.savingKey.set(null);
      },
    });
  }
}
