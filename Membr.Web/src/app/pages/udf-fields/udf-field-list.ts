import { Component, inject, OnInit, signal } from '@angular/core';
import { RouterLink } from '@angular/router';

import { ZardAlertComponent } from '@/shared/components/alert';
import { ZardBadgeComponent } from '@/shared/components/badge';
import { ZardButtonComponent } from '@/shared/components/button';
import { ZardCardImports } from '@/shared/components/card/card.imports';
import { ZardTableImports } from '@/shared/components/table';

import { UdfDefinitionDto, UdfFieldService } from '@/services/udf-field.service';

@Component({
  selector: 'app-udf-field-list',
  imports: [RouterLink, ZardAlertComponent, ZardBadgeComponent, ZardButtonComponent, ZardCardImports, ZardTableImports],
  templateUrl: './udf-field-list.html',
})
export class UdfFieldListComponent implements OnInit {
  private udfFieldService = inject(UdfFieldService);

  definitions = signal<UdfDefinitionDto[]>([]);
  loading = signal(false);
  error = signal('');
  actionLoadingId = signal<UdfDefinitionDto['id'] | null>(null);

  ngOnInit(): void {
    this.loadDefinitions();
  }

  loadDefinitions(): void {
    this.loading.set(true);
    this.udfFieldService.list().subscribe({
      next: (definitions) => {
        this.definitions.set(definitions);
        this.loading.set(false);
      },
      error: () => {
        this.error.set('Failed to load user-defined fields');
        this.loading.set(false);
      },
    });
  }

  toggleActive(definition: UdfDefinitionDto): void {
    this.actionLoadingId.set(definition.id);
    this.udfFieldService.update(definition.id, {
      name: definition.name,
      options: definition.options,
      defaultValue: definition.defaultValue,
      isActive: !definition.isActive,
    }).subscribe({
      next: () => {
        this.actionLoadingId.set(null);
        this.loadDefinitions();
      },
      error: () => {
        this.error.set('Failed to update field');
        this.actionLoadingId.set(null);
      },
    });
  }

  applyDefault(definition: UdfDefinitionDto): void {
    if (!confirm(`Apply the default value of "${definition.name}" to every member? This overwrites any values already set.`)) {
      return;
    }

    this.actionLoadingId.set(definition.id);
    this.udfFieldService.applyDefault(definition.id).subscribe({
      next: () => this.actionLoadingId.set(null),
      error: () => {
        this.error.set('Failed to apply default value');
        this.actionLoadingId.set(null);
      },
    });
  }

  deleteDefinition(definition: UdfDefinitionDto): void {
    if (!confirm(`Delete the "${definition.name}" field? This removes it and every member's value for it.`)) {
      return;
    }

    this.actionLoadingId.set(definition.id);
    this.udfFieldService.delete(definition.id).subscribe({
      next: () => {
        this.actionLoadingId.set(null);
        this.loadDefinitions();
      },
      error: () => {
        this.error.set('Failed to delete field');
        this.actionLoadingId.set(null);
      },
    });
  }
}
