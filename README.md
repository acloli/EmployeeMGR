# EmployeeMGR (社員管理 Web API)

> **Note**
> 本プロジェクトは、**C# / .NET / ASP.NET Core Web API / Entity Framework Core の基礎習得を目的とした学習・研修用プロジェクト**です。
> 本番運用向けではなく、Web API による CRUD 処理や ORM を用いた DB 永続化の基本を理解・実践するためのサンプル実装となっています。

Entity Framework Core と SQLite を利用した、シンプルな社員管理 Web API です。
社員情報の登録・一覧取得・詳細取得・更新・削除（CRUD）機能を提供します。

---

## 学習テーマ・学べること

- **ASP.NET Core Web API**: コントローラー（Controllers 方式）を用いた RESTful API の設計・実装
- **Entity Framework Core**: DbContext、マイグレーション（Code First）、SQLite を用いたデータ永続化
- **CRUD 実装**: 一覧・詳細・登録・更新・削除の各 HTTP メソッド（GET, POST, PUT, DELETE）とステータスコードの扱い
- **DTO の活用**: エンティティ（DB モデル）と API 入出力モデル（Request / Response DTO）の分離

---

## 主な機能 (CRUD)

- **一覧取得 (Read: List)**: 登録されている全社員の一覧を取得
- **詳細取得 (Read: Detail)**: 指定した ID の社員詳細情報を取得
- **新規登録 (Create)**: 新しい社員情報を登録
- **情報更新 (Update)**: 既存の社員情報を更新
- **削除 (Delete)**: 指定した ID の社員情報を削除

---

## 技術スタック

| 分類 | 技術 | バージョン / 備考 |
| :--- | :--- | :--- |
| プラットフォーム | .NET | 10.0 |
| フレームワーク | ASP.NET Core Web API | Controllers 方式 |
| ORM | Entity Framework Core | 10.0 (Sqlite) |
| データベース | SQLite | `EmployeeList.db` |
| API ドキュメント | OpenAPI / Swagger UI | `NSwag.AspNetCore` |

---

## プロジェクト構成

```text
EmployeeMGR/
├── Controllers/
│   └── EmployeesController.cs       # 社員管理 API コントローラー (CRUD 実装)
├── Models/
│   ├── Employee.cs                  # 社員エンティティ (DBモデル)
│   ├── EmployeeContext.cs           # DbContext (EF Core)
│   ├── CreateEmployeeRequest.cs     # 登録用リクエスト DTO
│   ├── UpdateEmployeeRequest.cs     # 更新用リクエスト DTO
│   └── EmployeeResponse.cs          # レスポンス用 DTO
├── Migrations/                      # EF Core マイグレーションファイル
├── Properties/
│   └── launchSettings.json          # 起動プロファイル設定 (ポート等)
├── appsettings.json                 # アプリケーション共通設定
├── appsettings.Development.json     # 開発環境用設定 (SQLite 接続文字列)
├── EmployeeList.db                  # SQLite データベースファイル
├── EmployeeMGR.csproj               # プロジェクト定義・依存関係
└── Program.cs                       # アプリケーションのエントリポイント
```

---

## セットアップと起動手順

### 前提条件
- [.NET 10.0 SDK](https://dotnet.microsoft.com/download) がインストールされていること

### 1. リポジトリの準備
```bash
cd EmployeeMGR
```

### 2. データベースマイグレーションの適用 (初回のみ)
データベースファイルが存在しない、または最新のスキーマを反映したい場合に実行します：
```bash
dotnet ef database update
```
※ 既に `EmployeeList.db` が存在する場合はスキップ可能です。

### 3. アプリケーションの起動
```bash
dotnet run
```
または launch profile を指定して起動：
```bash
dotnet run --launch-profile http
```

### 4. Swagger UI (API ドキュメント) へのアクセス
開発環境で起動中、ブラウザで以下にアクセスすると Swagger UI から各 API を確認・実行テストできます：
- **Swagger UI**: [http://localhost:5086/swagger](http://localhost:5086/swagger)
- **OpenAPI 仕様 (JSON)**: [http://localhost:5086/openapi/v1.json](http://localhost:5086/openapi/v1.json)

---

## API エンドポイント仕様

ベース URL: `/api/Employees`

| メソッド | エンドポイント | 説明 | 成功時ステータス |
| :--- | :--- | :--- | :--- |
| `GET` | `/api/Employees` | 社員一覧を取得 | `200 OK` |
| `GET` | `/api/Employees/{id}` | 指定 ID の社員詳細を取得 | `200 OK` |
| `POST` | `/api/Employees` | 社員を新規登録 | `201 Created` |
| `PUT` | `/api/Employees/{id}` | 指定 ID の社員情報を更新 | `204 NoContent` |
| `DELETE` | `/api/Employees/{id}` | 指定 ID の社員を削除 | `204 NoContent` |

---

### エンドポイント詳細とリクエスト / レスポンス例

#### 1. 社員一覧取得 (`GET /api/Employees`)
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

#### 2. 社員詳細取得 (`GET /api/Employees/{id}`)
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
- **エラー時**: 指定した ID の社員が存在しない場合は `404 Not Found`

#### 3. 社員新規登録 (`POST /api/Employees`)
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
  - URL の `{id}` とボディの `id` が不一致の場合: `400 Bad Request`
  - 対象の社員が存在しない場合: `404 Not Found`

#### 5. 社員削除 (`DELETE /api/Employees/{id}`)
- **レスポンス**: `204 No Content`
- **エラー時**: 対象の社員が存在しない場合: `404 Not Found`
