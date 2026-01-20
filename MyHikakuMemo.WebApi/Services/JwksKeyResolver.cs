using System.Security.Cryptography;
using System.Text.Json;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using Microsoft.IdentityModel.Tokens;

namespace MyHikakuMemo.WebApi.Services;

/// <summary>
/// JWT検証用のJWKS（JSON Web Key Set）から署名鍵を解決するサービス
/// キャッシング機能付きで、JWKS取得時のHTTPリクエストを削減します
/// </summary>
public class JwksKeyResolver(
    HttpClient httpClient,
    string jwksUrl,
    IMemoryCache cache,
    ILogger<JwksKeyResolver> logger)
{
    private readonly HttpClient _httpClient = httpClient ?? throw new ArgumentNullException(nameof(httpClient));
    private readonly string _jwksUrl = jwksUrl ?? throw new ArgumentNullException(nameof(jwksUrl));
    private readonly IMemoryCache _cache = cache ?? throw new ArgumentNullException(nameof(cache));
    private readonly ILogger<JwksKeyResolver> _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    private const string CacheKey = "JWKS_SecurityKeys";
    private const int CacheExpirationHours = 1;

    /// <summary>
    /// JWKSから署名鍵を解決します
    /// キャッシュを活用し、キャッシュミス時のみHTTPリクエストを発行します
    /// </summary>
    /// <param name="kid">JSON Web Token ヘッダーの "kid" (Key ID) パラメータ</param>
    /// <returns>条件に合致する署名鍵のコレクション</returns>
    public IEnumerable<SecurityKey> ResolveSigningKeys(string? kid)
        => ResolveSigningKeysAsync(kid).GetAwaiter().GetResult();

    /// <summary>
    /// JWKSから署名鍵を非同期で解決します
    /// キャッシュを活用し、キャッシュミス時のみHTTPリクエストを発行します
    /// </summary>
    /// <param name="kid">JSON Web Token ヘッダーの "kid" (Key ID) パラメータ</param>
    /// <returns>条件に合致する署名鍵のコレクション</returns>
    public async Task<IEnumerable<SecurityKey>> ResolveSigningKeysAsync(string? kid)
    {
        // キャッシュから取得を試みる
        if (TryGetFromCache(out var cachedKeys) && cachedKeys != null)
        {
            return FilterKeysByKid(cachedKeys, kid, fromCache: true);
        }

        // キャッシュミス時は JWKS エンドポイントから取得（同時アクセスはまとめて1回）
        var securityKeys = await _cache.GetOrCreateAsync(CacheKey, async entry =>
        {
            entry.AbsoluteExpirationRelativeToNow = TimeSpan.FromHours(CacheExpirationHours);
            return await FetchKeysFromJwksAsync();
        });

        return securityKeys == null
            ? Array.Empty<SecurityKey>()
            : FilterKeysByKid(securityKeys, kid, fromCache: false);
    }

    /// <summary>
    /// キャッシュから鍵を取得します
    /// </summary>
    private bool TryGetFromCache(out List<SecurityKey>? cachedKeys)
    {
        return _cache.TryGetValue(CacheKey, out cachedKeys) && cachedKeys != null;
    }

    /// <summary>
    /// JWKS エンドポイントから鍵を取得します
    /// </summary>
    private async Task<List<SecurityKey>> FetchKeysFromJwksAsync()
    {
        var attempts = 0;
        while (true)
        {
            attempts++;
            try
            {
                _logger.LogInformation("JWKS を取得中...");
                var jwksResponse = await _httpClient.GetStringAsync(_jwksUrl);
                var jwks = JsonSerializer.Deserialize<JsonElement>(jwksResponse);
                var keys = jwks.GetProperty("keys");

                var securityKeys = new List<SecurityKey>();

                foreach (var key in keys.EnumerateArray())
                {
                    var keyId = key.TryGetProperty("kid", out var kidProperty) ? kidProperty.GetString() : null;

                    var nValue = key.GetProperty("n").GetString();
                    var eValue = key.GetProperty("e").GetString();

                    var rsa = RSA.Create();
                    rsa.ImportParameters(new RSAParameters
                    {
                        Modulus = Base64UrlEncoder.DecodeBytes(nValue!),
                        Exponent = Base64UrlEncoder.DecodeBytes(eValue!)
                    });

                    var rsaKey = new RsaSecurityKey(rsa) { KeyId = keyId };
                    securityKeys.Add(rsaKey);
                }

                _logger.LogInformation("JWKS読み込み成功: {Count}個の鍵を取得", securityKeys.Count);
                return securityKeys;
            }
            catch (Exception ex)
            {
                if (attempts >= 2)
                {
                    _logger.LogError(ex, "JWKS読み込みエラー");
                    throw;
                }

                _logger.LogWarning(ex, "JWKS読み込み失敗。再試行します (試行回数: {Attempts})", attempts);
                await Task.Delay(TimeSpan.FromMilliseconds(200));
            }
        }
    }

    /// <summary>
    /// kid パラメータに基づいて鍵をフィルタリングします
    /// </summary>
    private IEnumerable<SecurityKey> FilterKeysByKid(List<SecurityKey> keys, string? kid, bool fromCache)
    {
        if (kid != null)
        {
            var matchedKeys = keys.Where(k => k.KeyId == kid).ToList();
            if (matchedKeys.Count != 0)
            {
                var source = fromCache ? "キャッシュ" : "新規取得";
                _logger.LogInformation("JWKS署名鍵を{Source}から取得 (kid: {Kid})", source, kid);
                return matchedKeys;
            }
        }
        else
        {
            var source = fromCache ? "キャッシュ" : "新規取得";
            _logger.LogInformation("JWKS署名鍵を{Source}から取得", source);
            return keys;
        }

        // kidに一致する鍵が見つからない場合は全て返す
        return keys;
    }
}
