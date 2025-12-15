# Neon セットアップ

- New Project
  - Postgres version: 17 (2025/12/14 時点で 18 はプレビュー)
  - Cloud service provider: AWS
    - Azure は Region が少ない
  - Region: AWS Asia Pacific 1 (Singapore)
    - 2025/12/13 時点では日本選択不可: https://neon.com/docs/introduction/regions
  - Enable Neon Auth: 一旦オフ (後から Enable できる)
- 接続文字列取得
  - Dashboard の「Connect」
  - .NET 選択
  - Connections String を 「Copy snippet」

## Render へ接続文字列追加

- Environment
  - Environment Variables の 「Edit」
    - KEY: ConnectionStrings\_\_DefaultConnection
    - VALUE: 「Copy snippet」した値
