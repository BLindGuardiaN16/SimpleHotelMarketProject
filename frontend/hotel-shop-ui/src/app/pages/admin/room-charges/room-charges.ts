import { Component, OnInit } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { AsyncPipe, NgFor, NgIf,DatePipe } from '@angular/common';
import { Observable } from 'rxjs';

@Component({
  selector: 'app-room-charges',
  standalone: true,
  imports: [NgFor, NgIf, AsyncPipe, DatePipe],
  templateUrl: './room-charges.html',
  styleUrl: './room-charges.css',
})
export class RoomCharges implements OnInit {
  private readonly apiBase = 'https://localhost:7234/api';

  sales$!: Observable<any[]>;
  selectedSale$?: Observable<any>;

  constructor(private http: HttpClient) {}

  ngOnInit(): void {
    this.sales$ = this.http.get<any[]>(`${this.apiBase}/sales`);
  }

  loadSaleDetail(id: number): void {
    this.selectedSale$ = this.http.get<any>(`${this.apiBase}/sales/${id}`);
  }

  reload(): void{
    this.sales$ = this.http.get<any[]>(`${this.apiBase}/sales`);
    this.selectedSale$ = undefined;
  }
  asUtc(value: any): any {
  if (typeof value === 'string') {
    // Z yoksa UTC olarak işaretle
    if (!value.endsWith('Z')) return value + 'Z';
  }
  return value;
}

}
