# Render セットアップ

## Render 設定

- Create new Project
  - Project name: {ProjectName}
  - Environment name: Production
- Settings
  - General
    - Region: Singapore
      - 2025/12/13 時点では日本選択不可: https://render.com/docs/regions
    - Instance Type: Free(0.1CPU 512MB)
  - Build & Deploy
    - Repository: GitHub のリポジトリを指定すると CI/CD が構築できる
    - Branch: main
    - Git Credentials: (you)
    - Root Directory: (空欄)
    - Build Filters: なし
    - Registry Credential: No credential
    - Dockerfile Path: ./Dockerfile
      - Defaults to ./Dockerfile. とあるが認識されなかったため明示
    - Docker Build Context Directory: $ .
    - Docker Command: (空欄)
    - Pre-Deploy Command: $ (空欄)
    - Auto-Deploy: On Commit
      - プッシュでデプロイされる
  - Custom Domains
    - あとで設定
