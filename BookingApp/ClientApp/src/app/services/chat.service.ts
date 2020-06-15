import { Injectable } from '@angular/core';
import { Observable } from 'rxjs/Observable';
import { HttpClient, HttpHeaders } from '@angular/common/http';
import { Response } from '@angular/http';
import 'rxjs/add/operator/map';
import 'rxjs/add/operator/catch';

import { BASE_API_URL } from '../globals';
import { Chat } from '../models/chat';

@Injectable()
export class ChatService {
  path: string;
  headers: HttpHeaders = new HttpHeaders({
    "Content-Type": "application/json",
    "Accept": "application/json"
  });

  constructor(private http: HttpClient) {
    this.path = BASE_API_URL + '/chat';
  }

  getChats(userId?: string): Observable<Chat[]> {
    return this.http.get(this.path + '/' + userId,
      {
        headers: this.headers
      }).map((response: Response) => response)
      .catch((error: any) =>
        Observable.throw(error.error || 'Server error')); 
  }  
}
