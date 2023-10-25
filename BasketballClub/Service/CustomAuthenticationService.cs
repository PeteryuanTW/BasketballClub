using BasketballClub.Data;
using BasketballClub.EFModels;
using Microsoft.AspNetCore.Components.Authorization;
using System.Security.Claims;
using System.Security.Cryptography;

namespace BasketballClub.Service
{
	public class CustomAuthenticationService : AuthenticationStateProvider
	{
		public enum Roles
		{
			Anonymous,
			Standard,
			Administrator
		}
		public UserInfo userInfo { get; set; } = new();
		public bool IsAuthenticated() => (userInfo.EmployeeId != 0);
		public readonly string[] RoleTypes = Enum.GetNames<Roles>();
		public override Task<AuthenticationState> GetAuthenticationStateAsync()
		{
			AuthenticationState auth = new(new ClaimsPrincipal(new ClaimsIdentity()));

			if (IsAuthenticated())
			{
				//if not empty then populate the Claims with Data!
				List<Claim> claims = new()
				{
					new Claim(ClaimTypes.Name, userInfo.UserName),
					new Claim(ClaimTypes.Role, RoleTypes[0])
				};

				// Fill roles of the user
				int roleIndex = userInfo.UserRole;// Array.IndexOf(RoleTypes, UserInfo.UserRole.ToString());
				if (roleIndex != -1)
				{
					for (int i = roleIndex; i > 0; i--)
					{
						claims.Add(new Claim(ClaimTypes.Role, RoleTypes[i]));
					}
				}

				// create a new state with the roles
				auth = new(new ClaimsPrincipal(new ClaimsIdentity(claims, "YourAppNameHere")));
			}

			return Task.FromResult(auth);
		}

		public async Task LoginAsync(UserInfo userInfo)
		{
			await Task.Run(() => { }); // holder so it would be async
			this.userInfo = userInfo;

			// Create Random User sessionKey
			byte[] byteArray = RandomNumberGenerator.GetBytes(8);
			string sessionKey = Convert.ToBase64String(byteArray);
			this.userInfo.SessionKey = sessionKey;

			// Notify the ASP.NET services that a user change happend
			NotifyAuthenticationStateChanged(GetAuthenticationStateAsync());
		}
		public async Task LogoutAsync()
		{
			await Task.Run(() =>
			{
				userInfo = new();
				// Notify the ASP.NET services that a user change happend
				NotifyAuthenticationStateChanged(GetAuthenticationStateAsync());
			}); // holder so it would be async 
		}
		public bool VerifyPassword(UserInfo user, string password)
		{
			//please change this to the proper algo
			return PasswordHelper.VerifyPassword(password, user.Password);
		}
	}
}
