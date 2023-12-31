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

import { GlobalConfigurationViewModel } from '../models/global-configuration-view-model';
import { GlobalIoTCoreStatusViewModel } from '../models/global-io-t-core-status-view-model';

@Injectable({
  providedIn: 'root',
})
export class GlobalService extends BaseService {
  constructor(
    config: ApiConfiguration,
    http: HttpClient
  ) {
    super(config, http);
  }

  /**
   * Path part for operation apiV1GlobalStartPost
   */
  static readonly ApiV1GlobalStartPostPath = '/api/v1/Global/start';

  /**
   * This method provides access to the full `HttpResponse`, allowing access to response headers.
   * To access only the response body, use `apiV1GlobalStartPost()` instead.
   *
   * This method doesn't expect any request body.
   */
  apiV1GlobalStartPost$Response(params?: {
  }): Observable<StrictHttpResponse<void>> {

    const rb = new RequestBuilder(this.rootUrl, GlobalService.ApiV1GlobalStartPostPath, 'post');
    if (params) {
    }

    return this.http.request(rb.build({
      responseType: 'text',
      accept: '*/*'
    })).pipe(
      filter((r: any) => r instanceof HttpResponse),
      map((r: HttpResponse<any>) => {
        return (r as HttpResponse<any>).clone({ body: undefined }) as StrictHttpResponse<void>;
      })
    );
  }

  /**
   * This method provides access to only to the response body.
   * To access the full response (for headers, for example), `apiV1GlobalStartPost$Response()` instead.
   *
   * This method doesn't expect any request body.
   */
  apiV1GlobalStartPost(params?: {
  }): Observable<void> {

    return this.apiV1GlobalStartPost$Response(params).pipe(
      map((r: StrictHttpResponse<void>) => r.body as void)
    );
  }

  /**
   * Path part for operation apiV1GlobalStopPost
   */
  static readonly ApiV1GlobalStopPostPath = '/api/v1/Global/stop';

  /**
   * This method provides access to the full `HttpResponse`, allowing access to response headers.
   * To access only the response body, use `apiV1GlobalStopPost()` instead.
   *
   * This method doesn't expect any request body.
   */
  apiV1GlobalStopPost$Response(params?: {
  }): Observable<StrictHttpResponse<void>> {

    const rb = new RequestBuilder(this.rootUrl, GlobalService.ApiV1GlobalStopPostPath, 'post');
    if (params) {
    }

    return this.http.request(rb.build({
      responseType: 'text',
      accept: '*/*'
    })).pipe(
      filter((r: any) => r instanceof HttpResponse),
      map((r: HttpResponse<any>) => {
        return (r as HttpResponse<any>).clone({ body: undefined }) as StrictHttpResponse<void>;
      })
    );
  }

  /**
   * This method provides access to only to the response body.
   * To access the full response (for headers, for example), `apiV1GlobalStopPost$Response()` instead.
   *
   * This method doesn't expect any request body.
   */
  apiV1GlobalStopPost(params?: {
  }): Observable<void> {

    return this.apiV1GlobalStopPost$Response(params).pipe(
      map((r: StrictHttpResponse<void>) => r.body as void)
    );
  }

  /**
   * Path part for operation apiV1GlobalStatusGet
   */
  static readonly ApiV1GlobalStatusGetPath = '/api/v1/Global/status';

  /**
   * This method provides access to the full `HttpResponse`, allowing access to response headers.
   * To access only the response body, use `apiV1GlobalStatusGet$Plain()` instead.
   *
   * This method doesn't expect any request body.
   */
  apiV1GlobalStatusGet$Plain$Response(params?: {
  }): Observable<StrictHttpResponse<GlobalIoTCoreStatusViewModel>> {

    const rb = new RequestBuilder(this.rootUrl, GlobalService.ApiV1GlobalStatusGetPath, 'get');
    if (params) {
    }

    return this.http.request(rb.build({
      responseType: 'text',
      accept: 'text/plain'
    })).pipe(
      filter((r: any) => r instanceof HttpResponse),
      map((r: HttpResponse<any>) => {
        return r as StrictHttpResponse<GlobalIoTCoreStatusViewModel>;
      })
    );
  }

  /**
   * This method provides access to only to the response body.
   * To access the full response (for headers, for example), `apiV1GlobalStatusGet$Plain$Response()` instead.
   *
   * This method doesn't expect any request body.
   */
  apiV1GlobalStatusGet$Plain(params?: {
  }): Observable<GlobalIoTCoreStatusViewModel> {

    return this.apiV1GlobalStatusGet$Plain$Response(params).pipe(
      map((r: StrictHttpResponse<GlobalIoTCoreStatusViewModel>) => r.body as GlobalIoTCoreStatusViewModel)
    );
  }

  /**
   * This method provides access to the full `HttpResponse`, allowing access to response headers.
   * To access only the response body, use `apiV1GlobalStatusGet$Json()` instead.
   *
   * This method doesn't expect any request body.
   */
  apiV1GlobalStatusGet$Json$Response(params?: {
  }): Observable<StrictHttpResponse<GlobalIoTCoreStatusViewModel>> {

    const rb = new RequestBuilder(this.rootUrl, GlobalService.ApiV1GlobalStatusGetPath, 'get');
    if (params) {
    }

    return this.http.request(rb.build({
      responseType: 'json',
      accept: 'text/json'
    })).pipe(
      filter((r: any) => r instanceof HttpResponse),
      map((r: HttpResponse<any>) => {
        return r as StrictHttpResponse<GlobalIoTCoreStatusViewModel>;
      })
    );
  }

  /**
   * This method provides access to only to the response body.
   * To access the full response (for headers, for example), `apiV1GlobalStatusGet$Json$Response()` instead.
   *
   * This method doesn't expect any request body.
   */
  apiV1GlobalStatusGet$Json(params?: {
  }): Observable<GlobalIoTCoreStatusViewModel> {

    return this.apiV1GlobalStatusGet$Json$Response(params).pipe(
      map((r: StrictHttpResponse<GlobalIoTCoreStatusViewModel>) => r.body as GlobalIoTCoreStatusViewModel)
    );
  }

  /**
   * Path part for operation apiV1GlobalConfigGet
   */
  static readonly ApiV1GlobalConfigGetPath = '/api/v1/Global/config';

  /**
   * This method provides access to the full `HttpResponse`, allowing access to response headers.
   * To access only the response body, use `apiV1GlobalConfigGet$Plain()` instead.
   *
   * This method doesn't expect any request body.
   */
  apiV1GlobalConfigGet$Plain$Response(params?: {
  }): Observable<StrictHttpResponse<GlobalConfigurationViewModel>> {

    const rb = new RequestBuilder(this.rootUrl, GlobalService.ApiV1GlobalConfigGetPath, 'get');
    if (params) {
    }

    return this.http.request(rb.build({
      responseType: 'text',
      accept: 'text/plain'
    })).pipe(
      filter((r: any) => r instanceof HttpResponse),
      map((r: HttpResponse<any>) => {
        return r as StrictHttpResponse<GlobalConfigurationViewModel>;
      })
    );
  }

  /**
   * This method provides access to only to the response body.
   * To access the full response (for headers, for example), `apiV1GlobalConfigGet$Plain$Response()` instead.
   *
   * This method doesn't expect any request body.
   */
  apiV1GlobalConfigGet$Plain(params?: {
  }): Observable<GlobalConfigurationViewModel> {

    return this.apiV1GlobalConfigGet$Plain$Response(params).pipe(
      map((r: StrictHttpResponse<GlobalConfigurationViewModel>) => r.body as GlobalConfigurationViewModel)
    );
  }

  /**
   * This method provides access to the full `HttpResponse`, allowing access to response headers.
   * To access only the response body, use `apiV1GlobalConfigGet$Json()` instead.
   *
   * This method doesn't expect any request body.
   */
  apiV1GlobalConfigGet$Json$Response(params?: {
  }): Observable<StrictHttpResponse<GlobalConfigurationViewModel>> {

    const rb = new RequestBuilder(this.rootUrl, GlobalService.ApiV1GlobalConfigGetPath, 'get');
    if (params) {
    }

    return this.http.request(rb.build({
      responseType: 'json',
      accept: 'text/json'
    })).pipe(
      filter((r: any) => r instanceof HttpResponse),
      map((r: HttpResponse<any>) => {
        return r as StrictHttpResponse<GlobalConfigurationViewModel>;
      })
    );
  }

  /**
   * This method provides access to only to the response body.
   * To access the full response (for headers, for example), `apiV1GlobalConfigGet$Json$Response()` instead.
   *
   * This method doesn't expect any request body.
   */
  apiV1GlobalConfigGet$Json(params?: {
  }): Observable<GlobalConfigurationViewModel> {

    return this.apiV1GlobalConfigGet$Json$Response(params).pipe(
      map((r: StrictHttpResponse<GlobalConfigurationViewModel>) => r.body as GlobalConfigurationViewModel)
    );
  }

  /**
   * Path part for operation apiV1GlobalConfigPut
   */
  static readonly ApiV1GlobalConfigPutPath = '/api/v1/Global/config';

  /**
   * This method provides access to the full `HttpResponse`, allowing access to response headers.
   * To access only the response body, use `apiV1GlobalConfigPut()` instead.
   *
   * This method sends `application/*+json` and handles request body of type `application/*+json`.
   */
  apiV1GlobalConfigPut$Response(params?: {
    body?: GlobalConfigurationViewModel
  }): Observable<StrictHttpResponse<void>> {

    const rb = new RequestBuilder(this.rootUrl, GlobalService.ApiV1GlobalConfigPutPath, 'put');
    if (params) {
      rb.body(params.body, 'application/*+json');
    }

    return this.http.request(rb.build({
      responseType: 'text',
      accept: '*/*'
    })).pipe(
      filter((r: any) => r instanceof HttpResponse),
      map((r: HttpResponse<any>) => {
        return (r as HttpResponse<any>).clone({ body: undefined }) as StrictHttpResponse<void>;
      })
    );
  }

  /**
   * This method provides access to only to the response body.
   * To access the full response (for headers, for example), `apiV1GlobalConfigPut$Response()` instead.
   *
   * This method sends `application/*+json` and handles request body of type `application/*+json`.
   */
  apiV1GlobalConfigPut(params?: {
    body?: GlobalConfigurationViewModel
  }): Observable<void> {

    return this.apiV1GlobalConfigPut$Response(params).pipe(
      map((r: StrictHttpResponse<void>) => r.body as void)
    );
  }

}
