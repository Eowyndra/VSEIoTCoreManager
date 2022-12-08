import { Component, Inject } from '@angular/core';
import { MAT_SNACK_BAR_DATA } from '@angular/material/snack-bar';
import { NotificationType } from 'src/app/enums/notification-type';
import { NotificationMessage } from 'src/app/models/notification-message';

@Component({
  selector: 'app-notification',
  templateUrl: './notification.component.html',
  styleUrls: ['./notification.component.scss']
})
export class NotificationComponent {
  public type = NotificationType;

  constructor(@Inject(MAT_SNACK_BAR_DATA) public data: { type: NotificationType; title: string; message: NotificationMessage[] }) {}
}

