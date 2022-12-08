/* tslint:disable */
/* eslint-disable */
import { Injectable } from '@angular/core';
import { HttpClient, HttpResponse } from '@angular/common/http';
import { BaseService } from '../base-service';
import { ApiConfiguration } from '../api-configuration';
import { StrictHttpResponse } from '../strict-http-response';
import { RequestBuilder } from '../request-builder';
import { Observable } from 'rxjs';
import { map, filter } from 'rxjs/operators';

import { AddDeviceViewModel } from '../models/add-device-view-model';
import { DeviceConfigurationViewModel } from '../models/device-configuration-view-model';
import { StatusViewModel } from '../models/status-view-model';

@Injectable({
  providedIn: 'root',
})
export class DeviceService extends BaseService {
  constructor(
    config: ApiConfiguration,
    http: HttpClient
  ) {
    super(config, http);
  }

  /**
   * Path part for operation apiV1DeviceGet
   */
  static readonly ApiV1DeviceGetPath = '/api/v1/Device';

  /**
   * This method provides access to the full `HttpResponse`, allowing access to response headers.
   * To access only the response body, use `apiV1DeviceGet$Plain()` instead.
   *
   * This method doesn't expect any request body.
   */
  apiV1DeviceGet$Plain$Response(params?: {
  }): Observable<StrictHttpResponse<Array<DeviceConfigurationViewModel>>> {

    const rb = new RequestBuilder(this.rootUrl, DeviceService.ApiV1DeviceGetPath, 'get');
    if (params) {
    }

    return this.http.request(rb.build({
      responseType: 'text',
      accept: 'text/plain'
    })).pipe(
      filter((r: any) => r instanceof HttpResponse),
      map((r: HttpResponse<any>) => {
        return r as StrictHttpResponse<Array<DeviceConfigurationViewModel>>;
      })
    );
  }

  /**
   * This method provides access to only to the response body.
   * To access the full response (for headers, for example), `apiV1DeviceGet$Plain$Response()` instead.
   *
   * This method doesn't expect any request body.
   */
  apiV1DeviceGet$Plain(params?: {
  }): Observable<Array<DeviceConfigurationViewModel>> {

    return this.apiV1DeviceGet$Plain$Response(params).pipe(
      map((r: StrictHttpResponse<Array<DeviceConfigurationViewModel>>) => r.body as Array<DeviceConfigurationViewModel>)
    );
  }

  /**
   * This method provides access to the full `HttpResponse`, allowing access to response headers.
   * To access only the response body, use `apiV1DeviceGet$Json()` instead.
   *
   * This method doesn't expect any request body.
   */
  apiV1DeviceGet$Json$Response(params?: {
  }): Observable<StrictHttpResponse<Array<DeviceConfigurationViewModel>>> {

    const rb = new RequestBuilder(this.rootUrl, DeviceService.ApiV1DeviceGetPath, 'get');
    if (params) {
    }

    return this.http.request(rb.build({
      responseType: 'json',
      accept: 'text/json'
    })).pipe(
      filter((r: any) => r instanceof HttpResponse),
      map((r: HttpResponse<any>) => {
        return r as StrictHttpResponse<Array<DeviceConfigurationViewModel>>;
      })
    );
  }

  /**
   * This method provides access to only to the response body.
   * To access the full response (for headers, for example), `apiV1DeviceGet$Json$Response()` instead.
   *
   * This method doesn't expect any request body.
   */
  apiV1DeviceGet$Json(params?: {
  }): Observable<Array<DeviceConfigurationViewModel>> {

    return this.apiV1DeviceGet$Json$Response(params).pipe(
      map((r: StrictHttpResponse<Array<DeviceConfigurationViewModel>>) => r.body as Array<DeviceConfigurationViewModel>)
    );
  }

  /**
   * Path part for operation apiV1DevicePost
   */
  static readonly ApiV1DevicePostPath = '/api/v1/Device';

  /**
   * This method provides access to the full `HttpResponse`, allowing access to response headers.
   * To access only the response body, use `apiV1DevicePost$Plain()` instead.
   *
   * This method sends `application/*+json` and handles request body of type `application/*+json`.
   */
  apiV1DevicePost$Plain$Response(params?: {
    body?: Array<AddDeviceViewModel>
  }): Observable<StrictHttpResponse<Array<AddDeviceViewModel>>> {

    const rb = new RequestBuilder(this.rootUrl, DeviceService.ApiV1DevicePostPath, 'post');
    if (params) {
      rb.body(params.body, 'application/*+json');
    }

    return this.http.request(rb.build({
      responseType: 'text',
      accept: 'text/plain'
    })).pipe(
      filter((r: any) => r instanceof HttpResponse),
      map((r: HttpResponse<any>) => {
        return r as StrictHttpResponse<Array<AddDeviceViewModel>>;
      })
    );
  }

  /**
   * This method provides access to only to the response body.
   * To access the full response (for headers, for example), `apiV1DevicePost$Plain$Response()` instead.
   *
   * This method sends `application/*+json` and handles request body of type `application/*+json`.
   */
  apiV1DevicePost$Plain(params?: {
    body?: Array<AddDeviceViewModel>
  }): Observable<Array<AddDeviceViewModel>> {

    return this.apiV1DevicePost$Plain$Response(params).pipe(
      map((r: StrictHttpResponse<Array<AddDeviceViewModel>>) => r.body as Array<AddDeviceViewModel>)
    );
  }

  /**
   * This method provides access to the full `HttpResponse`, allowing access to response headers.
   * To access only the response body, use `apiV1DevicePost$Json()` instead.
   *
   * This method sends `application/*+json` and handles request body of type `application/*+json`.
   */
  apiV1DevicePost$Json$Response(params?: {
    body?: Array<AddDeviceViewModel>
  }): Observable<StrictHttpResponse<Array<AddDeviceViewModel>>> {

    const rb = new RequestBuilder(this.rootUrl, DeviceService.ApiV1DevicePostPath, 'post');
    if (params) {
      rb.body(params.body, 'application/*+json');
    }

    return this.http.request(rb.build({
      responseType: 'json',
      accept: 'text/json'
    })).pipe(
      filter((r: any) => r instanceof HttpResponse),
      map((r: HttpResponse<any>) => {
        return r as StrictHttpResponse<Array<AddDeviceViewModel>>;
      })
    );
  }

  /**
   * This method provides access to only to the response body.
   * To access the full response (for headers, for example), `apiV1DevicePost$Json$Response()` instead.
   *
   * This method sends `application/*+json` and handles request body of type `application/*+json`.
   */
  apiV1DevicePost$Json(params?: {
    body?: Array<AddDeviceViewModel>
  }): Observable<Array<AddDeviceViewModel>> {

    return this.apiV1DevicePost$Json$Response(params).pipe(
      map((r: StrictHttpResponse<Array<AddDeviceViewModel>>) => r.body as Array<AddDeviceViewModel>)
    );
  }

  /**
   * Path part for operation apiV1DeviceIdStatusGet
   */
  static readonly ApiV1DeviceIdStatusGetPath = '/api/v1/Device/{id}/status';

  /**
   * This method provides access to the full `HttpResponse`, allowing access to response headers.
   * To access only the response body, use `apiV1DeviceIdStatusGet$Plain()` instead.
   *
   * This method doesn't expect any request body.
   */
  apiV1DeviceIdStatusGet$Plain$Response(params: {
    id: number;
  }): Observable<StrictHttpResponse<StatusViewModel>> {

    const rb = new RequestBuilder(this.rootUrl, DeviceService.ApiV1DeviceIdStatusGetPath, 'get');
    if (params) {
      rb.path('id', params.id, {});
    }

    return this.http.request(rb.build({
      responseType: 'text',
      accept: 'text/plain'
    })).pipe(
      filter((r: any) => r instanceof HttpResponse),
      map((r: HttpResponse<any>) => {
        return r as StrictHttpResponse<StatusViewModel>;
      })
    );
  }

  /**
   * This method provides access to only to the response body.
   * To access the full response (for headers, for example), `apiV1DeviceIdStatusGet$Plain$Response()` instead.
   *
   * This method doesn't expect any request body.
   */
  apiV1DeviceIdStatusGet$Plain(params: {
    id: number;
  }): Observable<StatusViewModel> {

    return this.apiV1DeviceIdStatusGet$Plain$Response(params).pipe(
      map((r: StrictHttpResponse<StatusViewModel>) => r.body as StatusViewModel)
    );
  }

  /**
   * This method provides access to the full `HttpResponse`, allowing access to response headers.
   * To access only the response body, use `apiV1DeviceIdStatusGet$Json()` instead.
   *
   * This method doesn't expect any request body.
   */
  apiV1DeviceIdStatusGet$Json$Response(params: {
    id: number;
  }): Observable<StrictHttpResponse<StatusViewModel>> {

    const rb = new RequestBuilder(this.rootUrl, DeviceService.ApiV1DeviceIdStatusGetPath, 'get');
    if (params) {
      rb.path('id', params.id, {});
    }

    return this.http.request(rb.build({
      responseType: 'json',
      accept: 'text/json'
    })).pipe(
      filter((r: any) => r instanceof HttpResponse),
      map((r: HttpResponse<any>) => {
        return r as StrictHttpResponse<StatusViewModel>;
      })
    );
  }

  /**
   * This method provides access to only to the response body.
   * To access the full response (for headers, for example), `apiV1DeviceIdStatusGet$Json$Response()` instead.
   *
   * This method doesn't expect any request body.
   */
  apiV1DeviceIdStatusGet$Json(params: {
    id: number;
  }): Observable<StatusViewModel> {

    return this.apiV1DeviceIdStatusGet$Json$Response(params).pipe(
      map((r: StrictHttpResponse<StatusViewModel>) => r.body as StatusViewModel)
    );
  }

}
