import { interval, Observable, Subject } from 'rxjs';
import { map, startWith, takeUntil } from 'rxjs/operators';

import { Overlay, OverlayRef, PositionStrategy } from '@angular/cdk/overlay';
import { ComponentPortal } from '@angular/cdk/portal';
import { Component, ElementRef, OnDestroy, ViewChild } from '@angular/core';
import { Router } from '@angular/router';

import { HelpCentreComponent } from './help-centre/help-centre.component';

@Component({
  selector: 'app-header',
  templateUrl: './header.component.html',
  styleUrls: ['./header.component.scss'],
})
export class HeaderComponent implements OnDestroy {
  @ViewChild('helpButton', { read: ElementRef, static: true })
  helpButtonRef!: ElementRef;
  currentTime$: Observable<Date> = interval(1000).pipe(
    map(() => new Date()),
    startWith(new Date())
  );
  private helpDialogOverlayRef: OverlayRef | undefined;
  private helpCentrePortal: ComponentPortal<HelpCentreComponent> | undefined;
  private readonly destroyed$ = new Subject();

  constructor(private readonly overlay: Overlay, private router: Router) {}

  ngOnDestroy(): void {
    this.destroyed$.next();
    this.destroyed$.complete();
  }

  openHelpDialog(): void {
    if (!this.helpDialogOverlayRef) {
      this.helpDialogOverlayRef = this.initHelpCentreDialog();
    }
    if (!this.helpCentrePortal) {
      this.helpCentrePortal = new ComponentPortal(HelpCentreComponent);
    }
    if (!this.helpDialogOverlayRef.hasAttached()) {
      this.helpDialogOverlayRef.attach(this.helpCentrePortal);
    } else {
      this.helpDialogOverlayRef.detach();
    }
  }

  public logout(): void {
    //Complete when Login/Auth is implemented
  }

  private initHelpCentreDialog(): OverlayRef {
    const overlayPosition: PositionStrategy = this.overlay
      .position()
      .connectedTo(this.helpButtonRef, { originX: 'center', originY: 'bottom' }, { overlayX: 'center', overlayY: 'top' });

    const overlayRef = this.overlay.create({
      positionStrategy: overlayPosition,
      hasBackdrop: true,
      backdropClass: 'cdk-overlay-transparent-backdrop',
    });

    overlayRef
      .backdropClick()
      .pipe(takeUntil(this.destroyed$))
      .subscribe(() => {
        if (overlayRef && overlayRef.hasAttached()) {
          overlayRef.detach();
        }
      });

    return overlayRef;
  }
}
