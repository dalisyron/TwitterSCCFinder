using LinqToTwitter;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading;
using Json.Net;
using Newtonsoft.Json;

namespace TwitterTest
{
    class Program
    {
        private static TwitterContext twC;

        static List<string> user_names;

        static List<string> getFriends(string user_name)
        {
            var followerBy100s = twC.Friendship
                .Where(f => f.Type == FriendshipType.FriendIDs && f.ScreenName == user_name)
                .SingleOrDefault()?.IDInfo.IDs
                .Select((id, idx) => new { id, idx })
                .GroupBy(e => e.idx / 100)
                .Select(g => string.Join(",", g.Select(e => e.id)));
            List<string> res = new List<string>();
            //author
            foreach (string s in followerBy100s)
            {
                res.Add(s);
            }
            if (res.Count > 0)
            {
                return res[0].Split(',').ToList();
            }
            else
            {
                return new List<string>();
            }
        }

        static List<string> getFriendsByID(string idt)
        {
            var followerBy100s = twC.Friendship
                .Where(f => f.Type == FriendshipType.FriendIDs && f.UserID == idt)
                .SingleOrDefault()?.IDInfo.IDs
                .Select((id, idx) => new { id, idx })
                .GroupBy(e => e.idx / 100)
                .Select(g => string.Join(",", g.Select(e => e.id)));
            List<string> res = new List<string>();
            foreach (string s in followerBy100s)
            {
                res.Add(s);
            }
            if (res.Count > 0)
            {
                return res[0].Split(',').ToList();
            }
            else
            {
                return new List<string>();
            }
        }
        static List<string> getFollowers(string user_name)
        {


            var followerBy100s = twC.Friendship
                .Where(f => f.Type == FriendshipType.FollowerIDs && f.ScreenName == user_name)
                .SingleOrDefault()?.IDInfo.IDs
                .Select((id, idx) => new { id, idx })
                .GroupBy(e => e.idx / 100)
                .Select(g => string.Join(",", g.Select(e => e.id)));
            List<string> res = new List<string>();
            foreach (string s in followerBy100s)
            {
                res.Add(s);
            }
            if (res.Count > 0)
            {
                return res[0].Split(',').ToList();
            }
            else
            {
                return new List<string>();
            }
        }
        static List<string> getFollowersByID(string idt)
        {


            var followerBy100s = twC.Friendship
                .Where(f => f.Type == FriendshipType.FollowerIDs && f.UserID == idt)
                .SingleOrDefault()?.IDInfo.IDs
                .Select((id, idx) => new { id, idx })
                .GroupBy(e => e.idx / 100)
                .Select(g => string.Join(",", g.Select(e => e.id)));
            List<string> res = new List<string>();
            foreach (string s in followerBy100s)
            {
                res.Add(s);
            }
            if (res.Count > 0)
            {
                return res[0].Split(',').ToList();
            }
            else
            {
                return new List<string>();
            }
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

        static List <string> Propagate(List <string> src_users, int criteria)
        {
            List<string> add = new List<string>();
            SortedDictionary<string, bool> sourceMark = new SortedDictionary<string, bool>();
            SortedDictionary<string, bool> candidMark = new SortedDictionary<string, bool>();
            SortedDictionary<string, int> sourceCount = new SortedDictionary<string, int>();
            SortedDictionary<string, bool> sinkFlag = new SortedDictionary<string, bool>();
            List<string> candidates = new List<string>();

            foreach (string user in src_users)
            {
                sourceMark[user] = true;
            }
            foreach (string user in src_users)
            {
                List<string> friends = new List<string>();
                while (friends.Count == 0)
                {
                    try
                    {
                        friends = getFriendsByID(user);
                        break;
                    }
                    catch (System.AggregateException ex)
                    {
                        var x = ex.InnerException;
                        string err = x.ToString().Split('\n')[0];
                        if (err.Contains("Rate limit"))
                        {
                            Console.WriteLine(err);
                            Console.WriteLine("Waiting 15 minutes for new API call window");
                            Thread.Sleep(60 * 15 * 1000);
                            Console.WriteLine("Done sleeping");
                        } else
                        {
                            break;
                        }
                    }
                }
                Console.Write("current user : ");
                Console.WriteLine(user);
                foreach (string f in friends) {
                    if (!sourceMark.ContainsKey(f))
                    {
                        if (sourceCount.ContainsKey(f))
                        {
                            sourceCount[f] += 1;
                        } else
                        {
                            sourceCount[f] = 1;
                        }
                        if (!candidMark.ContainsKey(f)) {
                            candidates.Add(f);
                            candidMark[f] = true;
                        }
                    }
                }
            }
            
            foreach (string u in candidates)
            {
                if (sourceCount[u] > criteria)
                {
                    add.Add(u);
                }
            }
            List<string> res = new List<string>();
            foreach (string u in src_users)
            {
                res.Add(u);
            }
            foreach (string u in add)
            {
                res.Add(u);
            }
            return res;
        }

        static Dictionary<string, List <string>> getAdjacencyList(List <string> users)
        {
            Dictionary<string, List<string>> adj = new Dictionary<string, List<string>>();
            SortedDictionary<string, bool> src_mark = new SortedDictionary<string, bool>();
            foreach (string user in users)
            {
                src_mark[user] = true;
            }
            foreach (string user in users)
            {
                List<string> friends = new List<string>();
                while (friends.Count == 0)
                {
                    try
                    {
                        friends = getFriendsByID(user);
                        List<string> tmp = new List<string>();
                        foreach (string u in friends)
                        {
                            if (src_mark.ContainsKey(u))
                            {
                                tmp.Add(u);
                            }
                        }
                        adj[user] = tmp;
                        break;
                    }
                    catch (System.AggregateException ex)
                    {
                        var x = ex.InnerException;
                        string err = x.ToString().Split('\n')[0];
                        if (err.Contains("Rate limit"))
                        {
                            Console.WriteLine(err);
                            Console.WriteLine("Waiting 15 minutes for new API call window");
                            Thread.Sleep(60 * 15 * 1000);
                            Console.WriteLine("Done sleeping");
                        }
                        else
                        {
                            break;
                        }
                    }
                }
            }
            return adj;
        }
        static void Main(string[] args)
        {
            IAuthorizer token = GetOauthToken();
            twC = new TwitterContext(token);

            List<string> init_users = new List<string>();
            List<string> user_ids = new List<string>();
            Dictionary<string, List<string>> adjacencyList = new Dictionary<string, List<string>>();

            if (File.Exists(TwitterSCCFinder.Settings.data_path + "\\initusers.txt"))
            {
                Console.WriteLine("Located initusers history");
                init_users = File.ReadAllText(TwitterSCCFinder.Settings.data_path + "\\initusers.txt").Split('\n').ToArray().ToList();
            }
            else
            {
                init_users = getFriends("DevCampIUST");
            }

            if (File.Exists(TwitterSCCFinder.Settings.data_path + "\\propagate1.txt"))
            {
                Console.WriteLine("Located propagate history");
                user_ids = File.ReadAllText(TwitterSCCFinder.Settings.data_path + "\\propagate1.txt").Split('\n').ToArray().ToList();
            }
            else
            {
                user_ids = Propagate(init_users, 5);
            }
            foreach (string user in user_ids)
            {
                Console.WriteLine(user);
            }
            if (File.Exists(TwitterSCCFinder.Settings.data_path + "\\friends.JSON"))
            {
                Console.WriteLine("Located neighbors history");

                string json = File.ReadAllText(TwitterSCCFinder.Settings.data_path + "\\friends.JSON");
                var values = JsonConvert.DeserializeObject<Dictionary<string, string>>(json);
                foreach (KeyValuePair<string, string> user in values)
                {
                    adjacencyList[user.Key] = user.Value.Split(',').ToArray().ToList();
                    if (adjacencyList[user.Key][0] == "")
                    {
                        adjacencyList[user.Key] = new List<string>();
                    }
                }
            }
            else
            {
                adjacencyList = getAdjacencyList(user_ids);
            }
            List<string> node_labels = new List<string>();
            SortedDictionary<string, int> ind = new SortedDictionary<string, int>();
            
            int cnt = 0;
            foreach (KeyValuePair<string, List<string>> user in adjacencyList)
            {
                cnt += adjacencyList[user.Key].Count;
                node_labels.Add(user.Key);
            }
            for (int i = 0; i < node_labels.Count; i++)
            {
                ind[node_labels[i]] = i;
            }
            long[][] edges = new long[cnt][];
            int cur_ind = 0;
            foreach (KeyValuePair<string, List<string>> user in adjacencyList)
            {
                foreach(string v in adjacencyList[user.Key])
                {
                    edges[cur_ind] = new long[2];
                    edges[cur_ind][0] = ind[user.Key];
                    edges[cur_ind][1] = ind[v];
                    cur_ind++;
                }
            }
            long ans = SCCAlgorithm.StronglyConnected.Solve(node_labels.Count, edges);
            Console.WriteLine(node_labels.Count);
            Console.WriteLine(ans);

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
