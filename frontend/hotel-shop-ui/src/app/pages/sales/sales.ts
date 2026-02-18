import { Component } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { AsyncPipe, NgFor } from '@angular/common';
import { BehaviorSubject, combineLatest, map, Observable, startWith } from 'rxjs';

type Category = {
  id: number;
  name: string;
  description?: string | null;
  isActive: boolean;
};

type Product = {
  id: number;
  name: string;
  categoryId: number;
  unitPrice: number;
  stockQuantity: number;
  isActive: boolean;
};

type CartItem = {
  productId: number;
  name: string;
  unitPrice: number;
  quantity: number;
  maxStock:number;
};

@Component({
  selector: 'app-sales',
  standalone: true,
  imports: [NgFor, AsyncPipe],
  templateUrl: './sales.html',
})
export class Sales {
  private readonly apiBase = 'https://localhost:7234/api';

  categories$!: Observable<Category[]>;
  products$!: Observable<Product[]>;
  filteredProducts$!: Observable<Product[]>;

  private selectedCategoryIdSubject = new BehaviorSubject<number | null>(null);
  selectedCategoryId$ = this.selectedCategoryIdSubject.asObservable();

  cart: CartItem[] = [];

  constructor(private http: HttpClient) {
    this.categories$ = this.http.get<Category[]>(`${this.apiBase}/categories/active`);
    this.products$ = this.http.get<Product[]>(`${this.apiBase}/products/active`);

    
    this.filteredProducts$ = combineLatest([
      this.products$,
      this.selectedCategoryId$.pipe(startWith(null)),
    ]).pipe(
      map(([products, selectedId]) => {
        if (selectedId == null) return products;
        return products.filter(p => p.categoryId === selectedId);
      })
    );
    
  }

  selectCategory(id: number | null): void {
    this.selectedCategoryIdSubject.next(id);
  }

  addToCart(p: Product):void {
    const existing = this.cart.find(x => x.productId === p.id);

    if(existing){
      if(existing.quantity >= existing.maxStock){
        alert('Üzgünüz, seçtiğiniz ürünün stoğu bitmiştir.')
        return;
      }
      existing.quantity += 1;
      return;
    }
    if(p.stockQuantity <=0){
      alert('Üzgünüz, seçtiğiniz ürünün stoğu yoktur en kısa zamanda sizlere tekrardan ulaştıracağız')
      return;
    }

    

    this.cart.push({
      productId: p.id,
      name: p.name,
      unitPrice: p.unitPrice,
      quantity:1,
      maxStock:p.stockQuantity
    });
  }
increase(item: CartItem):void{
  if(item.quantity >= item.maxStock){
    alert('Üzgünüz, seçtiğiniz ürünün stoğu bitmiştir.')
    return;
  }
  item.quantity +=1;
}
decrease(item:CartItem):void {
  item.quantity -=1;
  if(item.quantity<=0){
    this.cart = this.cart.filter(x =>x.productId !== item.productId);
  }
}
remove(item: CartItem):void{
  this.cart = this.cart.filter(x =>x.productId !== item.productId);
}

  get cartTotal(): number {
  return this.cart.reduce((sum,item) => sum + item.unitPrice * item.quantity,0 )
}

}
