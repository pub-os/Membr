import { Routes } from '@angular/router';
import { authGuard } from '@/guards/auth.guard';
import { adminGuard } from '@/guards/admin.guard';

export const routes: Routes = [
  { path: '', redirectTo: 'dashboard', pathMatch: 'full' },
  {
    path: 'login',
    loadComponent: () => import('./pages/auth/login').then(m => m.LoginComponent),
  },
  {
    path: 'dashboard',
    canActivate: [authGuard],
    loadComponent: () => import('./pages/dashboard/dashboard').then(m => m.DashboardComponent),
  },
  {
    path: 'members',
    canActivate: [authGuard],
    loadComponent: () => import('./pages/members/members').then(m => m.MembersComponent),
  },
  {
    path: 'member/list',
    canActivate: [authGuard],
    loadComponent: () => import('./pages/members/member-list').then(m => m.MemberListComponent),
  },
  {
    path: 'member/create',
    canActivate: [authGuard],
    loadComponent: () => import('./pages/members/member-create').then(m => m.CreateMembersComponent),
  },
  {
    path: 'member/:id',
    canActivate: [authGuard],
    loadComponent: () => import('./pages/members/member-overview').then(m => m.MemberOverviewComponent),
  },
  {
    path: 'membershiptype/list',
    canActivate: [authGuard, adminGuard],
    loadComponent: () => import('./pages/membership-types/membership-type-list').then(m => m.MembershipTypeListComponent),
  },
  {
    path: 'membershiptype/create',
    canActivate: [authGuard, adminGuard],
    loadComponent: () => import('./pages/membership-types/membership-type-create').then(m => m.MembershipTypeCreateComponent),
  },
  {
    path: 'settings/memberships',
    canActivate: [authGuard, adminGuard],
    loadComponent: () => import('./pages/settings/membership-settings').then(m => m.MembershipSettingsComponent),
  },
  {
    path: 'settings/udf-fields',
    canActivate: [authGuard, adminGuard],
    loadComponent: () => import('./pages/udf-fields/udf-field-list').then(m => m.UdfFieldListComponent),
  },
  {
    path: 'settings/udf-fields/create',
    canActivate: [authGuard, adminGuard],
    loadComponent: () => import('./pages/udf-fields/udf-field-form').then(m => m.UdfFieldFormComponent),
  },
  {
    path: 'settings/udf-fields/values',
    canActivate: [authGuard, adminGuard],
    loadComponent: () => import('./pages/udf-fields/udf-values-grid').then(m => m.UdfValuesGridComponent),
  },
  {
    path: 'settings/udf-fields/:id/edit',
    canActivate: [authGuard, adminGuard],
    loadComponent: () => import('./pages/udf-fields/udf-field-form').then(m => m.UdfFieldFormComponent),
  },
  {
    path: 'users/list',
    canActivate: [authGuard, adminGuard],
    loadComponent: () => import('./pages/users/user-list').then(m => m.UserListComponent),
  },
  {
    path: 'users/create',
    canActivate: [authGuard, adminGuard],
    loadComponent: () => import('./pages/users/user-create').then(m => m.UserCreateComponent),
  },
];