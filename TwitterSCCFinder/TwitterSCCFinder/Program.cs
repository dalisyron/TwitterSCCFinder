using LinqToTwitter;
using System;
using System.Diagnostics;
using System.Linq;

//
// You need to first add the "LinqToTwitter" nuget package from 
// nuget package manager (right click on solution...)
// 

namespace TwitterTest
{
    class Program
    {
        static void Main(string[] args)
        {
            IAuthorizer token = GetOauthToken();
            using (TwitterContext twC = new TwitterContext(token))
            {
                var user = twC.User.Where(u => u.Type == UserType.Show && u.ScreenName == "DevCampIUST")
                    .SingleOrDefault();

                var followerBy100s = twC.Friendship
                    .Where(f => f.Type == FriendshipType.FriendIDs && f.ScreenName == user.ScreenName)
                    .SingleOrDefault()?.IDInfo.IDs
                    .Select((id, idx) => new { id, idx })
                    .GroupBy(e => e.idx / 100)
                    .Select(g => string.Join(",", g.Select(e => e.id)));

                var result = followerBy100s.SelectMany(ids => twC.User.Where(u => u.Type == UserType.Lookup && u.UserIdList == ids));

                Console.WriteLine(user.ScreenNameResponse);

                foreach (User r in result)
                {
                    Console.WriteLine(r.ScreenNameResponse);
                }
                Console.Read();
            }
        }

        private static IAuthorizer GetOauthToken()
        {
            Console.Write("Inorder to access twitter's API please enter your Consumer Key : ");
            string ck = Console.ReadLine();
            Console.Write("Inorder to access twitter's API please enter your Consumer Secret Key : ");
            string cs = Console.ReadLine();

            var auth = new PinAuthorizer()
            {
                CredentialStore = new InMemoryCredentialStore
                {
                    ConsumerKey = ck,
                    ConsumerSecret = cs
                },
                GoToTwitterAuthorization =
                    pageLink =>
                        Process.Start(pageLink),
                GetPin = () =>
                {
                    Console.WriteLine(
                        "\nAfter authorizing this application, Twitter " +
                        "will give you a 7-digit PIN Number.\n");
                    Console.Write("Enter the PIN number here: ");
                    return Console.ReadLine();
                }
            };
            auth.AuthorizeAsync().Wait();
            return auth;
        }
    }
}
