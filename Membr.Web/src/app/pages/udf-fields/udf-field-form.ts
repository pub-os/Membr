import { Component, computed, inject, OnInit, signal } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { ActivatedRoute, Router, RouterLink } from '@angular/router';

import { ZardAlertComponent } from '@/shared/components/alert';
import { ZardButtonComponent } from '@/shared/components/button';
import { ZardCardImports } from '@/shared/components/card/card.imports';
import { ZardInputComponent } from '@/shared/components/input';
import { UdfValueInputComponent } from '@/shared/components/udf-value/udf-value-input.component';

import { UdfFieldType, UdfFieldService } from '@/services/udf-field.service';

@Component({
  selector: 'app-udf-field-form',
  imports: [FormsModule, RouterLink, ZardAlertComponent, ZardButtonComponent, ZardCardImports, ZardInputComponent, UdfValueInputComponent],
  templateUrl: './udf-field-form.html',
})
export class UdfFieldFormComponent implements OnInit {
  private udfFieldService = inject(UdfFieldService);
  private route = inject(ActivatedRoute);
  private router = inject(Router);

  loading = signal(false);
  saving = signal(false);
  error = signal('');

  definitionId: number | null = null;
  isEditMode = computed(() => this.definitionId !== null);

  name = '';
  type: UdfFieldType = 'String';
  optionsText = '';
  isActive = true;
  defaultValue = signal<string | null>(null);

  readonly types: UdfFieldType[] = ['String', 'Bool', 'Date', 'DateTime', 'MultiSelect'];

  options = computed<string[]>(() =>
    this.optionsText.split(',').map(o => o.trim()).filter(o => o.length > 0),
  );

  ngOnInit(): void {
    const idParam = this.route.snapshot.paramMap.get('id');
    if (!idParam) return;

    this.definitionId = Number(idParam);
    this.loading.set(true);
    this.udfFieldService.get(this.definitionId).subscribe({
      next: (definition) => {
        this.name = definition.name;
        this.type = definition.type;
        this.optionsText = definition.options.join(', ');
        this.isActive = definition.isActive;
        this.defaultValue.set(definition.defaultValue ?? null);
        this.loading.set(false);
      },
      error: () => {
        this.error.set('Failed to load field');
        this.loading.set(false);
      },
    });
  }

  save(): void {
    if (!this.name.trim()) {
      this.error.set('Name is required');
      return;
    }

    this.saving.set(true);
    this.error.set('');

    const request$ = this.isEditMode()
      ? this.udfFieldService.update(this.definitionId!, {
          name: this.name,
          options: this.type === 'MultiSelect' ? this.options() : [],
          defaultValue: this.defaultValue(),
          isActive: this.isActive,
        })
      : this.udfFieldService.create({
          name: this.name,
          type: this.type,
          options: this.type === 'MultiSelect' ? this.options() : [],
          defaultValue: this.defaultValue(),
        });

    request$.subscribe({
      next: () => {
        this.saving.set(false);
        this.router.navigate(['/settings/udf-fields']);
      },
      error: (err) => {
        this.error.set(err.error?.errors?.name?.[0] ?? 'Failed to save field');
        this.saving.set(false);
      },
    });
  }
}
