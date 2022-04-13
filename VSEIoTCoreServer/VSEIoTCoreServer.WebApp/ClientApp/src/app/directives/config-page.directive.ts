import { Directive, OnDestroy, OnInit } from '@angular/core';
import { AbstractControl, FormControl, FormGroup, FormGroupDirective, NgForm } from '@angular/forms';
import { ErrorStateMatcher } from '@angular/material/core';
import { Subject } from 'rxjs';
import { DeviceConfigurationUI } from '../models/device-configuration';

/** Error when invalid control is dirty, touched, or submitted. */
export class MyErrorStateMatcher implements ErrorStateMatcher {
  isErrorState(control: FormControl | null, form: FormGroupDirective | NgForm | null): boolean {
    const isSubmitted = form && form.submitted;
    return !!(control && control.invalid && (control.dirty || control.touched || isSubmitted));
  }
}

@Directive()
export abstract class ConfigPageDirective implements OnInit, OnDestroy {
  public abstract formModel: FormModel<Partial<DeviceConfigurationUI>>;
  public abstract form: FormGroup;
  public matcher = new MyErrorStateMatcher();
  protected readonly destroyed$ = new Subject();

  public abstract ngOnInit(): void;

  public ngOnDestroy(): void {
    this.destroyed$.next();
    this.destroyed$.complete();
  }

  public abstract updateChanges(): void;
}

export type FormModel<T> = {
  [key in keyof T]: AbstractControl;
};
