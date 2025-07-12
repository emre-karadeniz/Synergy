using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Synergy.Framework.Auth.Configuration;
using Synergy.Framework.Auth.Data;
using Synergy.Framework.Auth.Entities;

namespace Synergy.Framework.Auth.Services;

public class TokenService
{
    private readonly TokenOptions _tokenOptions;
    private readonly SynergyIdentityDbContext _dbContext;

    public TokenService(TokenOptions tokenOptions, SynergyIdentityDbContext dbContext)
    {
        _tokenOptions = tokenOptions;
        _dbContext = dbContext;
    }

    public string GenerateToken(string userId, IEnumerable<Claim>? extraClaims = null)
    {
        var claims = new List<Claim>
        {
            new Claim(ClaimTypes.NameIdentifier, userId),
            new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
        };
        if (extraClaims != null)
            claims.AddRange(extraClaims);

        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_tokenOptions.SigningKey));
        var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);
        var token = new JwtSecurityToken(
            issuer: _tokenOptions.Issuer,
            audience: _tokenOptions.Audience,
            claims: claims,
            expires: DateTime.UtcNow.AddMinutes(_tokenOptions.AccessTokenExpiration),
            signingCredentials: creds
        );
        return new JwtSecurityTokenHandler().WriteToken(token);
    }

    public async Task<RefreshToken> GenerateRefreshTokenAsync(string userId, string ip)
    {
        var refreshToken = new RefreshToken
        {
            Token = Convert.ToBase64String(Guid.NewGuid().ToByteArray()),
            UserId = userId,
            ExpiresAt = DateTime.UtcNow.AddMinutes(_tokenOptions.RefreshTokenExpiration),
            CreatedByIp = ip
        };
        await _dbContext.Set<RefreshToken>().AddAsync(refreshToken);
        await _dbContext.SaveChangesAsync();
        return refreshToken;
    }

    public async Task SaveUserSessionAsync(string userId, string accessToken, string refreshToken, string ip)
    {
        var session = new UserTokenSession
        {
            UserId = userId,
            AccessToken = accessToken,
            RefreshToken = refreshToken,
            IpAddress = ip
        };
        await _dbContext.Set<UserTokenSession>().AddAsync(session);
        await _dbContext.SaveChangesAsync();
    }

    public async Task<RefreshToken?> GetRefreshTokenAsync(string token)
    {
        return await _dbContext.Set<RefreshToken>().FirstOrDefaultAsync(rt => rt.Token == token);
    }

    public async Task<UserTokenSession?> GetUserSessionByRefreshTokenAsync(string refreshToken)
    {
        return await _dbContext.Set<UserTokenSession>().FirstOrDefaultAsync(us => us.RefreshToken == refreshToken);
    }

    public async Task RevokeRefreshTokenAsync(string token)
    {
        var refreshToken = await GetRefreshTokenAsync(token);
        if (refreshToken != null)
        {
            refreshToken.IsRevoked = true;
            _dbContext.Set<RefreshToken>().Update(refreshToken);
        }

        var session = await GetUserSessionByRefreshTokenAsync(token);
        if (session != null)
        {
            session.RevokedAt = DateTime.UtcNow;
            _dbContext.Set<UserTokenSession>().Update(session);
        }

        await _dbContext.SaveChangesAsync();
    }
}
