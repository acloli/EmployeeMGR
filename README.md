# EmployeeMGR (社員管理 Web API)

> **Note**
> 本プロジェクトは、**C# / .NET / ASP.NET Core Web API / Entity Framework Core の基礎習得を目的とした学習・研修用プロジェクト**です。
> 本番運用向けではなく、Web API による CRUD 処理、ORM を用いた DB 永続化、JWT による認証・認可（ロール別アクセス制御含む）、および xUnit による自動テスト（単体・結合テスト）の基本を理解・実践するためのサンプル実装となっています。

Entity Framework Core と SQLite を利用した、シンプルな社員管理 Web API です。
社員情報の登録・一覧取得・詳細取得・更新・削除（CRUD）機能に加え、JWT（JSON Web Token）を用いた認証・認可機能、および xUnit による自動テスト環境を提供します。

---

## 学習テーマ・学べること

- **ASP.NET Core Web API**: コントローラー（Controllers 方式）を用いた RESTful API の設計・実装
- **Entity Framework Core**: DbContext、マイグレーション（Code First）、SQLite を用いたデータ永続化
- **CRUD 実装**: 一覧・詳細・登録・更新・削除の各 HTTP メソッド（GET, POST, PUT, DELETE）とステータスコードの扱い
- **DTO の活用**: エンティティ（DB モデル）と API 入出力モデル（Request / Response DTO）の分離
- **サービス層と DI (依存性の注入)**: コントローラーからビジネスロジックやトークン生成処理をサービス層に分離し、DI コンテナで管理
- **JWT 認証・認可 (Authentication & Authorization)**:
  - `Microsoft.AspNetCore.Authentication.JwtBearer` によるトークン検証
  - `[Authorize]` / `[AllowAnonymous]` 属性によるエンドポイントのアクセス制御
  - `[Authorize(Roles = "Admin")]` によるロールベースの認可制御（RBAC）
  - クレーム（Name, Role）を含めた JWT トークンの生成・発行
- **OpenAPI / Swagger の認証対応**: Transformer を用いた Bearer 認証スキームの定義と Swagger UI 上でのテスト
- **自動テスト (xUnit & WebApplicationFactory)**:
  - **単体テスト (Unit Test)**: SQLite in-memory 接続（`DataSource=:memory:`）を用いた DbContext の分離とサービスクラスのテスト
  - **結合テスト (Integration Test)**: `WebApplicationFactory<Program>` を用いたエンドツーエンドの API 疎通確認・ロール認可テスト

---

## 主な機能

- **認証 (Authentication)**:
  - ログイン (`POST /api/auth/login`): ユーザー認証および JWT トークンの発行（Admin / User ロール付与）
- **社員管理 (CRUD - 要認証)**:
  - **一覧取得 (Read: List)**: 登録されている全社員の一覧を取得（全認証ユーザー）
  - **詳細取得 (Read: Detail)**: 指定した ID の社員詳細情報を取得（全認証ユーザー）
  - **新規登録 (Create)**: 新しい社員情報を登録（全認証ユーザー）
  - **情報更新 (Update)**: 既存の社員情報を更新（全認証ユーザー）
  - **削除 (Delete)**: 指定した ID の社員情報を削除（**Admin ロール限定**）
- **自動テスト (Test Automation)**:
  - サービスクラスの単体テスト（xUnit + SQLite in-memory）
  - API エンドポイントの結合テスト（xUnit + WebApplicationFactory）

---

## 技術スタック

| 分類 | 技術 | バージョン / 備考 |
| :--- | :--- | :--- |
| プラットフォーム | .NET | 10.0 |
| フレームワーク | ASP.NET Core Web API | Controllers 方式 |
| 認証・認可 | JWT Bearer (ロール認可対応) | `Microsoft.AspNetCore.Authentication.JwtBearer` (10.0) |
| ORM | Entity Framework Core | 10.0 (Sqlite) |
| データベース | SQLite | `EmployeeList.db` |
| API ドキュメント | OpenAPI / Swagger UI | `NSwag.AspNetCore` / `Microsoft.AspNetCore.OpenApi` |
| テストフレームワーク | xUnit | 2.9.3 |
| 統合テストライブラリ | ASP.NET Core Testing | `Microsoft.AspNetCore.Mvc.Testing` (10.0) |

---

## プロジェクト構成

```text
EmployeeMGR/
├── Auth/
│   ├── LoginRequest.cs                  # ログインリクエスト DTO
│   └── LoginResponse.cs                 # ログインレスポンス DTO (JWT & 有効期限)
├── Controllers/
│   ├── AuthController.cs                # 認証 API コントローラー (ログイン・トークン発行)
│   └── EmployeesController.cs           # 社員管理 API コントローラー (CRUD 実装, [Authorize], 削除は Admin 限定)
├── Models/
│   ├── Employee.cs                      # 社員エンティティ (DBモデル)
│   ├── EmployeeContext.cs               # DbContext (EF Core)
│   ├── CreateEmployeeRequest.cs         # 登録用リクエスト DTO
│   ├── UpdateEmployeeRequest.cs         # 更新用リクエスト DTO
│   └── EmployeeResponse.cs              # レスポンス用 DTO
├── Services/
│   ├── IEmployeeService.cs              # 社員管理サービス インターフェース
│   ├── EmployeeService.cs               # 社員管理サービス 実装
│   ├── ITokenService.cs                 # JWT トークン生成サービス インターフェース
│   └── TokenService.cs                  # JWT トークン生成サービス 実装
├── OpenApi/
│   ├── AuthOperationTransformer.cs      # Swagger/OpenAPI 用 認証要件 Transformer
│   └── BearerSecuritySchemeTransformer.cs # Swagger/OpenAPI 用 Bearer スキーム定義
├── Migrations/                          # EF Core マイグレーションファイル
├── Properties/
│   └── launchSettings.json              # 起動プロファイル設定 (ポート等)
├── appsettings.json                     # アプリケーション共通設定
├── appsettings.Development.json         # 開発環境用設定 (SQLite 接続文字列等)
├── EmployeeList.db                      # SQLite データベースファイル
├── EmployeeMGR.csproj                   # メインプロジェクト定義
├── Program.cs                           # アプリケーションのエントリポイント
└── EmployeeMGR.Tests/                   # テストプロジェクト (xUnit)
    ├── EmployeeMGR.Tests.csproj         # テストプロジェクト定義
    ├── CustomWebApplicationFactory.cs   # 結合テスト用 WebApplicationFactory (テスト用JWT・DB初期化設定)
    ├── Services/
    │   └── EmployeeServiceTests.cs      # EmployeeService の単体テスト (SQLite in-memory)
    └── Integration/
        └── EmployeesApiTests.cs         # Web API の結合テスト (認証・ロール認可検証)
```

---

## セットアップと起動手順

### 前提条件
- [.NET 10.0 SDK](https://dotnet.microsoft.com/download) がインストールされていること

### 1. リポジトリの準備
```bash
cd EmployeeMGR
```

### 2. JWT 設定の構成 (初回のみ)
本アプリケーションでは起動時に `Jwt:Key`, `Jwt:Issuer`, `Jwt:Audience` の設定が必要です。
開発環境では .NET User Secrets または `appsettings.Development.json` に設定します。

**User Secrets を使用する場合 (推奨):**
```bash
dotnet user-secrets set "Jwt:Key" "YourSuperSecretKeyWithAtLeast32CharactersLong!"
dotnet user-secrets set "Jwt:Issuer" "EmployeeMGR"
dotnet user-secrets set "Jwt:Audience" "EmployeeMGRUser"
```

※ または `appsettings.Development.json` に直接記述することも可能です（シークレットキーのコミットにはご注意ください）：
```json
{
  "Jwt": {
    "Key": "YourSuperSecretKeyWithAtLeast32CharactersLong!",
    "Issuer": "EmployeeMGR",
    "Audience": "EmployeeMGRUser"
  }
}
```

### 3. データベースマイグレーションの適用 (初回のみ)
データベースファイルが存在しない、または最新のスキーマを反映したい場合に実行します：
```bash
dotnet ef database update
```
※ 既に `EmployeeList.db` が存在する場合はスキップ可能です。

### 4. アプリケーションの起動
```bash
dotnet run
```
または launch profile を指定して起動：
```bash
dotnet run --launch-profile http
```

### 5. Swagger UI (API ドキュメント) へのアクセスと認証テスト
開発環境で起動中、ブラウザで以下にアクセスすると Swagger UI から各 API を確認・実行テストできます：
- **Swagger UI**: [http://localhost:5086/swagger](http://localhost:5086/swagger)
- **OpenAPI 仕様 (JSON)**: [http://localhost:5086/openapi/v1.json](http://localhost:5086/openapi/v1.json)

**Swagger UI での認証手順:**
1. `POST /api/auth/login` でログインを実行し、レスポンスの `token` 文字列をコピーします。
2. 画面右上にある **「Authorize」** ボタンをクリックします。
3. `Value:` 入力欄に取得したトークンを入力（または `Bearer <token>`）して「Authorize」をクリックします。
4. 保護された `/api/Employees` エンドポイントが実行可能になります。

---

## 自動テストの実行

本プロジェクトには xUnit による単体テストおよび結合テストが含まれています。
以下のコマンドで全テストを一括実行できます：

```bash
dotnet test
```

### 実装されているテスト一覧

#### 1. 単体テスト (`EmployeeMGR.Tests/Services/EmployeeServiceTests.cs`)
SQLite in-memory モード (`DataSource=:memory:`) を使用し、実際のファイル DB に影響を与えずに独立した環境でサービスクラスをテストします。
- **`GetEmployeeByIdAsync_WhenEmployeeExists_ReturnsEmployee`**: 指定した ID の社員が存在する場合に、該当する社員エンティティが正しく取得できることを検証
- **`GetEmployeeByIdAsync_WhenNotFound_ReturnsNull`**: 存在しない ID を指定した場合に `null` が返されることを検証

#### 2. 結合テスト (`EmployeeMGR.Tests/Integration/EmployeesApiTests.cs`)
`CustomWebApplicationFactory` を通じてテスト用サーバーを起動し、HTTP リクエストを送信して API 全体の動作を検証します。
- **`GetEmployees_WithoutToken_ReturnsUnauthorized`**: Authorization ヘッダーを付与せずにアクセスした場合、`401 Unauthorized` が返ることを検証
- **`GetEmployees_WithToken_ReturnsOk`**: 正常にログインして取得した Bearer トークンを付与した場合、`200 OK` が返ることを検証
- **`DeleteEmployee_WithUserToken_Returns403`**: 一般ユーザー（`User` ロール）のトークンで社員削除 API を実行した場合、管理者権限が必要なため `403 Forbidden` が返ることを検証

---

## API エンドポイント仕様

### 1. 認証 API (`/api/Auth`)

| メソッド | エンドポイント | 説明 | 認証 | 成功時ステータス |
| :--- | :--- | :--- | :--- | :--- |
| `POST` | `/api/auth/login` | ログインして JWT トークンを取得 | 不要 (`AllowAnonymous`) | `200 OK` |

#### テスト用アカウント
| ユーザー名 (UserName) | パスワード (Password) | 付与されるロール | 権限範囲 |
| :--- | :--- | :--- | :--- |
| `admin` | `test123` | `Admin` | 全操作（社員の閲覧・登録・更新・**削除**） |
| `user` | `test123` | `User` | 社員の閲覧・登録・更新（**削除不可**） |

#### ログインリクエスト / レスポンス例
- **リクエスト (`POST /api/auth/login`)**:
  ```json
  {
    "userName": "admin",
    "password": "test123"
  }
  ```
- **レスポンス (200 OK)**:
  ```json
  {
    "token": "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9...",
    "expiresAt": "2026-10-07T10:00:00Z"
  }
  ```
- **エラー時**: 認証失敗時は `401 Unauthorized`

---

### 2. 社員管理 API (`/api/Employees`)

※ 社員管理 API のすべてのエンドポイントは **認証必須 (`[Authorize]`)** です。
リクエスト時に HTTP ヘッダーに Bearer トークンを指定してください：
```http
Authorization: Bearer <取得したJWTトークン>
```

| メソッド | エンドポイント | 説明 | 認可要件 | 成功時ステータス |
| :--- | :--- | :--- | :--- | :--- |
| `GET` | `/api/Employees` | 社員一覧を取得 | 認証必須 (全ロール) | `200 OK` |
| `GET` | `/api/Employees/{id}` | 指定 ID の社員詳細を取得 | 認証必須 (全ロール) | `200 OK` |
| `POST` | `/api/Employees` | 社員を新規登録 | 認証必須 (全ロール) | `201 Created` |
| `PUT` | `/api/Employees/{id}` | 指定 ID の社員情報を更新 | 認証必須 (全ロール) | `204 NoContent` |
| `DELETE` | `/api/Employees/{id}` | 指定 ID の社員を削除 | **Admin ロール必須** | `204 NoContent` |

---

### エンドポイント詳細とリクエスト / レスポンス例

#### 1. 社員一覧取得 (`GET /api/Employees`)
- **ヘッダー**: `Authorization: Bearer <token>`
- **レスポンス (200 OK)**:
  ```json
  [
    {
      "id": 1,
      "name": "山田 太郎",
      "email": "yamada@example.com",
      "phone": "090-1234-5678",
      "department": "Engineering"
    },
    {
      "id": 2,
      "name": "佐藤 花子",
      "email": "sato@example.com",
      "phone": "080-9876-5432",
      "department": "Marketing"
    }
  ]
  ```
- **エラー時**: 未認証または無効なトークンの場合 `401 Unauthorized`

#### 2. 社員詳細取得 (`GET /api/Employees/{id}`)
- **ヘッダー**: `Authorization: Bearer <token>`
- **レスポンス (200 OK)**:
  ```json
  {
    "id": 1,
    "name": "山田 太郎",
    "email": "yamada@example.com",
    "phone": "090-1234-5678",
    "department": "Engineering"
  }
  ```
- **エラー時**:
  - 未認証: `401 Unauthorized`
  - 対象が存在しない場合: `404 Not Found`

#### 3. 社員新規登録 (`POST /api/Employees`)
- **ヘッダー**: `Authorization: Bearer <token>`
- **リクエストボディ**:
  ```json
  {
    "name": "鈴木 一郎",
    "email": "suzuki@example.com",
    "phone": "070-1111-2222",
    "department": "Sales"
  }
  ```
- **レスポンス (201 Created)**:
  - `Location` ヘッダーに作成されたリソースの URL（例: `/api/Employees/3`）が返されます。
  - レスポンスボディ:
    ```json
    {
      "id": 3,
      "name": "鈴木 一郎",
      "email": "suzuki@example.com",
      "phone": "070-1111-2222",
      "department": "Sales"
    }
    ```

#### 4. 社員情報更新 (`PUT /api/Employees/{id}`)
- **ヘッダー**: `Authorization: Bearer <token>`
- **リクエストボディ**:
  ```json
  {
    "id": 1,
    "name": "山田 太郎 (更新)",
    "email": "yamada.new@example.com",
    "phone": "090-1234-5678",
    "department": "Management"
  }
  ```
- **レスポンス**: `204 No Content`
- **エラー時**:
  - 未認証: `401 Unauthorized`
  - URL の `{id}` とボディの `id` が不一致の場合: `400 Bad Request`
  - 対象の社員が存在しない場合: `404 Not Found`

#### 5. 社員削除 (`DELETE /api/Employees/{id}`)
- **ヘッダー**: `Authorization: Bearer <Admin権限のトークン>`
- **レスポンス**: `204 No Content`
- **エラー時**:
  - 未認証: `401 Unauthorized`
  - **権限不足（一般ユーザーなど `Admin` ロール以外の場合）: `403 Forbidden`**
  - 対象の社員が存在しない場合: `404 Not Found`
