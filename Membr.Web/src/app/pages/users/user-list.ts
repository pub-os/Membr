import {Component, inject, OnInit, signal} from '@angular/core';
import {UserDto, UserService} from '@/services/user.service';
import {RouterLink} from '@angular/router';
import {ZardAlertComponent} from '@/shared/components/alert';
import {ZardBadgeComponent} from '@/shared/components/badge';
import {ZardButtonComponent} from '@/shared/components/button';
import {
  ZardCardActionComponent,
  ZardCardComponent,
  ZardCardContentComponent,
  ZardCardDescriptionComponent, ZardCardHeaderComponent, ZardCardTitleComponent
} from '@/shared/components/card';
import {
  ZardTableBodyComponent,
  ZardTableCellComponent,
  ZardTableComponent,
  ZardTableHeadComponent, ZardTableHeaderComponent, ZardTableRowComponent
} from '@/shared/components/table';

@Component({
  selector: 'app-user-list',
  imports: [
    RouterLink,
    ZardAlertComponent,
    ZardBadgeComponent,
    ZardButtonComponent,
    ZardCardActionComponent,
    ZardCardComponent,
    ZardCardContentComponent,
    ZardCardDescriptionComponent,
    ZardCardHeaderComponent,
    ZardCardTitleComponent,
    ZardTableBodyComponent,
    ZardTableCellComponent,
    ZardTableComponent,
    ZardTableHeadComponent,
    ZardTableHeaderComponent,
    ZardTableRowComponent
  ],
  templateUrl: './user-list.html',
})
export class UserListComponent implements OnInit {
  private userService = inject(UserService);
  users = signal<UserDto[]>([]);
  loading = signal(false);
  error = signal('');

  ngOnInit() {
    this.loadUsers();
  }

  loadUsers() {
    this.loading.set(true);
    this.userService.list().subscribe({
      next: users => {
        this.users.set(users);
        this.loading.set(false);
      },
      error: () => {
        this.error.set('Failed to load users');
        this.loading.set(false);
      }
    });
  }
}
