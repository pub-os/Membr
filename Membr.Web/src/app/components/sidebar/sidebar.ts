import { ChangeDetectionStrategy, Component, computed, inject } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { Router, RouterLink, RouterLinkActive } from '@angular/router';

import { NgIcon, provideIcons } from '@ng-icons/core';
import {
  lucideGalleryVerticalEnd,
  lucideLayoutDashboard,
  lucideListChecks,
  lucideLogOut,
  lucidePlus,
  lucideSearch,
  lucideSettings,
  lucideTags,
  lucideUserPlus,
  lucideUsers,
} from '@ng-icons/lucide';

import { ZardAvatarComponent } from '@/shared/components/avatar/avatar.component';
import { ZardDropdownImports } from '@/shared/components/dropdown';
import { ZardInputGroupImports } from '@/shared/components/input-group';
import { ZardInputComponent } from '@/shared/components/input/input.component';
import { ZardSeparatorComponent } from '@/shared/components/separator/separator.component';
import { ZardSidebarImports } from '@/shared/components/sidebar/sidebar.imports';
import { AuthService } from '@/services/auth.service';

interface NavItem {
  readonly title: string;
  readonly icon: string;
  readonly link: string;
}

interface NavGroup {
  readonly title: string;
  readonly items: readonly NavItem[];
}

@Component({
  selector: 'app-sidebar',
  imports: [
    ZardSidebarImports,
    ZardSeparatorComponent,
    ZardAvatarComponent,
    ZardDropdownImports,
    ZardInputGroupImports,
    ZardInputComponent,
    NgIcon,
    RouterLink,
    RouterLinkActive,
    FormsModule,
  ],
  templateUrl: './sidebar.html',
  changeDetection: ChangeDetectionStrategy.OnPush,
  viewProviders: [
    provideIcons({
      lucideGalleryVerticalEnd,
      lucideLayoutDashboard,
      lucideListChecks,
      lucideLogOut,
      lucidePlus,
      lucideSearch,
      lucideSettings,
      lucideTags,
      lucideUserPlus,
      lucideUsers,
    }),
  ],
})
export class Sidebar {
  private readonly router = inject(Router);
  protected readonly auth = inject(AuthService);

  private readonly baseNavGroups: readonly NavGroup[] = [
    {
      title: 'Overview',
      items: [{ title: 'Dashboard', icon: 'lucideLayoutDashboard', link: '/dashboard' }],
    },
    {
      title: 'Members',
      items: [
        { title: 'All Members', icon: 'lucideUsers', link: '/member/list' },
        { title: 'Add Member', icon: 'lucideUserPlus', link: '/member/create' },
      ],
    },
  ];

  private readonly adminNavGroups: readonly NavGroup[] = [
    {
      title: 'Membership Types',
      items: [
        { title: 'All Types', icon: 'lucideTags', link: '/membershiptype/list' },
        { title: 'Add Type', icon: 'lucidePlus', link: '/membershiptype/create' },
      ],
    },
    {
      title: 'Users',
      items: [
        { title: 'All Users', icon: 'lucideUsers', link: '/users/list' },
        { title: 'Add User', icon: 'lucideUserPlus', link: '/users/create' },
      ],
    },
    {
      title: 'Settings',
      items: [
        { title: 'Memberships', icon: 'lucideSettings', link: '/settings/memberships' },
        { title: 'Custom Fields', icon: 'lucideListChecks', link: '/settings/udf-fields' },
      ],
    },
  ];

  readonly navGroups = computed<readonly NavGroup[]>(() =>
    this.auth.isAdmin() ? [...this.baseNavGroups, ...this.adminNavGroups] : this.baseNavGroups,
  );

  searchQuery = '';

  protected readonly userInitials = computed(() => {
    const name = this.auth.user()?.displayName.trim();
    if (!name) return '?';
    const parts = name.split(/\s+/);
    return parts.length > 1 ? `${parts[0][0]}${parts[1][0]}`.toUpperCase() : name.slice(0, 2).toUpperCase();
  });

  search(): void {
    const query = this.searchQuery.trim();
    if (!query) return;
    this.router.navigate(['/member/list'], { queryParams: { q: query } });
  }

  logout(): void {
    this.auth.logout().subscribe(() => this.router.navigate(['/login']));
  }
}
