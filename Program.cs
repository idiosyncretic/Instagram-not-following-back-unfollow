


using InstagramApiSharp;
using InstagramApiSharp.API;
using InstagramApiSharp.API.Processors;
using InstagramApiSharp.Classes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;



namespace InstagramApiSharpTest
{
    class Program
    {
        static async Task Main(string[] args)
        {
            // set the text color 
            void WriteLine(string text, ConsoleColor color)
            {
                Console.ForegroundColor = color;
                Console.WriteLine(text);
                Console.ResetColor();
            }
            WriteLine("Username: ", ConsoleColor.Green);
            string usr = Console.ReadLine();
            Console.WriteLine("Password: ", ConsoleColor.Green);
            string pswd = Console.ReadLine();
            // create an instance of the api
            WriteLine("Trying to login... ", ConsoleColor.Blue);
            var userSession = new UserSessionData
            {

                UserName = usr,
                Password = pswd
            };
            var api = InstagramApiSharp.API.Builder.InstaApiBuilder.CreateBuilder()
                .SetUser(userSession)
                .Build();

            // login to the account
            var loginResult = await api.LoginAsync();
            if (loginResult.Succeeded)
            {
                WriteLine("Logged in successfully.", ConsoleColor.Green);
            }
            else
            {
                WriteLine("Login failed: " + loginResult.Info.Message, ConsoleColor.Red);
                return;
            }

            // get the list of users that are followed by the account
            var followingResult = await api.UserProcessor.GetUserFollowingAsync(userSession.UserName, PaginationParameters.MaxPagesToLoad(5));
            if (followingResult.Succeeded)
            {
                WriteLine("Retrieved following list.", ConsoleColor.Green);
            }
            else
            {
                WriteLine("Failed to get following list: " + followingResult.Info.Message, ConsoleColor.Red);
                return;
            }

            // get the list of users that are following the account
            var followersResult = await api.UserProcessor.GetUserFollowersAsync(userSession.UserName, PaginationParameters.MaxPagesToLoad(5));
            if (followersResult.Succeeded)
            {
                WriteLine("Retrieved followers list.", ConsoleColor.Green);
            }
            else
            {
                WriteLine("Failed to get followers list: " + followersResult.Info.Message, ConsoleColor.Red);
                return;
            }

            // find the users that are followed by the account but aren't following back
            var following = followingResult.Value.Select(x => x.UserName).ToHashSet();
            var followers = followersResult.Value.Select(x => x.UserName).ToHashSet();
            var notFollowingBack = following.Except(followers).ToList();

            // show the result
            WriteLine("The users that are followed by the account but aren't following back are:", ConsoleColor.Blue);

            foreach (var user in notFollowingBack)
            {
                WriteLine(user,ConsoleColor.Cyan);
            }
            WriteLine($"Total: {notFollowingBack.Count}", ConsoleColor.Blue);


            Random random = new Random();
            

            //*  iterate over the list of users
            foreach (var user in notFollowingBack)
            {
                // get the user id (pk) from the username
                var userInfo = await api.UserProcessor.GetUserInfoByUsernameAsync(user);
                var userId = userInfo.Value.Pk;

                // unfollow the user
                var unfollowResult = await api.UserProcessor.UnFollowUserAsync(userId);


                // check if the operation was successful
                if (unfollowResult.Succeeded)
                {
                    // print a message
                    WriteLine("Unfollowed " + user,ConsoleColor.Green);
                }
                else
                {
                    // print an error
                    WriteLine("Failed to unfollow " + user + ": " + unfollowResult.Info.Message, ConsoleColor.Red);
                }

                // wait for 20 seconds
                WriteLine($"Please Wait ({random.Next(15000, 35000) / 1000}s) for next unfollow.",ConsoleColor.Cyan);
                await Task.Delay(random.Next(15000, 35000));
            }
        }
    }
}

