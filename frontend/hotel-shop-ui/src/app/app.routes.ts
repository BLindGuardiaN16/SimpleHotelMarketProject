import { Routes } from '@angular/router';

import { Login } from './pages/login/login';
import { Sales } from './pages/sales/sales';
import { Products } from './pages/admin/products/products';
import { Categories } from './pages/admin/categories/categories';
import { RoomCharges } from './pages/admin/room-charges/room-charges';

export const routes: Routes = [
  { path: '', redirectTo: 'login', pathMatch: 'full' },

  { path: 'login', component: Login },
  { path: 'sales', component: Sales },

  { path: 'admin/products', component: Products },
  { path: 'admin/categories', component: Categories },
  { path: 'admin/room-charges', component: RoomCharges },

  { path: '**', redirectTo: 'login' }
];
