# Docker 起動時 dotnet エラー

```zsh
% docker run {image-name}
The command could not be loaded, possibly because:

- You intended to execute a .NET application:
  The application '{ProjectName}.dll' does not exist or is not a managed .dll or .exe.
- You intended to execute a .NET SDK command:
  No .NET SDKs were found.

Download a .NET SDK:
https://aka.ms/dotnet/download

Learn about SDK resolution:
https://aka.ms/dotnet/sdk-not-found
```

## 解決策

Dockerfile 記載の dll 名の大文字小文字が誤っていた。

```Dockerfile
ENTRYPOINT ["dotnet", "{ProjectName}.dll"]
```
