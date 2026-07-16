# ASP_MessageBoard

ASP_MessageBoard 是使用 ASP.NET Core MVC 製作的簡易社群留言板，支援手機號碼註冊與登入、文章 CRUD、文章留言及圖片上傳。

專案採用 SQL Server，所有資料庫存取皆透過 Stored Procedure 執行，並以 Controller、Service、Repository 分層管理責任。

## 開發環境

- .NET 10 SDK
- ASP.NET Core MVC
- SQL Server
- Bootstrap
- Visual Studio 2026、Visual Studio 2022（需支援 .NET 10）或其他可執行 .NET 10 的開發工具

## 建立資料庫

本專案預設使用的資料庫名稱為：

```text
ASP_MessageBoard
```

可以使用 SQL Server Management Studio（SSMS）或其他 SQL Server 管理工具建立資料庫。

### 1. 建立資料庫

先連線至 SQL Server，再執行：

```sql
IF DB_ID(N'ASP_MessageBoard') IS NULL
BEGIN
    CREATE DATABASE ASP_MessageBoard;
END;
GO
```

### 2. 切換至專案資料庫

```sql
USE ASP_MessageBoard;
GO
```

### 3. 執行資料庫腳本

請在 `ASP_MessageBoard` 資料庫中，依照以下順序執行：

1. `DB/01_CreateTables.sql`
2. `DB/02_CreateStoredProcedures.sql`

各腳本用途如下：

| 腳本 | 用途 |
| --- | --- |
| `01_CreateTables.sql` | 建立 `Users`、`Posts`、`Comments` 資料表、外鍵、唯一索引及查詢索引 |
| `02_CreateStoredProcedures.sql` | 建立註冊登入、文章 CRUD 及留言所需的 Stored Procedure |

`01_CreateTables.sql` 會以 Transaction 建立資料表及索引；若執行失敗，會 Rollback。刪除文章時，`usp_Post_Delete` 也會在同一個 Transaction 中刪除文章及其留言。

> SQL 腳本本身不包含 `USE ASP_MessageBoard`，執行前請確認查詢視窗目前選取的資料庫是 `ASP_MessageBoard`，避免將資料表建立到其他資料庫。

## 設定資料庫連線

預設連線字串位於：

- `appsettings.json`
- `appsettings.Development.json`

目前預設設定：

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=127.0.0.1;Database=ASP_MessageBoard;Trusted_Connection=True;TrustServerCertificate=True;"
  }
}
```

如果 SQL Server 使用 Windows 驗證，只要依實際環境修改 `Server`：

```text
Server=伺服器名稱;Database=ASP_MessageBoard;Trusted_Connection=True;TrustServerCertificate=True;
```

## 執行專案

確認資料庫及 Stored Procedure 建立完成後，在專案根目錄執行：

```powershell
dotnet restore
dotnet run --launch-profile https
```

第一次使用時，先進入註冊頁建立帳號，再使用手機號碼與密碼登入。

## 主要功能

- 使用手機號碼註冊及登入
- 使用 Cookie Authentication 與 Claims 保存登入狀態
- 列出所有文章
- 登入使用者可以新增文章
- 文章作者可以編輯及刪除自己的文章
- 登入使用者可以新增留言
- 支援文章圖片及使用者照片上傳
- 使用 Bootstrap 支援響應式頁面
- 使用 Anti-forgery Token 防止 CSRF
- Razor 預設 HTML Encoding 防止使用者內容直接形成 HTML
- 所有 SQL 呼叫使用參數及 Stored Procedure
- 刪除文章及留言時使用 Transaction

## 專案資料夾結構

```text
ASP_MessageBoard/
├─ Common/
│  └─ Exceptions/                 共用的業務例外
├─ Controllers/                   接收 HTTP Request、驗證頁面輸入及回傳 View
├─ DB/                            SQL Server DDL 與 Stored Procedure 腳本
├─ Models/
│  └─ Entities/                   對應核心資料的 Entity
├─ Repositories/
│  ├─ Implementations/            執行 Stored Procedure 與資料映射
│  ├─ Interfaces/                 Repository 介面
│  └─ Models/                     資料庫查詢結果模型
├─ Services/
│  ├─ DTOs/                       Service 輸入及輸出資料
│  ├─ Implementations/            業務規則、權限及圖片處理
│  └─ Interfaces/                 Service 介面
├─ ViewModels/                    Razor 頁面的輸入及顯示模型
├─ Views/                         Razor Views
├─ wwwroot/
│  ├─ css/                        網站樣式
│  ├─ js/                         前端 JavaScript
│  ├─ lib/                        Bootstrap、jQuery 等前端套件
│  └─ uploads/                    執行期間上傳的圖片
├─ Program.cs                     DI、Cookie Authentication 與 Middleware 設定
├─ appsettings.json               共用應用程式設定
└─ appsettings.Development.json   開發環境設定
```

## 程式分層

主要請求流程如下：

```text
Browser
  → Controller / View
  → Service
  → Repository
  → Stored Procedure
  → SQL Server
```

### Controller 與 View

Controller 負責接收請求、檢查 ModelState、取得登入使用者 Claim，並將結果傳給 View。View 使用 ViewModel 顯示資料，不直接執行資料庫操作。

### Service

Service 負責主要業務規則，例如：

- 建立及驗證帳號
- 判斷文章作者是否有編輯或刪除權限
- 整理文章與留言的輸出資料
- 驗證、儲存及刪除圖片
- 將資料庫錯誤轉換成業務例外

### Repository

Repository 負責：

- 建立 SQL Server 連線
- 使用參數呼叫 Stored Procedure
- 將查詢結果映射為 Entity 或 Repository Model

Repository 不負責頁面顯示，也不應包含主要業務規則。

### Stored Procedure

資料表的查詢與異動由 `DB/02_CreateStoredProcedures.sql` 中的 Stored Procedure 執行。文章更新及刪除會同時傳入 `PostId` 與登入使用者的 `UserId`，在資料庫層再次驗證文章作者。

## 主要資料表

| 資料表 | 用途 |
| --- | --- |
| `Users` | 使用者帳號、手機號碼、密碼雜湊及個人資料 |
| `Posts` | 文章內容、作者及文章圖片 |
| `Comments` | 文章留言、留言者及所屬文章 |

`Users.PhoneNumber` 具有唯一索引，避免重複手機號碼註冊。