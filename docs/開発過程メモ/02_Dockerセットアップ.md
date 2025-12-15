# Docker セットアップ

## Dockerfile をルートに作成

- 参考: [# Docker コンテナーで ASP.NET Core アプリを実行する](https://learn.microsoft.com/ja-jp/aspnet/core/host-and-deploy/docker/building-net-docker-images?view=aspnetcore-10.0)

```Dockerfile
# buildステージ
## dotnet/sdk: CLI, Debug, 単体テスト, コンパイル用の SDK イメージ
FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
## コンテナ内の作業ディレクトリを指定
WORKDIR /source
## Dockerfile 階層のファイル全て→作業ディレクトリにコピー
COPY . ./
## 依存関係の復元
RUN dotnet restore
## out フォルダに release ビルドした DLL 等を出力
RUN dotnet publish -c release -o out
## 確認用
# RUN ls -la out

# アプリ実行ステージ
## dotnet/aspnet: 軽量な ASP.NET Core ランタイム・ライブラリイメージ
FROM mcr.microsoft.com/dotnet/aspnet:10.0
## コンテナ内の作業ディレクトリを指定
WORKDIR /source
## build ステージで publish したDLL等をカレントにコピー
COPY --from=build /source/out .
## コンテナのデフォルトの起動コマンドを指定
ENTRYPOINT ["dotnet", "{ProjectName}.dll"]
```

## Docker コンテナ起動

```zsh
# docker build . : 指定パスのDockerfileからDocker Image作成
# -t: イメージ名:タグ名
% docker build . -t {lower-case-image-name}:latest

# docker run {image-name}: Image名からコンテナ作成 & コマンド実行
# -i: 標準入力を開いた状態にする
# -t: 疑似端末を割り当てる
# -d: バックグラウンド実行
# --rm: ビルド時の一時コンテナを削除する
# -p: ポートマッピング(ローカル:コンテナ内)
# -name: コンテナ名
% docker run -d --rm -p 8080:8080 --name {container-name} {image-name}

# docker ps -a: コンテナ一覧を表示
% docker ps -a
```

指定したポートでアクセスできることを確認する
[localhost](http://localhost:8080/weatherforecast)

## docker-compose 作成

```yaml
services:
  { service-name }:
    # 指定ディレクトリのDockerfileからイメージをビルド
    build: .
    # Docker起動時に常に再起動するように設定
    restart: always
    # コンテナ名
    container_name: { container-name }
    # ポート(ローカル:コンテナ内)
    ports:
      - 8080:8080
    # 環境変数
    environment:
      ASPNETCORE_ENVIRONMENT: Development
```
