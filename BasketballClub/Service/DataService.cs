using BasketballClub.EFModels;
using DevExpress.Blazor.Popup.Internal;
using Microsoft.EntityFrameworkCore;
using System.Drawing;
using System.Security.Cryptography;

namespace BasketballClub.Service
{
	public class DataService
	{
		private readonly IServiceScopeFactory scopeFactory;


		public DataService(IServiceScopeFactory scopeFactory)
		{
			this.scopeFactory = scopeFactory;
		}

		#region User Info.
		public async Task<bool> RegisterUserInfo(UserInfo userInfo)
		{
			using (var scope = scopeFactory.CreateScope())
			{
				var context = scope.ServiceProvider.GetRequiredService<BasketballClubDBContext>();
				UserInfo res = context.UserInfos.FirstOrDefault(x => x.EmployeeId == userInfo.EmployeeId);
				if (res != null)
				{
					return false;
				}
				else
				{
					await context.AddAsync(userInfo);
					await context.SaveChangesAsync();
					return true;
				}
			}
		}
		public Task<UserInfo> GetUserInfoByName(string userName)
		{
			using (var scope = scopeFactory.CreateScope())
			{
				var context = scope.ServiceProvider.GetRequiredService<BasketballClubDBContext>();
				UserInfo res = context.UserInfos.FirstOrDefault(x => x.UserName == userName);
				if (res != null)
				{
					return Task.FromResult(res);
				}
				return Task.FromResult(new UserInfo() { EmployeeId = 0 });
			}
		}
		public Task<UserInfo> GetUserInfoByID(int employeeID)
		{
			using (var scope = scopeFactory.CreateScope())
			{
				var context = scope.ServiceProvider.GetRequiredService<BasketballClubDBContext>();
				UserInfo res = context.UserInfos.FirstOrDefault(x => x.EmployeeId == employeeID);
				if (res != null)
				{
					return Task.FromResult(res);
				}
				return Task.FromResult(new UserInfo() { EmployeeId = 0 });
			}
		}
		public async Task<bool> UpdateUserSessionKey(int id, string sessionKey)
		{
			bool updated = false;
			using (var scope = scopeFactory.CreateScope())
			{
				var context = scope.ServiceProvider.GetRequiredService<BasketballClubDBContext>();
				UserInfo userInfo = context.UserInfos.FirstOrDefault(x => x.EmployeeId == id);
				if (userInfo == null)
				{
					updated = false;
				}
				else
				{
					userInfo.SessionKey = sessionKey;
					await context.SaveChangesAsync();
					updated = true;
				}
			}
			return updated;
		}
		#endregion

		#region game
		public Task<List<Game>> GetAllGames()
		{
			using (var scope = scopeFactory.CreateScope())
			{
				var context = scope.ServiceProvider.GetRequiredService<BasketballClubDBContext>();
				return Task.FromResult(context.Games.ToList());
			}
		}
		public event Action<List<Game>>? GameUpdateAct;
		private async Task OnGameUpdateAsync() => GameUpdateAct?.Invoke(await GetAllGames());
		public Task<Game> GetGameByID(string gameId)
		{
			using (var scope = scopeFactory.CreateScope())
			{
				var context = scope.ServiceProvider.GetRequiredService<BasketballClubDBContext>();
				return Task.FromResult(context.Games.FirstOrDefault(x=>x.Id == gameId));
			}
		}

		public async Task<bool> AddNewGame(Game newGame)
		{
			try
			{
				using (var scope = scopeFactory.CreateScope())
				{
					var context = scope.ServiceProvider.GetRequiredService<BasketballClubDBContext>();
					Game res = context.Games.FirstOrDefault(x => x.Id == newGame.Id);
					if (res != null)
					{
						if (res.Host != newGame.Host)
						{
							return false;
						}
						else
						{
							res.Place = newGame.Place;
							res.StartTime = newGame.StartTime;
							res.EndTine = newGame.EndTine;
						}
					}
					else
					{
						await context.AddAsync(newGame);
					}
					await context.SaveChangesAsync();
					await OnGameUpdateAsync();
					return true;
				}
			}
			catch (Exception e)
			{
				return false;
			}
		}
		public async Task<bool> RemoveGameByID(string gameId)
		{
			try
			{
				using (var scope = scopeFactory.CreateScope())
				{
					var context = scope.ServiceProvider.GetRequiredService<BasketballClubDBContext>();
					//remove main game
					Game res = context.Games.FirstOrDefault(x => x.Id == gameId);
					if (res != null)
					{
						context.Remove(res);
						await context.SaveChangesAsync();
					}
					//remove all participants
					IEnumerable<GameParticipant> otherGameParticipants = context.GameParticipants.Where(x => x.Id == gameId);
					if (otherGameParticipants != null)
					{
						context.RemoveRange(otherGameParticipants);
						await context.SaveChangesAsync();
					}
					await OnGameUpdateAsync();
					return true;
				}
			}
			catch (Exception e)
			{
				return false;
			}
		}
		#endregion

		#region place
		public Task<List<Court>> GetAllCourts()
		{
			using (var scope = scopeFactory.CreateScope())
			{
				var context = scope.ServiceProvider.GetRequiredService<BasketballClubDBContext>();
				return Task.FromResult(context.Courts.ToList());
			}
		}
		#endregion

		#region game participants
		public Task<List<GameParticipant>> GetParticipantsByGameID(string gamdID)
		{
			using (var scope = scopeFactory.CreateScope())
			{
				var context = scope.ServiceProvider.GetRequiredService<BasketballClubDBContext>();
				return Task.FromResult(context.GameParticipants.AsNoTracking().Where(x=>x.Id == gamdID).ToList());
			}
		}
		public async Task<bool> JoinGame(GameParticipant gameParticipant)
		{
			using (var scope = scopeFactory.CreateScope())
			{
				var context = scope.ServiceProvider.GetRequiredService<BasketballClubDBContext>();
				GameParticipant res = context.GameParticipants.FirstOrDefault(x => x.Id == gameParticipant.Id && x.ParticipantName == gameParticipant.ParticipantName);
				if (res != null)
				{
					if (gameParticipant.Amount > 0)
					{
						res.Amount = gameParticipant.Amount;
						await context.SaveChangesAsync();
						return true;
					}
					else
					{
						return false;
					}
				}
				else
				{
					if (gameParticipant.Amount > 0)
					{
						await context.AddAsync(gameParticipant);
						await context.SaveChangesAsync();
						return true;
					}
					else
					{
						return false;
					}
				}
			}
		}

		public async Task<bool> LeaveGame(GameParticipant gameParticipant)
		{
			using (var scope = scopeFactory.CreateScope())
			{
				var context = scope.ServiceProvider.GetRequiredService<BasketballClubDBContext>();
				GameParticipant res = context.GameParticipants.FirstOrDefault(x => x.Id == gameParticipant.Id && x.ParticipantName == gameParticipant.ParticipantName);
				if (res != null)
				{
					if (gameParticipant.Amount <= 0)
					{
						context.Remove(res);
						await context.SaveChangesAsync();
						Game mainGame = await GetGameByID(gameParticipant.Id);
						if (mainGame.Host == gameParticipant.ParticipantName)
						{
							await RemoveGameByID(mainGame.Id);
							await context.SaveChangesAsync();
						}
						return true;
					}
					else
					{
						return false;
					}
				}
				else
				{
					return false;
				}
			}
		}
		#endregion
	}
}
