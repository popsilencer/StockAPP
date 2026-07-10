# Stock App — Design Spec

- **วันที่**: 2026-07-10
- **วัตถุประสงค์**: โปรแกรมสต๊อกสินค้า (ทดลองสร้าง)
- **Stack**: Vue 3 (frontend) + .NET 9 Web API (backend) + LiteDB (NoSQL)

## 1. Scope

พื้นฐาน:

- รายการสินค้า (CRUD)
- รับเข้า / จ่ายออกสต๊อก (stock movement)
- ดูยอดคงเหลือ
- แจ้งเตือนของใกล้หมด (low-stock alert)
- login 1 user (JWT)

ไม่อยู่ใน scope: หมวดหมู่สินค้า, supplier/customer, ใบสั่งซื้อ/ขาย, report ซับซ้อน, multi-user.

## 2. โครงสร้าง Project

```
D:\Stock\
  backend/     # .NET 9 Web API
  frontend/    # Vue 3 + Vite
```

## 3. Backend (.NET 9, Layered Architecture)

```
backend/
  Controllers/    AuthController, ProductsController, MovementsController
  Services/       AuthService, ProductService, StockService
  Repositories/   ProductRepository, MovementRepository, UserRepository
  Models/
    Entities/     Product, StockMovement, User
    Dtos/         request/response DTOs
  Middleware/     ExceptionMiddleware
  Data/           LiteDbContext
  appsettings.json
  Program.cs
```

- Flow: `Controller → Service → Repository → LiteDB file`
- Logic ธุรกิจ (คำนวณคงเหลือ, เช็คของใกล้หมด) อยู่ใน Service layer
- NuGet packages:
  - `LiteDB`
  - `BCrypt.Net-Next` (hash password)
  - `Microsoft.AspNetCore.Authentication.JwtBearer`

### Authentication

- 1 user เริ่มต้น seed ตอนรันครั้งแรก (username/password จาก `appsettings.json`)
- password hash ด้วย BCrypt
- `POST /api/auth/login` → ตรวจ credential → คืน JWT (claim: userId, username)
- JWT วางใน `Authorization: Bearer <token>`
- API endpoint อื่น ๆ ต้องผ่าน JWT (policy `[Authorize]`)

## 4. Data Model (LiteDB Documents)

### Product

| Field | Type | รายละเอียด |
|---|---|---|
| Id | int | LiteDB auto-increment |
| Sku | string | unique, required |
| Name | string | required |
| Description | string | optional |
| Unit | string | เช่น ชิ้น, กล่อง |
| Quantity | int | คงเหลือปัจจุบัน |
| ReorderLevel | int | ระดับที่ถือว่าใกล้หมด |

### StockMovement

| Field | Type | รายละเอียด |
|---|---|---|
| Id | int | auto-increment |
| ProductId | int | FK → Product |
| Type | enum | `In` / `Out` |
| Quantity | int | จำนวน (>0) |
| Note | string | optional |
| CreatedAt | DateTime | UTC |

### User

| Field | Type |
|---|---|
| Id | int |
| Username | string (unique) |
| PasswordHash | string |

### กลยุทธ์คงเหลือ

`Product.Quantity` เก็บคงเหลือ (read เร็ว สำหรับ list + low-stock check). ทุกการเปลี่ยนแปลงต้อง:

1. เขียน `StockMovement` record
2. อัปเดต `Product.Quantity` (`In` เพิ่ม, `Out` ลด)

Service ทำทั้งสองภายใน transaction เดียวกัน (LiteDB transaction). กรณี `Out` เกิน `Quantity` → error (400), ไม่อนุญาตติดลบ.

## 5. API Endpoints

| Method | Path | Body | Response | Notes |
|---|---|---|---|---|
| POST | `/api/auth/login` | `{username,password}` | `{token}` | คืน JWT |
| GET | `/api/products` | — | `[Product]` | list ทั้งหมด, support `?search=` |
| GET | `/api/products/{id}` | — | `Product` | |
| POST | `/api/products` | ProductDto | `Product` | สร้าง, sku ซ้ำ → 409 |
| PUT | `/api/products/{id}` | ProductDto | `Product` | |
| DELETE | `/api/products/{id}` | — | 204 | |
| POST | `/api/products/{id}/movements` | `{type,quantity,note}` | `StockMovement` | ปรับสต๊อก |
| GET | `/api/products/low-stock` | — | `[Product]` | Quantity ≤ ReorderLevel |
| GET | `/api/movements` | — | `[StockMovement]` | `?productId=` filter |

## 6. Frontend (Vue 3 + Vite + PrimeVue)

```
frontend/
  src/
    api/
      http.js        # axios instance + interceptors (แนบ JWT, catch error)
      products.js
      auth.js
    stores/
      auth.js        # Pinia: token, user, login/logout
      products.js    # Pinia: list, CRUD actions
    views/
      LoginView.vue
      ProductsView.vue
      ProductFormDialog.vue
      StockAdjustDialog.vue
      MovementsView.vue
    router/
      index.js       # routes + auth guard
    App.vue
    main.js
```

- Composition API (`<script setup>`)
- State: Pinia
- Routing: Vue Router + navigation guard (เช็ค token, ไม่มี → redirect /login)
- UI: PrimeVue (DataTable, Dialog, InputText, Textarea, Button, Toast, Tag)
- หน้าจอ:
  - **Login**: ฟอร์ม username/password
  - **Products**: DataTable แสดงสินค้า, แถวที่ low-stock ไฮไลต์ (Tag สีแดง), ปุ่มเพิ่ม/แก้/ลบ/ปรับสต๊อก
  - **ProductForm**: Dialog สร้าง/แก้สินค้า
  - **StockAdjust**: Dialog รับเข้า/จ่ายออก
  - **Movements**: ตารางประวัติ, filter ตามสินค้า

## 7. Data Flow

```
Vue component
  → Pinia action
    → api/ module (axios)
      → [Authorization: Bearer JWT]
        → Controller
          → Service (business logic)
            → Repository
              → LiteDB file (stock.db)
        ← ExceptionMiddleware → HTTP status code
      ← JSON response
    ← update store
  ← render
```

## 8. Error Handling

**Backend**

- Validation (DataAnnotations): 400 + error detail
- 401: JWT ไม่ถูกต้อง/หมดอายุ/ขาด
- 404: resource ไม่พบ
- 409: sku ซ้ำ
- 400: กรณี `Out` เกินคงเหลือ
- Global `ExceptionMiddleware`: จับ exception ที่ไม่คาดฝัน → 500 + generic message (ไม่ leak stack trace)

**Frontend**

- axios response interceptor จับ 4xx/5xx
- → PrimeVue Toast แสดงข้อความ error
- 401 → clear token + redirect /login

## 9. Testing

- **Backend**: xUnit ทดสอบ Service layer (`ProductService`, `StockService`) ใช้ temp LiteDB file ต่อ test. กรณีทดสอบ: CRUD, stock in/out, ป้องกันติดลบ, low-stock logic.
- **Frontend**: ข้ามไปก่อน (scope ทดลอง)

## 10. การรัน (Development)

- Backend: `dotnet run` → `https://localhost:5xxx` (CORS เปิดให้ frontend origin)
- Frontend: `npm run dev` → `http://localhost:5173`, Vite proxy `/api` → backend
- LiteDB file: `backend/stock.db` (auto-create)

## 11. Out of Scope / อนาคต

- category สินค้า
- multi-user + role
- supplier/customer + purchase/sales order
- report/dashboard
- การ deploy จริง
