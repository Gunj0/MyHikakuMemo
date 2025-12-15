# Supabase の制約

## Supabase セットアップ

- New Project
  - Project name: choice-note
  - Region: Singapore (Tokyo も選べるが Render に合わせる)
- 接続文字列
  - メニューバーの「Connect」
  - Type: .NET
  - Render は IPv6 対応していないので Transaction pooler を使用

## 断続的エラーにより断念

- 環境
  - Render.com の ASP.NET Core から Supabase に接続
- 事象
  - 以下の DB 接続エラーが断続的に発生する
  - エラー内容要約
    - DB に TCP 接続しようとして拒否される
    - `Npgsql.NpgsqlException (0x80004005): Failed to connect to 13.213.241.248:5432`
    - `System.Net.Sockets.SocketException (111): Connection refused`
- 結論
  - Supabase から Neon に移行したことで解決

## 調査経過

- 接続エラー内容

```zsh
info: ChoiceNote.WebAPI.Controllers.NoteController[0]
      GetNotesAsync called
fail: Microsoft.EntityFrameworkCore.Database.Connection[20004]
      An error occurred using the connection to database 'postgres' on server 'tcp://aws-1-ap-southeast-1.pooler.supabase.com:5432'.
fail: Microsoft.EntityFrameworkCore.Query[10100]
      An exception occurred while iterating over the results of a query for context type 'ChoiceNote.WebAPI.Data.AppDbContext'.
      System.InvalidOperationException: An exception has been raised that is likely due to a transient failure.
       ---> Npgsql.NpgsqlException (0x80004005): Failed to connect to 13.213.241.248:5432
       ---> System.Net.Sockets.SocketException (111): Connection refused
         at System.Net.Sockets.Socket.AwaitableSocketAsyncEventArgs.ThrowException(SocketError error, CancellationToken cancellationToken)
         at System.Net.Sockets.Socket.AwaitableSocketAsyncEventArgs.System.Threading.Tasks.Sources.IValueTaskSource.GetResult(Int16 token)
         at System.Net.Sockets.Socket.<ConnectAsync>g__WaitForConnectWithCancellation|285_0(AwaitableSocketAsyncEventArgs saea, ValueTask connectTask, CancellationToken cancellationToken)
         at Npgsql.TaskTimeoutAndCancellation.ExecuteAsync(Func`2 getTaskFunc, NpgsqlTimeout timeout, CancellationToken cancellationToken)
         at Npgsql.Internal.NpgsqlConnector.ConnectAsync(NpgsqlTimeout timeout, CancellationToken cancellationToken)
         at Npgsql.Internal.NpgsqlConnector.ConnectAsync(NpgsqlTimeout timeout, CancellationToken cancellationToken)
         at Npgsql.Internal.NpgsqlConnector.RawOpen(SslMode sslMode, NpgsqlTimeout timeout, Boolean async, CancellationToken cancellationToken)
         at Npgsql.Internal.NpgsqlConnector.<Open>g__OpenCore|214_1(NpgsqlConnector conn, SslMode sslMode, NpgsqlTimeout timeout, Boolean async, CancellationToken cancellationToken)
         at Npgsql.Internal.NpgsqlConnector.Open(NpgsqlTimeout timeout, Boolean async, CancellationToken cancellationToken)
         at Npgsql.PoolingDataSource.OpenNewConnector(NpgsqlConnection conn, NpgsqlTimeout timeout, Boolean async, CancellationToken cancellationToken)
         at Npgsql.PoolingDataSource.<Get>g__RentAsync|33_0(NpgsqlConnection conn, NpgsqlTimeout timeout, Boolean async, CancellationToken cancellationToken)
         at Npgsql.NpgsqlConnection.<Open>g__OpenAsync|42_0(Boolean async, CancellationToken cancellationToken)
         at Microsoft.EntityFrameworkCore.Storage.RelationalConnection.OpenInternalAsync(Boolean errorsExpected, CancellationToken cancellationToken)
         at Microsoft.EntityFrameworkCore.Storage.RelationalConnection.OpenInternalAsync(Boolean errorsExpected, CancellationToken cancellationToken)
         at Microsoft.EntityFrameworkCore.Storage.RelationalConnection.OpenAsync(CancellationToken cancellationToken, Boolean errorsExpected)
         at Microsoft.EntityFrameworkCore.Storage.RelationalCommand.ExecuteReaderAsync(RelationalCommandParameterObject parameterObject, CancellationToken cancellationToken)
         at Microsoft.EntityFrameworkCore.Query.Internal.SingleQueryingEnumerable`1.AsyncEnumerator.InitializeReaderAsync(AsyncEnumerator enumerator, CancellationToken cancellationToken)
         at Npgsql.EntityFrameworkCore.PostgreSQL.Storage.Internal.NpgsqlExecutionStrategy.ExecuteAsync[TState,TResult](TState state, Func`4 operation, Func`4 verifySucceeded, CancellationToken cancellationToken)
         --- End of inner exception stack trace ---
         at Npgsql.EntityFrameworkCore.PostgreSQL.Storage.Internal.NpgsqlExecutionStrategy.ExecuteAsync[TState,TResult](TState state, Func`4 operation, Func`4 verifySucceeded, CancellationToken cancellationToken)
         at Microsoft.EntityFrameworkCore.Query.Internal.SingleQueryingEnumerable`1.AsyncEnumerator.MoveNextAsync()
      System.InvalidOperationException: An exception has been raised that is likely due to a transient failure.
       ---> Npgsql.NpgsqlException (0x80004005): Failed to connect to 13.213.241.248:5432
       ---> System.Net.Sockets.SocketException (111): Connection refused
         at System.Net.Sockets.Socket.AwaitableSocketAsyncEventArgs.ThrowException(SocketError error, CancellationToken cancellationToken)
         at System.Net.Sockets.Socket.AwaitableSocketAsyncEventArgs.System.Threading.Tasks.Sources.IValueTaskSource.GetResult(Int16 token)
         at System.Net.Sockets.Socket.<ConnectAsync>g__WaitForConnectWithCancellation|285_0(AwaitableSocketAsyncEventArgs saea, ValueTask connectTask, CancellationToken cancellationToken)
         at Npgsql.TaskTimeoutAndCancellation.ExecuteAsync(Func`2 getTaskFunc, NpgsqlTimeout timeout, CancellationToken cancellationToken)
         at Npgsql.Internal.NpgsqlConnector.ConnectAsync(NpgsqlTimeout timeout, CancellationToken cancellationToken)
         at Npgsql.Internal.NpgsqlConnector.ConnectAsync(NpgsqlTimeout timeout, CancellationToken cancellationToken)
         at Npgsql.Internal.NpgsqlConnector.RawOpen(SslMode sslMode, NpgsqlTimeout timeout, Boolean async, CancellationToken cancellationToken)
         at Npgsql.Internal.NpgsqlConnector.<Open>g__OpenCore|214_1(NpgsqlConnector conn, SslMode sslMode, NpgsqlTimeout timeout, Boolean async, CancellationToken cancellationToken)
         at Npgsql.Internal.NpgsqlConnector.Open(NpgsqlTimeout timeout, Boolean async, CancellationToken cancellationToken)
         at Npgsql.PoolingDataSource.OpenNewConnector(NpgsqlConnection conn, NpgsqlTimeout timeout, Boolean async, CancellationToken cancellationToken)
         at Npgsql.PoolingDataSource.<Get>g__RentAsync|33_0(NpgsqlConnection conn, NpgsqlTimeout timeout, Boolean async, CancellationToken cancellationToken)
         at Npgsql.NpgsqlConnection.<Open>g__OpenAsync|42_0(Boolean async, CancellationToken cancellationToken)
         at Microsoft.EntityFrameworkCore.Storage.RelationalConnection.OpenInternalAsync(Boolean errorsExpected, CancellationToken cancellationToken)
         at Microsoft.EntityFrameworkCore.Storage.RelationalConnection.OpenInternalAsync(Boolean errorsExpected, CancellationToken cancellationToken)
         at Microsoft.EntityFrameworkCore.Storage.RelationalConnection.OpenAsync(CancellationToken cancellationToken, Boolean errorsExpected)
         at Microsoft.EntityFrameworkCore.Storage.RelationalCommand.ExecuteReaderAsync(RelationalCommandParameterObject parameterObject, CancellationToken cancellationToken)
         at Microsoft.EntityFrameworkCore.Query.Internal.SingleQueryingEnumerable`1.AsyncEnumerator.InitializeReaderAsync(AsyncEnumerator enumerator, CancellationToken cancellationToken)
         at Npgsql.EntityFrameworkCore.PostgreSQL.Storage.Internal.NpgsqlExecutionStrategy.ExecuteAsync[TState,TResult](TState state, Func`4 operation, Func`4 verifySucceeded, CancellationToken cancellationToken)
         --- End of inner exception stack trace ---
         at Npgsql.EntityFrameworkCore.PostgreSQL.Storage.Internal.NpgsqlExecutionStrategy.ExecuteAsync[TState,TResult](TState state, Func`4 operation, Func`4 verifySucceeded, CancellationToken cancellationToken)
         at Microsoft.EntityFrameworkCore.Query.Internal.SingleQueryingEnumerable`1.AsyncEnumerator.MoveNextAsync()
fail: Microsoft.AspNetCore.Server.Kestrel[13]
      Connection id "0HNFRD8BA3KBN", Request id "0HNFRD8BA3KBN:00000003": An unhandled exception was thrown by the application.
      System.InvalidOperationException: An exception has been raised that is likely due to a transient failure.
       ---> Npgsql.NpgsqlException (0x80004005): Failed to connect to 13.213.241.248:5432
       ---> System.Net.Sockets.SocketException (111): Connection refused
         at System.Net.Sockets.Socket.AwaitableSocketAsyncEventArgs.ThrowException(SocketError error, CancellationToken cancellationToken)
         at System.Net.Sockets.Socket.AwaitableSocketAsyncEventArgs.System.Threading.Tasks.Sources.IValueTaskSource.GetResult(Int16 token)
         at System.Net.Sockets.Socket.<ConnectAsync>g__WaitForConnectWithCancellation|285_0(AwaitableSocketAsyncEventArgs saea, ValueTask connectTask, CancellationToken cancellationToken)
         at Npgsql.TaskTimeoutAndCancellation.ExecuteAsync(Func`2 getTaskFunc, NpgsqlTimeout timeout, CancellationToken cancellationToken)
         at Npgsql.Internal.NpgsqlConnector.ConnectAsync(NpgsqlTimeout timeout, CancellationToken cancellationToken)
         at Npgsql.Internal.NpgsqlConnector.ConnectAsync(NpgsqlTimeout timeout, CancellationToken cancellationToken)
         at Npgsql.Internal.NpgsqlConnector.RawOpen(SslMode sslMode, NpgsqlTimeout timeout, Boolean async, CancellationToken cancellationToken)
         at Npgsql.Internal.NpgsqlConnector.<Open>g__OpenCore|214_1(NpgsqlConnector conn, SslMode sslMode, NpgsqlTimeout timeout, Boolean async, CancellationToken cancellationToken)
         at Npgsql.Internal.NpgsqlConnector.Open(NpgsqlTimeout timeout, Boolean async, CancellationToken cancellationToken)
         at Npgsql.PoolingDataSource.OpenNewConnector(NpgsqlConnection conn, NpgsqlTimeout timeout, Boolean async, CancellationToken cancellationToken)
         at Npgsql.PoolingDataSource.<Get>g__RentAsync|33_0(NpgsqlConnection conn, NpgsqlTimeout timeout, Boolean async, CancellationToken cancellationToken)
         at Npgsql.NpgsqlConnection.<Open>g__OpenAsync|42_0(Boolean async, CancellationToken cancellationToken)
         at Microsoft.EntityFrameworkCore.Storage.RelationalConnection.OpenInternalAsync(Boolean errorsExpected, CancellationToken cancellationToken)
         at Microsoft.EntityFrameworkCore.Storage.RelationalConnection.OpenInternalAsync(Boolean errorsExpected, CancellationToken cancellationToken)
         at Microsoft.EntityFrameworkCore.Storage.RelationalConnection.OpenAsync(CancellationToken cancellationToken, Boolean errorsExpected)
         at Microsoft.EntityFrameworkCore.Storage.RelationalCommand.ExecuteReaderAsync(RelationalCommandParameterObject parameterObject, CancellationToken cancellationToken)
         at Microsoft.EntityFrameworkCore.Query.Internal.SingleQueryingEnumerable`1.AsyncEnumerator.InitializeReaderAsync(AsyncEnumerator enumerator, CancellationToken cancellationToken)
         at Npgsql.EntityFrameworkCore.PostgreSQL.Storage.Internal.NpgsqlExecutionStrategy.ExecuteAsync[TState,TResult](TState state, Func`4 operation, Func`4 verifySucceeded, CancellationToken cancellationToken)
         --- End of inner exception stack trace ---
         at Npgsql.EntityFrameworkCore.PostgreSQL.Storage.Internal.NpgsqlExecutionStrategy.ExecuteAsync[TState,TResult](TState state, Func`4 operation, Func`4 verifySucceeded, CancellationToken cancellationToken)
         at Microsoft.EntityFrameworkCore.Query.Internal.SingleQueryingEnumerable`1.AsyncEnumerator.MoveNextAsync()
         at Microsoft.EntityFrameworkCore.EntityFrameworkQueryableExtensions.ToListAsync[TSource](IQueryable`1 source, CancellationToken cancellationToken)
         at Microsoft.EntityFrameworkCore.EntityFrameworkQueryableExtensions.ToListAsync[TSource](IQueryable`1 source, CancellationToken cancellationToken)
         at ChoiceNote.WebAPI.Data.Repositories.NoteRepository.GetNotesAsync() in /source/ChoiceNote.WebAPI/Data/Repositories/NoteRepository.cs:line 20
         at ChoiceNote.WebAPI.Services.NoteService.GetNotesAsync() in /source/ChoiceNote.WebAPI/Services/NoteService.cs:line 19
         at ChoiceNote.WebAPI.Controllers.NoteController.GetNotesAsync() in /source/ChoiceNote.WebAPI/Controllers/NoteController.cs:line 26
         at lambda_method4(Closure, Object)
         at Microsoft.AspNetCore.Mvc.Infrastructure.ActionMethodExecutor.AwaitableObjectResultExecutor.Execute(ActionContext actionContext, IActionResultTypeMapper mapper, ObjectMethodExecutor executor, Object controller, Object[] arguments)
         at Microsoft.AspNetCore.Mvc.Infrastructure.ControllerActionInvoker.<InvokeActionMethodAsync>g__Awaited|12_0(ControllerActionInvoker invoker, ValueTask`1 actionResultValueTask)
         at Microsoft.AspNetCore.Mvc.Infrastructure.ControllerActionInvoker.<InvokeNextActionFilterAsync>g__Awaited|10_0(ControllerActionInvoker invoker, Task lastTask, State next, Scope scope, Object state, Boolean isCompleted)
         at Microsoft.AspNetCore.Mvc.Infrastructure.ControllerActionInvoker.Rethrow(ActionExecutedContextSealed context)
         at Microsoft.AspNetCore.Mvc.Infrastructure.ControllerActionInvoker.Next(State& next, Scope& scope, Object& state, Boolean& isCompleted)
         at Microsoft.AspNetCore.Mvc.Infrastructure.ControllerActionInvoker.<InvokeInnerFilterAsync>g__Awaited|13_0(ControllerActionInvoker invoker, Task lastTask, State next, Scope scope, Object state, Boolean isCompleted)
         at Microsoft.AspNetCore.Mvc.Infrastructure.ResourceInvoker.<InvokeFilterPipelineAsync>g__Awaited|20_0(ResourceInvoker invoker, Task lastTask, State next, Scope scope, Object state, Boolean isCompleted)
         at Microsoft.AspNetCore.Mvc.Infrastructure.ResourceInvoker.<InvokeAsync>g__Awaited|17_0(ResourceInvoker invoker, Task task, IDisposable scope)
         at Microsoft.AspNetCore.Mvc.Infrastructure.ResourceInvoker.<InvokeAsync>g__Awaited|17_0(ResourceInvoker invoker, Task task, IDisposable scope)
         at Microsoft.AspNetCore.Authorization.AuthorizationMiddleware.Invoke(HttpContext context)
         at Microsoft.AspNetCore.Authentication.AuthenticationMiddleware.Invoke(HttpContext context)
         at Microsoft.AspNetCore.Server.Kestrel.Core.Internal.Http.HttpProtocol.ProcessRequests[TContext](IHttpApplication`1 application)
```

## 考えられる原因 1

- 接続元のリージョン/ネットワーク制限
- Supabase は AWS の特定リージョンなので独自クラウドの Render が別リージョンだと接続拒否が起きやすい
- →Supabase、Render.com ともにシンガポールリージョンのため考えにくい

## 考えられる原因 2

- 接続文字列が Session pooler だと 1 コネクションを専有して接続を開きっぱなしになり枯渇する
- Transaction pooler なら 1 リクエストごとのコネクションなので枯渇しにくい
- →Transaction pooler に変更したが変化なし

## 考えられる原因 3

- ASP.NET Core + EF Core で DbContext のライフサイクルが長いかも
- →AddDbContext のデフォルト(Scoped)にしているため考えにくい

## 考えられる原因 4

- `likely due to a transient failure`とあるため瞬断が発生している
  - →DB 接続にリトライを設定したが、少し改善したものの根本解決はせず
    - しばらく接続できなくなることもあるため瞬断だけとは考えにくい

```cs
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseNpgsql(
        builder.Configuration.GetConnectionString("DefaultConnection")
        ?? throw new InvalidOperationException("Connection string not found."),
        npgsqlOptions =>
        {
            npgsqlOptions.EnableRetryOnFailure(
                maxRetryCount: 5, // 最大リトライ回数
                maxRetryDelay: TimeSpan.FromSeconds(30), // リトライ間隔の最大値
                errorCodesToAdd: null); // 追加でリトライ対象にするエラーコード
        })
);
```

## 考えられる原因 5

- コネクションプールが少ないと socket が足りずエラーになる様子
  - [reddit](https://www.reddit.com/r/Supabase/comments/1fan2ka/supabase_rejecting_conenctions_from_rendercom/?tl=ja)
- →Supabase で Database > Settings > Connection pooling configuration > Pool Size
  - 15→40 に変更(49 以上にすると以下エラーになる)したが解消せず
  - Reports > Database > Pooler 系は 10 以下でもダメだった

```text
プールサイズがデータベースの最大接続数(60)の80%を超えています
これにより、データベース接続が不安定になり、信頼性が低下する可能性があります。
```

## 考えられる原因 6

- 接続文字列の設定不足
  - [Npgsql](https://www.npgsql.org/doc/api/Npgsql.NpgsqlConnectionStringBuilder.html)
  - → 接続文字列にプール設定を追加したが解消せず

```text
User Id=postgres.{host};Password={password};Server=aws-1-ap-southeast-1.pooler.supabase.com;Port=6543;Database=postgres;Minimum Pool Size=0;Maximum Pool Size=10;
```

## 考えられる原因 7

- Render.com と Supabase の相性
- → 以下の構成で Azure App Service を構築し Supabase に接続したが解決せず

```text
サブスクリプション
└ リソースグループ
  ├ App Serviceプラン
  └ App Services
```

## 考えられる原因 8

- Supabase の接続 Pool/安定性の問題
- →Neon に移行したら解決した
- →Supabase の無料プランでのプーラーの安定性・同時接続上限・ネットワーク挙動が原因だったと結論
