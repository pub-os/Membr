import { ChangeDetectionStrategy, Component, computed, input, model } from '@angular/core';
import { FormsModule } from '@angular/forms';

import { ZardInputComponent } from '@/shared/components/input/input.component';
import { UdfFieldType } from '@/services/udf-field.service';

@Component({
  selector: 'app-udf-value-input',
  imports: [FormsModule, ZardInputComponent],
  changeDetection: ChangeDetectionStrategy.OnPush,
  template: `
    @switch (type()) {
      @case ('Bool') {
        <input type="checkbox" class="h-4 w-4" [ngModel]="boolValue()" (ngModelChange)="setBool($event)" />
      }
      @case ('Date') {
        <input z-input type="date" [ngModel]="value()" (ngModelChange)="value.set($event || null)" />
      }
      @case ('DateTime') {
        <input z-input type="datetime-local" [ngModel]="value()" (ngModelChange)="value.set($event || null)" />
      }
      @case ('MultiSelect') {
        <div class="flex flex-wrap gap-3">
          @for (option of options(); track option) {
            <label class="flex items-center gap-1.5 text-sm">
              <input type="checkbox" class="h-4 w-4" [ngModel]="isSelected(option)" (ngModelChange)="toggleOption(option, $event)" />
              {{ option }}
            </label>
          }
        </div>
      }
      @default {
        <input z-input type="text" [ngModel]="value()" (ngModelChange)="value.set($event || null)" />
      }
    }
  `,
})
export class UdfValueInputComponent {
  type = input.required<UdfFieldType>();
  options = input<string[]>([]);
  value = model<string | null>(null);

  boolValue = computed(() => this.value() === 'true');

  setBool(checked: boolean): void {
    this.value.set(checked ? 'true' : 'false');
  }

  private selectedOptions = computed<string[]>(() => {
    if (!this.value()) return [];
    try {
      const parsed = JSON.parse(this.value()!);
      return Array.isArray(parsed) ? parsed : [];
    } catch {
      return [];
    }
  });

  isSelected(option: string): boolean {
    return this.selectedOptions().includes(option);
  }

  toggleOption(option: string, checked: boolean): void {
    const current = this.selectedOptions();
    const next = checked ? [...current, option] : current.filter(o => o !== option);
    this.value.set(JSON.stringify(next));
  }
}
