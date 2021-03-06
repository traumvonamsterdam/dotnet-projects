﻿using AutoMapper;
using Microsoft.AspNetCore.Identity;
using System;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using TravelBug.Dtos;
using TravelBug.Entities.UserData;
using TravelBug.Infrastructure.Exceptions;

namespace TravelBug.Infrastructure.UserLogic
{
  public interface IRefreshTokenService
  {
    Task<User> GetRefreshToken(string refreshToken);
  }

  public class RefreshTokenService : IRefreshTokenService
  {
    private readonly UserManager<AppUser> _userManager;
    private readonly IJwtGenerator _jwtGenerator;
    private readonly IUserAccessor _userAccessor;
    private readonly IMapper _mapper;

    public RefreshTokenService(UserManager<AppUser> userManager, IJwtGenerator jwtGenerator, IUserAccessor userAccessor, IMapper mapper)
    {
      _userManager = userManager;
      _jwtGenerator = jwtGenerator;
      _userAccessor = userAccessor;
      _mapper = mapper;
    }

    public async Task<User> GetRefreshToken(string refreshToken
        )
    {
      var user = await _userManager.FindByNameAsync(_userAccessor.GetCurrentUsername());

      var oldToken = user.RefreshTokens.SingleOrDefault(x => x.Token == refreshToken);

      // If token is invalid, return 401
      if (oldToken != null && !oldToken.IsActive) throw new RestException(HttpStatusCode.Unauthorized);

      // Revoke the old token if it exists
      if (oldToken != null)
        oldToken.Revoked = DateTime.UtcNow;

      // Add new token to user and save to database
      var newRefreshToken = _jwtGenerator.GenerateRefreshToken();
      user.RefreshTokens.Add(newRefreshToken);
      await _userManager.UpdateAsync(user);

      // Return user data
      var returnedUser = _mapper.Map<AppUser, User>(user);
      returnedUser.Token = _jwtGenerator.CreateToken(user);
      returnedUser.RefreshToken = newRefreshToken.Token;

      return returnedUser;
    }
  }
}
