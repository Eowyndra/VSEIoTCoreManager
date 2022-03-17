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

import { DeviceConfigurationViewModel } from '../models/device-configuration-view-model';
import { IStatus } from '../models/i-status';

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
   * Path part for operation apiV1DeviceDeviceIdStatusGet
   */
  static readonly ApiV1DeviceDeviceIdStatusGetPath = '/api/v1/Device/device/{id}/status';

  /**
   * This method provides access to the full `HttpResponse`, allowing access to response headers.
   * To access only the response body, use `apiV1DeviceDeviceIdStatusGet$Plain()` instead.
   *
   * This method doesn't expect any request body.
   */
  apiV1DeviceDeviceIdStatusGet$Plain$Response(params: {
    id: number;
  }): Observable<StrictHttpResponse<IStatus>> {

    const rb = new RequestBuilder(this.rootUrl, DeviceService.ApiV1DeviceDeviceIdStatusGetPath, 'get');
    if (params) {
      rb.path('id', params.id, {});
    }

    return this.http.request(rb.build({
      responseType: 'text',
      accept: 'text/plain'
    })).pipe(
      filter((r: any) => r instanceof HttpResponse),
      map((r: HttpResponse<any>) => {
        return r as StrictHttpResponse<IStatus>;
      })
    );
  }

  /**
   * This method provides access to only to the response body.
   * To access the full response (for headers, for example), `apiV1DeviceDeviceIdStatusGet$Plain$Response()` instead.
   *
   * This method doesn't expect any request body.
   */
  apiV1DeviceDeviceIdStatusGet$Plain(params: {
    id: number;
  }): Observable<IStatus> {

    return this.apiV1DeviceDeviceIdStatusGet$Plain$Response(params).pipe(
      map((r: StrictHttpResponse<IStatus>) => r.body as IStatus)
    );
  }

  /**
   * This method provides access to the full `HttpResponse`, allowing access to response headers.
   * To access only the response body, use `apiV1DeviceDeviceIdStatusGet$Json()` instead.
   *
   * This method doesn't expect any request body.
   */
  apiV1DeviceDeviceIdStatusGet$Json$Response(params: {
    id: number;
  }): Observable<StrictHttpResponse<IStatus>> {

    const rb = new RequestBuilder(this.rootUrl, DeviceService.ApiV1DeviceDeviceIdStatusGetPath, 'get');
    if (params) {
      rb.path('id', params.id, {});
    }

    return this.http.request(rb.build({
      responseType: 'json',
      accept: 'text/json'
    })).pipe(
      filter((r: any) => r instanceof HttpResponse),
      map((r: HttpResponse<any>) => {
        return r as StrictHttpResponse<IStatus>;
      })
    );
  }

  /**
   * This method provides access to only to the response body.
   * To access the full response (for headers, for example), `apiV1DeviceDeviceIdStatusGet$Json$Response()` instead.
   *
   * This method doesn't expect any request body.
   */
  apiV1DeviceDeviceIdStatusGet$Json(params: {
    id: number;
  }): Observable<IStatus> {

    return this.apiV1DeviceDeviceIdStatusGet$Json$Response(params).pipe(
      map((r: StrictHttpResponse<IStatus>) => r.body as IStatus)
    );
  }

}
