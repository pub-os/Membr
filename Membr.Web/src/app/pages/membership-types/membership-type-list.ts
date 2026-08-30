import {Component, inject, OnInit, signal} from '@angular/core';
import {MembershipTypeDto, MembershipTypeService} from '@/services/membership-type.service';
import {RouterLink} from '@angular/router';
import {ZardAlertComponent} from '@/shared/components/alert';
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
  selector: 'app-membership-type-list',
  imports: [
    RouterLink,
    ZardAlertComponent,
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
  templateUrl: './membership-type-list.html',
  styleUrl: './membership-type-list.scss',
})
export class MembershipTypeListComponent implements OnInit {
  private membershipTypeService = inject(MembershipTypeService);
  membershipTypes = signal<MembershipTypeDto[]>([]);
  loading = signal(false);
  error = signal('');
  ngOnInit() {
    this.loadMembershipTypes()
  }

  loadMembershipTypes(){
    this.loading.set(true);
    this.membershipTypeService.list().subscribe({
      next: membershipTypes => {
        this.membershipTypes.set(membershipTypes)
        this.loading.set(false)
      },
      error: error => {
        this.loading.set(false);
      }

    })
  }

}