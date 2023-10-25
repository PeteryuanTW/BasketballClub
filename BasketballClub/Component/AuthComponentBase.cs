using BasketballClub.EFModels;
using BasketballClub.Service;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.JSInterop;

namespace BasketballClub.Component
{
    public class AuthComponentBase : ComponentBase
    {
        [Inject] AuthenticationStateProvider Asp { get; set; }
        [Inject] DataService dataService { get; set; }
        [Inject] IJSRuntime JsRuntime { get; set; }
        private CustomAuthenticationService auth { get; set; }
        public UserInfo userInfo => auth.userInfo;

        protected override void OnInitialized()
        {
            auth = (CustomAuthenticationService)Asp;
        }

        protected override async Task OnAfterRenderAsync(bool firstRender)
        {
            if (firstRender)
            {
                //Console.WriteLine("First render !");
                if (!auth.IsAuthenticated())
                    await UserReAuthorize();
            }

            if (auth.IsAuthenticated())
                await OnAfterRenderContextAsync(firstRender);
        }
        public virtual Task OnAfterRenderContextAsync(bool firstRender)
        {
            return Task.CompletedTask;
        }
        public async Task<bool> UserReAuthorize()
        {
            string sessionKeyCookie = await ReadCookie("TMBKBToken");

            // Check if empty
            if (string.IsNullOrEmpty(sessionKeyCookie)) { return false; }

            string[] keyParts = sessionKeyCookie.Split('.'); // ID, TIME, KEY

            // Check if the key is expired
            DateTime keyTime = DateTime.FromBinary(long.Parse(keyParts[1]));
            if (keyTime < DateTime.Now)
            {
                // Delete key form cookie and db and return false
                await UserLogoutAsync();
                return false;
            }

            int userID = int.Parse(keyParts[0]);

            UserInfo user = await dataService.GetUserInfoByID(userID);
            await auth.LoginAsync(user);

            return true;
        }

        public async Task<bool> UserLoginAsync(string username, string password)
        {
            // Check if the input is empty
            if (string.IsNullOrEmpty(username) || string.IsNullOrEmpty(password)) { return false; }

            // try to load the user from database
            var user = await dataService.GetUserInfoByName(username);

            // if the the id == -1 then the user or password is wrong
            if (user.EmployeeId == -1) { return false; }

            // Do a password check !
            if (!auth.VerifyPassword(user, password)) { return false; }

            // login the user into the ASP.NET Auth
            await auth.LoginAsync(user);

            // Store the session key with other info in Cookie
            int cookieActiveDuration = 360;
            DateTime expiryTime = DateTime.Now.AddMinutes(cookieActiveDuration);
            string cookieKey = string.Format("{0}.{1}.{2}", user.EmployeeId, expiryTime.ToBinary(), user.SessionKey);
            await WriteCookie("TMBKBToken", cookieKey, cookieActiveDuration);
            await dataService.UpdateUserSessionKey(user.EmployeeId, cookieKey);

            return true;
        }

        public async Task<bool> UserLogoutAsync()
        {
            await dataService.UpdateUserSessionKey(userInfo.EmployeeId, "");
            await DeleteCookie("TMBKBToken");
            await auth.LogoutAsync();
            return true;
        }

        public async Task WriteCookie(string cookieName, string cookieValue, int durationMinutes = 1)
        {
            await JsRuntime.InvokeVoidAsync("CookieWriter.Write", cookieName, cookieValue, DateTime.Now.AddMinutes(durationMinutes));
        }
        public async Task<string> ReadCookie(string cookieName)
        {
            return await JsRuntime.InvokeAsync<string>("CookieReader.Read", cookieName);
        }
        public async Task DeleteCookie(string cookieName)
        {
            await JsRuntime.InvokeVoidAsync("CookieRemover.Delete", cookieName);
        }
    }
}
