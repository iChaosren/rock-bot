using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Discord;
using Discord.Net;
using Discord.Net.WebSockets;
using Discord.Net.Rest;
using Discord.Commands;
using rock_bot.Models;
using System.Data.Entity;

namespace rock_bot
{
    public class Program
    {
        public static void Main(string[] args) => new Program().Start().GetAwaiter().GetResult();

        private DiscordClient bot;
        private RockModel db = new RockModel();

        public async Task Start()
        {
            bot = new DiscordClient();

            int MessagesInConsole = 0;

            bot.MessageReceived += async (s, e) =>
            {
                if (e.Message.User.Id == 249522425794920448)
                    return;

                if(MessagesInConsole >= 10)
                {
                    Console.Clear();
                    MessagesInConsole = 0;
                }

                Console.WriteLine(e.Channel.Name + " | " + e.Message);
                MessagesInConsole++;

                foreach(restricted_keywords special in Shuffle(await db.restricted_keywords.ToListAsync()))
                {
                    if(e.Message.RawText.StartsWith("rock " + special.keyword) || e.Message.RawText.StartsWith("/" + special.keyword))
                    {
                        switch(special.keyword)
                        {
                            case "add":
                                Add(e);
                                break;
                            case "clear":

                                return;

                                long UserID = Convert.ToInt64(e.User.Id);

                                /*Console.WriteLine("UserID Sending Message: " + UserID.ToString());

                                foreach (user all in await db.users.ToListAsync())
                                    Console.WriteLine(all.id);*/

                                user current = null;
                                try { current = await db.users.FindAsync(UserID); } catch(Exception ex) { Console.WriteLine(ex.Message + "\n" + ex.StackTrace); return; }
                                if (current == null) return;

                                if(current.permissions.Count(x => x.name == special.keyword) > 0)
                                {
                                    Channel paintbucket = e.Channel;
                                    await e.Channel.Delete();
                                    Channel newChannel = await e.Server.CreateChannel(paintbucket.Name, paintbucket.Type);

                                    foreach(Channel.PermissionOverwrite permission in paintbucket.PermissionOverwrites)
                                        foreach(Role role in paintbucket.Users.Select(x => x.Roles))
                                            await newChannel.AddPermissionsRule(role, permission.Permissions);
                                    
                                    return;
                                }
                                else
                                {
                                    await e.Channel.SendMessage("Sorry...\nYou don't have permission to do that... :robot::neutral_face::robot:");
                                }
                                break;
                            case "meme":
                                string content = e.Message.RawText.Substring(e.Message.RawText.ToUpper().IndexOf("MEME") + 4);
                                if (content.Length == 0) return;
                                if(content[0] == ' ') { content = content.Substring(1).ToUpper(); }
                                //if (content.IndexOf(' ') > 0) { content = content.Substring(content.IndexOf(' ')); }

                                List<meme> PossibleMemes = await db.memes.Where(x => x.keyword.ToUpper() == content).ToListAsync();

                                List<meme> PaintBucket = PossibleMemes;

                                foreach(meme me in PaintBucket)
                                {
                                    if(me.lastusage.HasValue && me.maxspan.HasValue)
                                    {
                                        if ((DateTime.Now - me.lastusage.Value).TotalMinutes < me.maxspan.Value)
                                            PossibleMemes.Remove(me);
                                    }
                                }

                                if(PossibleMemes.Count > 0)
                                {
                                    //TODO: Log Usage
                                    await e.Channel.SendMessage(PossibleMemes[new Random().Next(0, (PossibleMemes.Count - 2))].response);
                                }                                   

                                return;
                        }

                        return;
                    }

                    if(special.keyword.StartsWith("#"))
                    {
                        string NoHash = special.keyword.Substring(1);

                    }
                }

                foreach(string keywords in Shuffle(await db.memes.GroupBy(x => x.keyword).Select(y => y.Key).ToListAsync()))
                {
                    bool ContainsKey = false;
                    string[] allWords = keywords.ToUpper().Split(' ');

                    int containedCount = 0;

                    foreach (string keyword in allWords)
                    {
                        if(RemoveRandom(e.Message.RawText).ToUpper().Contains(keyword))
                        {
                            containedCount++;
                            if (containedCount == allWords.Length)
                                ContainsKey = true;
                        }
                    }

                    if(ContainsKey)
                    {
                        List<meme> PossibleMemes = await db.memes.Where(x => x.keyword == keywords).ToListAsync();

                        List<meme> PaintBucket = PossibleMemes;

                        foreach (meme me in PaintBucket)
                        {
                            if (me.lastusage.HasValue && me.maxspan.HasValue)
                            {
                                if ((DateTime.Now - me.lastusage.Value).TotalMinutes < me.maxspan.Value)
                                    PossibleMemes.Remove(me);
                            }
                        }

                        if (PossibleMemes.Count > 0)
                        {
                            int index = new Random().Next(0, PossibleMemes.Count);

                            //TODO: Log Usage
                            await e.Channel.SendMessage(PossibleMemes[index].response);
                            return;
                        }


                        return;
                    }
                }
                

                //await e.Channel.SendMessage("Rock and Roll!!!");
            };

            bot.ExecuteAndWait(async () =>
            {
                try
                {
                    await bot.Connect("MjQ5NTIyNDI1Nzk0OTIwNDQ4.CxJa1w.1llDJ6C9UPGixaZQdJhkMV4wKeA", TokenType.Bot);
                    Console.WriteLine("Connection State: " + (bot.State == ConnectionState.Connected).ToString());
                    bot.JoinedServer += Bot_JoinedServer;
                    await Task.Delay(1000);
                    //Console.ReadKey();
                }
                catch (Exception ex)
                {
                    Console.WriteLine("Connection Failed - " + ex.Message);
                    //Console.ReadKey();
                }
                /*
                if (bot.Servers.Any())
                {
                    foreach (Server server in bot.Servers)
                    {
                        Console.WriteLine(server.Id.ToString() + " - " + server.Name);

                        foreach (Channel channel in server.AllChannels)
                        {
                            if (channel.Type == ChannelType.Text)
                            {
                                await channel.SendMessage("Rock Bot Is In the building!");
                                Console.WriteLine("Message Sent on Channel: " + channel.Name);
                                await Task.Delay(100);
                            }
                            else
                            {
                                await channel.SendTTSMessage("Rock Bot Is In the building!");
                                Console.WriteLine("Message Sent on Channel: " + channel.Name);
                                await Task.Delay(100);
                            }
                        }
                    }
                }*/
            });
        }

        private static Random rng = new Random();

        public static IList<T> Shuffle<T>(IList<T> list)
        {
            int n = list.Count;
            while (n > 1)
            {
                n--;
                int k = rng.Next(n + 1);
                T value = list[k];
                list[k] = list[n];
                list[n] = value;
            }

            return list;
        }

        public static string RemoveRandom(string content)
        {
            foreach(string invalid in InvalidCharacters)
            {
                content = content.Replace(invalid, "");
            }

            return content;
        }

        private static readonly string[] InvalidCharacters = new string[]
        {
            ",",
            ".",
            "<",
            ">",
            "?",
            "/",
            "\"",
            "'",
            ";",
            "{",
            "}",
            "[",
            "]",
            "\\",
            "|",
            "!",
            "@",
            "#",
            "$",
            "%",
            "^",
            "&",
            "*",
            "(",
            ")",
            "_",
            "+",
            "=",
            "~",
            "`"
        };

        private void Add(MessageEventArgs e)
        {
            throw new NotImplementedException();
        }

        private void Bot_JoinedServer(object sender, ServerEventArgs e)
        {
            Console.WriteLine("Joined Server - " + e.Server.Name);
        }



    }
}
