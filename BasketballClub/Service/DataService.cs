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

		public async Task<(bool, string)> UpsertGame(Game newGame)
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
							return (false, "Game host not match");
						}
						else
						{
							res.Place = newGame.Place;
							res.StartTime = newGame.StartTime;
							res.EndTine = newGame.EndTine;

							await context.SaveChangesAsync();
							await OnGameUpdateAsync();
							return (true, "Update game " + newGame.Id + " success");
						}
					}
					else
					{
						await context.AddAsync(newGame);
						await context.SaveChangesAsync();
						await OnGameUpdateAsync();
						return (true, "Add game " + newGame.Id + " success");
					}
				}
			}
			catch (Exception e)
			{
				return (false, e.Message);
			}
		}
		public async Task<(bool, string)> RemoveGameByID(string gameId)
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
					return (true, "Delete game "+ gameId + " success");
				}
			}
			catch (Exception e)
			{
				return (false, e.Message);
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
		public async Task<(bool, string)> JoinGame(GameParticipant gameParticipant)
		{
			using (var scope = scopeFactory.CreateScope())
			{
				var context = scope.ServiceProvider.GetRequiredService<BasketballClubDBContext>();
				GameParticipant res = context.GameParticipants.FirstOrDefault(x => x.Id == gameParticipant.Id && x.ParticipantName == gameParticipant.ParticipantName);
				if (res != null)
				{
					if (gameParticipant.Amount > 0)
					{
						//update amount
						res.Amount = gameParticipant.Amount;
						await context.SaveChangesAsync();
						return (true, "Update game "+ gameParticipant.Id + " success");
					}
					else
					{
						return (false, "Update game " + gameParticipant.Id + " fail(join amount should more than 0)");
					}
				}
				else
				{
					if (gameParticipant.Amount > 0)
					{
						await context.AddAsync(gameParticipant);
						await context.SaveChangesAsync();
						return (true, "Join game " + gameParticipant.Id + " success");
					}
					else
					{
						return (false, "Join game " + gameParticipant.Id + " fail(join amount should more than 0)");
					}
				}
			}
		}

		public async Task<(bool, string)> LeaveGame(GameParticipant gameParticipant)
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
						return (true, "Leave " + gameParticipant.Id + " success");
					}
					else
					{
						return (false, "Leave " + gameParticipant.Id + " fail(leave amount should less than 0)");
					}
				}
				else
				{
					return (false, "Leave " + gameParticipant.Id + " fail("+ gameParticipant.Id + " not found)");
				}
			}
		}
		#endregion

		#region activity bulltin
		public Task<List<BulltinContent>> GetAllActivities()
		{
			using (var scope = scopeFactory.CreateScope())
			{
				var context = scope.ServiceProvider.GetRequiredService<BasketballClubDBContext>();
				return Task.FromResult(context.BulltinContents.ToList());
			}
		}
		public Task<BulltinContent> GetActivityByID(string id)
		{
			using (var scope = scopeFactory.CreateScope())
			{
				var context = scope.ServiceProvider.GetRequiredService<BasketballClubDBContext>();
				return Task.FromResult(context.BulltinContents.FirstOrDefault(x=>x.Id==id));
			}
		}
		public async Task<bool> AddNewActivity(BulltinContent newBulltinContent)
		{
			using (var scope = scopeFactory.CreateScope())
			{
				try
				{
					var context = scope.ServiceProvider.GetRequiredService<BasketballClubDBContext>();
					await context.AddAsync(newBulltinContent);
					await context.SaveChangesAsync();
					return true;
				}
				catch (Exception e)
				{
					return false;
				}
			}
		}
		#endregion
	}
}
