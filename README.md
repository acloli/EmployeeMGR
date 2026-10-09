# EmployeeMGR (社員管理 Web API)

> **Note**
> 本プロジェクトは、**C# / .NET / ASP.NET Core Web API / Entity Framework Core の基礎習得を目的とした学習・研修用プロジェクト**です。
> 本番運用向けではなく、Web API による CRUD 処理、ORM を用いた DB 永続化、JWT によるロールベース認証・認可、グローバル例外ハンドリング（ProblemDetails）、および xUnit による自動テスト（単体・結合テスト）の基本を理解・実践するためのサンプル実装となっています。

Entity Framework Core と SQLite を利用した、社員管理 Web API です。
社員情報の登録・一覧取得・詳細取得・更新・削除（CRUD）機能に加え、JWT によるロールベースアクセス制御、統一されたエラーハンドリング（RFC 7807 ProblemDetails）、構造化ログ、非同期キャンセル制御（CancellationToken）、および xUnit による自動テスト環境を提供します。

---

## 学習テーマ・学べること

- **ASP.NET Core Web API**: コントローラー（Controllers 方式）を用いた RESTful API の設計・実装
- **Entity Framework Core**: DbContext、マイグレーション（Code First）、SQLite を用いたデータ永続化
- **CRUD 実装**: 一覧・詳細・登録・更新・削除の各 HTTP メソッド（GET, POST, PUT, DELETE）と適切なステータスコード（200, 201 Created + Locationヘッダー, 204, 400, 401, 403, 404, 500）の扱い
- **DTO の活用**: エンティティ（DB モデル）と API 入出力モデル（Request / Response DTO）の分離
- **サービス層と DI (依存性の注入)**: コントローラーからビジネスロジックやトークン生成処理をサービス層に分離し、DI コンテナで管理
- **JWT 認証・認可 (Role-Based Access Control: RBAC)**:
  - `Microsoft.AspNetCore.Authentication.JwtBearer` によるトークン検証
  - `[Authorize]` / `[AllowAnonymous]` / `[Authorize(Roles = "Admin")]` 属性によるロール別アクセス制御
  - 参照系は一般ユーザー含む全認証ユーザー、登録・更新・削除は Admin ロール限定
- **グローバル例外ハンドリング & ProblemDetails (RFC 7807)**:
  - `IExceptionHandler` (`GlobalExceptionHandler`) による未処理例外の集約ハンドリング
  - 内部実装や DB の機密情報・スタックトレースを隠蔽した安全な 500 エラーレスポンス（`traceId` 連携）
  - バリデーションエラーやリソース不在時の統一的な `ProblemDetails` レスポンス
- **非同期キャンセル制御 (`CancellationToken`)**:
  - コントローラーからサービス層、EF Core までの `CancellationToken` 伝播による安全なリクエスト中断処理
- **構造化ロギング (`ILogger`)**:
  - 操作ログおよび例外発生時の TraceId 付きログ記録
- **OpenAPI / Swagger の認証対応**: Transformer を用いた Bearer 認証スキームの定義と Swagger UI 上でのテスト
- **自動テスト (xUnit & WebApplicationFactory)**:
  - **単体テスト (Unit Test)**: SQLite in-memory 接続（`DataSource=:memory:`）を用いた DbContext の分離とサービスクラスのテスト
  - **結合テスト (Integration Test)**: `WebApplicationFactory<Program>` を用いたエンドツーエンドの API 疎通、正常系（200, 201）、異常系（400, 401, 403, 404, 500）の自動検証

---

## 主な機能

- **認証 (Authentication)**:
  - ログイン (`POST /api/auth/login`): ユーザー認証および JWT トークンの発行（Admin / User ロール付与）
- **社員管理 (CRUD - 要認証)**:
  - **一覧取得 (Read: List)**: 登録されている全社員の一覧を取得（全認証ユーザー）
  - **詳細取得 (Read: Detail)**: 指定した ID の社員詳細情報を取得（全認証ユーザー）
  - **新規登録 (Create)**: 新しい社員情報を登録（**Admin ロール限定** / `201 Created` + Location ヘッダー）
  - **情報更新 (Update)**: 既存の社員情報を更新（**Admin ロール限定**）
  - **削除 (Delete)**: 指定した ID の社員情報を削除（**Admin ロール限定**）
- **共通基盤**:
  - グローバル例外ハンドリング（RFC 7807 ProblemDetails）
  - 構造化ロギング (`ILogger`)
  - 非同期処理のキャンセル対応 (`CancellationToken`)
- **自動テスト (Test Automation)**:
  - サービスクラスの単体テスト（xUnit + SQLite in-memory）
  - Web API の結合テスト（xUnit + WebApplicationFactory）

---

## 技術スタック

| 分類 | 技術 | バージョン / 備考 |
| :--- | :--- | :--- |
| プラットフォーム | .NET | 10.0 |
| フレームワーク | ASP.NET Core Web API | Controllers 方式 |
| 認証・認可 | JWT Bearer (RBAC) | `Microsoft.AspNetCore.Authentication.JwtBearer` (10.0) |
| エラーハンドリング | ProblemDetails / ExceptionHandler | RFC 7807 準拠 (`IExceptionHandler`) |
| ロギング | ASP.NET Core Logging | `ILogger` (構造化ログ) |
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
│   └── EmployeesController.cs           # 社員管理 API コントローラー (CRUD 実装, RBAC 制御, ProblemDetails)
├── ExceptionHandlers/
│   └── GlobalExceptionHandler.cs        # 未処理例外ハンドラー (IExceptionHandler 実装, 安全な500返却)
├── Models/
│   ├── Employee.cs                      # 社員エンティティ (DBモデル)
│   ├── EmployeeContext.cs               # DbContext (EF Core)
│   ├── CreateEmployeeRequest.cs         # 登録用リクエスト DTO
│   ├── UpdateEmployeeRequest.cs         # 更新用リクエスト DTO
│   ├── EmployeeResponse.cs              # レスポンス用 DTO
│   ├── EmployeeSearchRequest.cs        # 検索・ページネーション用リクエスト DTO
│   └── PagedResponse.cs                 # ページネーションレスポンス DTO
├── Services/
│   ├── IEmployeeService.cs              # 社員管理サービス インターフェース (CancellationToken 対応)
│   ├── EmployeeService.cs               # 社員管理サービス 実装 (ログ出力, ページング順序保証)
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
        └── EmployeesApiTests.cs         # Web API の結合テスト (200, 201, 400, 401, 403, 404, 500検証)
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

※ または `appsettings.Development.json` に直接記述することも可能です：
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
`CustomWebApplicationFactory` を通じてテスト用インプロセスサーバーを起動し、HTTP リクエストを送信して API 全体の動作・ステータスコードを網羅的に検証します。
- **未認証 (`401 Unauthorized`)**: Authorization ヘッダーなしのリクエストが拒否されることを検証
- **正常取得 (`200 OK`)**: Bearer トークン付きリクエストで社員一覧が取得できることを検証
- **新規作成 (`201 Created`)**: 管理者トークンで社員作成後、`201 Created` と `Location` ヘッダーが返り、Location 先から作成データを取得できることを検証
- **ロール認可拒否 (`403 Forbidden`)**: 一般ユーザー（`User` ロール）で更新・削除を実行した際に拒否されることを検証
- **リクエスト不正 (`400 Bad Request`)**:
  - 入力バリデーション違反時に `ValidationProblemDetails` が返ることを検証
  - 更新リクエストで URL の ID とボディの ID が不一致の場合に `ProblemDetails` が返ることを検証
- **リソース不在 (`404 Not Found`)**: 存在しない ID に対する GET / PUT / DELETE 時に `ProblemDetails` が返ることを検証
- **内部エラー安全保護 (`500 Internal Server Error`)**: 予期せぬ例外発生時、スタックトレースや DB 内部情報を隠蔽し、`traceId` を含んだ安全な `ProblemDetails` が返ることを検証

---

## API エンドポイント仕様

### 1. 認証 API (`/api/Auth`)

| メソッド | エンドポイント | 説明 | 認証 | 成功時ステータス |
| :--- | :--- | :--- | :--- | :--- |
| `POST` | `/api/auth/login` | ログインして JWT トークンを取得 | 不要 (`AllowAnonymous`) | `200 OK` |

#### テスト用アカウント
| ユーザー名 (UserName) | パスワード (Password) | 付与されるロール | 権限範囲 |
| :--- | :--- | :--- | :--- |
| `admin` | `test123` | `Admin` | 全操作（社員の閲覧・**登録・更新・削除**） |
| `user` | `test123` | `User` | 社員の閲覧のみ（**登録・更新・削除は不可**） |

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
    "expiresAt": "2026-10-09T10:00:00Z"
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
| `GET` | `/api/Employees` | 社員一覧を取得 (ページネーション対応) | 認証必須 (全ロール) | `200 OK` |
| `GET` | `/api/Employees/all` | 全社員一覧を取得 | 認証必須 (全ロール) | `200 OK` |
| `GET` | `/api/Employees/{id}` | 指定 ID の社員詳細を取得 | 認証必須 (全ロール) | `200 OK` |
| `POST` | `/api/Employees` | 社員を新規登録 | **Admin ロール必須** | `201 Created` |
| `PUT` | `/api/Employees/{id}` | 指定 ID の社員情報を更新 | **Admin ロール必須** | `204 NoContent` |
| `DELETE` | `/api/Employees/{id}` | 指定 ID の社員を削除 | **Admin ロール必須** | `204 NoContent` |

---

### エンドポイント詳細とリクエスト / レスポンス例

#### 1. 社員一覧取得 (`GET /api/Employees?page=1&pageSize=20`)
- **ヘッダー**: `Authorization: Bearer <token>`
- **レスポンス (200 OK)**:
  ```json
  {
    "items": [
      {
        "id": 1,
        "name": "山田 太郎",
        "email": "yamada@example.com",
        "phone": "090-1234-5678",
        "department": "Engineering"
      }
    ],
    "page": 1,
    "pageSize": 20,
    "totalCount": 1
  }
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
- **エラー時 (404 Not Found - ProblemDetails)**:
  ```json
  {
    "type": "about:blank",
    "title": "Employee not found",
    "status": 404,
    "detail": "Employee with ID 999 was not found"
  }
  ```

#### 3. 社員新規登録 (`POST /api/Employees`)
- **ヘッダー**: `Authorization: Bearer <Admin権限のトークン>`
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
  - `Location`: `/api/Employees/3`
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
- **エラー時**:
  - 一般ユーザーで実行: `403 Forbidden`
  - バリデーションエラー (`400 Bad Request`):
    ```json
    {
      "type": "https://tools.ietf.org/html/rfc9110#section-15.5.1",
      "title": "One or more validation errors occurred.",
      "status": 400,
      "errors": {
        "Name": ["The Name field is required."]
      }
    }
    ```

#### 4. 社員情報更新 (`PUT /api/Employees/{id}`)
- **ヘッダー**: `Authorization: Bearer <Admin権限のトークン>`
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
  - 一般ユーザーで実行: `403 Forbidden`
  - URL の ID とボディの ID 不一致 (`400 Bad Request`):
    ```json
    {
      "type": "about:blank",
      "title": "Invalid request",
      "status": 400,
      "detail": "ID mismatch. Request ID: 2, URL ID: 1"
    }
    ```
  - 対象が存在しない場合 (`404 Not Found`):
    ```json
    {
      "type": "about:blank",
      "title": "Employee not found",
      "status": 404,
      "detail": "Employee with ID 1 was not found"
    }
    ```

#### 5. 社員削除 (`DELETE /api/Employees/{id}`)
- **ヘッダー**: `Authorization: Bearer <Admin権限のトークン>`
- **レスポンス**: `204 No Content`
- **エラー時**:
  - 一般ユーザーで実行: `403 Forbidden`
  - 対象が存在しない場合 (`404 Not Found`):
    ```json
    {
      "type": "about:blank",
      "title": "Employee not found",
      "status": 404,
      "detail": "Employee with ID 999 was not found"
    }
    ```

---

### 3. 共通エラーレスポンス仕様 (ProblemDetails: RFC 7807)

未処理の内部例外が発生した場合、`GlobalExceptionHandler` により内部スタックトレースや機密情報を隠蔽した以下の安全な JSON レスポンスが返されます：

```json
{
  "type": "about:blank",
  "title": "Internal Server Error",
  "status": 500,
  "detail": "An unexpected error occurred.",
  "traceId": "0HN1234567890:00000001"
}
```
※ `traceId` はサーバーログと照合して調査を行うための識別子です。
