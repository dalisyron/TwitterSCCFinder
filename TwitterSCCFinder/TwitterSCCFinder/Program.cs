using LinqToTwitter;
using System;
using System.Diagnostics;
using System.Linq;
using System.Collections.Generic;

namespace TwitterTest
{
    class Program
    {
        private static TwitterContext twC;

        static List <string> getFriends(string user_name)
        {
            var user = twC.User.Where(u => u.Type == UserType.Show && u.ScreenName == user_name)
                .SingleOrDefault();

            var followerBy100s = twC.Friendship
                .Where(f => f.Type == FriendshipType.FriendIDs && f.ScreenName == user.ScreenName)
                .SingleOrDefault()?.IDInfo.IDs
                .Select((id, idx) => new { id, idx })
                .GroupBy(e => e.idx / 100)
                .Select(g => string.Join(",", g.Select(e => e.id)));

            var result = followerBy100s.SelectMany(ids => twC.User.Where(u => u.Type == UserType.Lookup && u.UserIdList == ids));
            List<string> res = new List<string>();
            
            foreach (User r in result)
            {
                res.Add(r.ScreenNameResponse);
            }
            return res;
        }

        static List <string> getFollowers(string user_name)
        {
            var user = twC.User.Where(u => u.Type == UserType.Show && u.ScreenName == user_name)
                .SingleOrDefault();

            var followerBy100s = twC.Friendship
                .Where(f => f.Type == FriendshipType.FollowerIDs && f.ScreenName == user.ScreenName)
                .SingleOrDefault()?.IDInfo.IDs
                .Select((id, idx) => new { id, idx })
                .GroupBy(e => e.idx / 100)
                .Select(g => string.Join(",", g.Select(e => e.id)));

            var result = followerBy100s.SelectMany(ids => twC.User.Where(u => u.Type == UserType.Lookup && u.UserIdList == ids));
            List<string> res = new List<string>();

            foreach (User r in result)
            {
                res.Add(r.ScreenNameResponse);
            }
            return res;
        }

        static bool Follows(string source_user, string dest_user)
        {
            var friendship =
                (from friend in twC.Friendship
                 where friend.Type == FriendshipType.Show &&
                       friend.SourceScreenName == source_user &&
                       friend.TargetScreenName == dest_user
                 select friend)
                .First();

            return friendship.TargetRelationship.FollowedBy;
        }

        static void Main(string[] args)
        {
            IAuthorizer token = GetOauthToken();
            twC = new TwitterContext(token);

            
            Console.Read();
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
