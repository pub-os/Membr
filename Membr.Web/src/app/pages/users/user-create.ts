import { Component, inject, OnInit, signal } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { Router } from '@angular/router';

import { ZardAlertComponent } from '@/shared/components/alert/alert.component';
import { ZardButtonComponent } from '@/shared/components/button/button.component';
import { ZardCardImports } from '@/shared/components/card/card.imports';
import { ZardInputComponent } from '@/shared/components/input/input.component';

import { CreateUserRequest, UserService } from '@/services/user.service';

@Component({
  selector: 'app-user-create',
  imports: [FormsModule, ZardCardImports, ZardInputComponent, ZardButtonComponent, ZardAlertComponent],
  templateUrl: './user-create.html',
})
export class UserCreateComponent implements OnInit {
  private userService = inject(UserService);
  private router = inject(Router);

  loading = signal(false);
  error = signal('');
  roles = signal<string[]>([]);

  email = '';
  displayName = '';
  password = '';
  role = '';

  ngOnInit(): void {
    this.userService.listRoles().subscribe({
      next: roles => {
        this.roles.set(roles);
        if (roles.length) this.role = roles[0];
      },
      error: () => this.error.set('Failed to load roles'),
    });
  }

  createUser(): void {
    if (!this.email.trim() || !this.displayName.trim()) {
      this.error.set('Email and name are required');
      return;
    }

    if (!this.password) {
      this.error.set('Password is required');
      return;
    }

    if (!this.role) {
      this.error.set('A role must be selected');
      return;
    }

    const request: CreateUserRequest = {
      email: this.email.trim(),
      displayName: this.displayName.trim(),
      password: this.password,
      role: this.role,
    };

    this.loading.set(true);
    this.error.set('');

    this.userService.create(request).subscribe({
      next: () => {
        this.loading.set(false);
        this.router.navigate(['/users/list']);
      },
      error: (err) => {
        this.error.set(err.error?.errors?.email?.[0] ?? 'Failed to create user');
        this.loading.set(false);
      }
    });
  }
}
