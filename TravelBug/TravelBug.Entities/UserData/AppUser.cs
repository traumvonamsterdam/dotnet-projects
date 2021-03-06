﻿using System;
using System.Collections.Generic;
using Microsoft.AspNetCore.Identity;

namespace TravelBug.Entities.UserData
{
  public class AppUser : IdentityUser
  {
    public string DisplayName { get; set; }
    public string Bio { get; set; }
    public DateTimeOffset LastLogin { get; set; }
    public virtual UserPhoto ProfilePicture { get; set; } = new UserPhoto();
    public virtual ICollection<Blog> Blogs { get; set; } = new List<Blog>();
    public virtual ICollection<UserFollowing> Followings { get; set; } = new List<UserFollowing>();
    public virtual ICollection<UserFollowing> Followers { get; set; } = new List<UserFollowing>();
    public virtual ICollection<RefreshToken> RefreshTokens { get; set; } = new List<RefreshToken>();
  }
}
