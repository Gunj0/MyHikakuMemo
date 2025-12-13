# buildステージ
FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /source
COPY . ./
RUN dotnet restore
RUN dotnet publish -c release -o out

# アプリ実行ステージ
FROM mcr.microsoft.com/dotnet/aspnet:10.0
WORKDIR /source
COPY --from=build /source/out .
ENTRYPOINT ["dotnet", "MyHikakuMemo.WebApi.dll"]
