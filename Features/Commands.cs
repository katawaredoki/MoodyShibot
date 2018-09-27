using System;
using System.Net.Sockets;
using System.Net;
using System.Collections.Generic;
using System.Text;
using TwitchLib;
using TwitchLib.Client;
using TwitchLib.Client.Events;
using MySql.Data.MySqlClient;
using System.IO;
using System.Linq;
using TwitchLib.Client.Extensions;

namespace EleunameBotConsole.Features
{
    public class Commands
    {
        #region Interactive commands
        public static void punchCommand(OnChatCommandReceivedArgs e)
        {
            Console.WriteLine("SYSTEM: punch command was called!");
            Program.CommandLogger($"{e.Command.ChatMessage.Username} has used !punch at {DateTime.UtcNow} {Environment.NewLine}");
            Console.WriteLine("Successfully wrote to the log.");
            if (e.Command.ArgumentsAsList.Count == 0)
            {
                Program.client.SendMessage(Program.client.JoinedChannels[0], "You gotta target someone first, dummy!");
            }
            else

            {
                Program.client.SendMessage(Program.client.JoinedChannels[0], $"Woah, {e.Command.ChatMessage.Username} has punched {e.Command.ArgumentsAsList[0]} in the face");
            }
        }
        public static void hugCommand(OnChatCommandReceivedArgs e)
        {
            Console.WriteLine("SYSTEM: hug command was called!");
            Program.CommandLogger($"{e.Command.ChatMessage.Username} has used !hug at {DateTime.UtcNow} {Environment.NewLine}");
            Console.WriteLine("Successfully wrote to the log.");
            if (e.Command.ArgumentsAsList.Count == 0)
            {
                Program.client.SendMessage(Program.client.JoinedChannels[0], "You gotta target someone first, dummy!");
            }
            else

            {
                Program.client.SendMessage(Program.client.JoinedChannels[0], $"Awwww, {e.Command.ChatMessage.Username} just gave {e.Command.ArgumentsAsList[0]} a really warm hug!");
            }
        }
        #endregion
        #region Points commands
        public static void dailybonusCommand(OnChatCommandReceivedArgs e)
        {
            int r = Program.rnd.Next(Program.dailybonuses.Length);
            int dailybonus = Program.dailybonuses[r];
            int record = 0;
            string sql = $"SELECT timestamp FROM Users WHERE username = '{e.Command.ChatMessage.Username.ToLower()}';";
            record = int.Parse(Handlers.DatabaseHandler.ScalarCommand(sql));
            if (DateTimeOffset.UtcNow.ToUnixTimeSeconds() - record >= 86400)
            {
                if (dailybonus < 0)
                {
                    Program.client.SendMessage(Program.client.JoinedChannels[0], $"Oof. Lady Luck decided that today is not a lucky day for you! You lost {dailybonus} snacks! Better luck tomorrow :c");
                }
                else
                {
                    Program.client.SendMessage(Program.client.JoinedChannels[0], $"{e.Command.ChatMessage.Username.ToLower()} - Good news! The Lady Luck decided that your bonus for today is {dailybonus} snacks!");

                }
                Handlers.DatabaseHandler.ExecuteNonQuery($"UPDATE Users SET timestamp = {DateTimeOffset.UtcNow.ToUnixTimeSeconds()} WHERE username='{e.Command.ChatMessage.Username.ToLower()}'; UPDATE Users SET points = points + {dailybonus} WHERE username='{e.Command.ChatMessage.Username.ToLower()}';");
            }
        }
        public static void givepointsCommand(OnChatCommandReceivedArgs e)
        {
            bool check = Array.Exists(Program.highmods, element => element == e.Command.ChatMessage.Username);
            if (e.Command.ChatMessage.IsBroadcaster && e.Command.ArgumentsAsList.Count > 1 || check == true && e.Command.ArgumentsAsList.Count > 1)
            {
                Console.WriteLine("SYSTEM: givepoints command was called!");
                Program.CommandLogger($"{e.Command.ChatMessage.Username} has used !givepoints at {DateTime.UtcNow} {Environment.NewLine}");
                Console.WriteLine("Successfully wrote to the log.");
                int given;
                if (int.TryParse(e.Command.ArgumentsAsList[1], out given))
                {
                    if (given < 0)
                    {
                        Console.WriteLine("COMMANDS: Points have been taken, is it okay?");
                        Program.client.SendMessage(Program.client.JoinedChannels[0], $"{e.Command.ArgumentsAsList[1]} snacks have been taken from {e.Command.ArgumentsAsList[0]}");
                        Handlers.DatabaseHandler.ExecuteNonQuery($"INSERT INTO Users (username,points) values ('{e.Command.ArgumentsAsList[0].ToLower()}', {e.Command.ArgumentsAsList[1]}) ON DUPLICATE KEY UPDATE points = points + {e.Command.ArgumentsAsList[1]}");
                    }
                    else
                    {
                        Console.WriteLine("COMMANDS: Points have been given, is it okay?");
                        Program.client.SendMessage(Program.client.JoinedChannels[0], $"{e.Command.ArgumentsAsList[1]} snacks have been awarded to {e.Command.ArgumentsAsList[0]}");
                        Handlers.DatabaseHandler.ExecuteNonQuery($"INSERT INTO Users (username,points) values ('{e.Command.ArgumentsAsList[0].ToLower()}', {e.Command.ArgumentsAsList[1]}) ON DUPLICATE KEY UPDATE points = points + {e.Command.ArgumentsAsList[1]}");
                    }
                }
                else
                {
                    Console.WriteLine("ERROR: You must provide a number as parameter, noob.");
                    Program.CommandLogger($"{e.Command.ChatMessage.Username} has used !givepoints on {e.Command.ArgumentsAsList[0]} at {DateTime.UtcNow} {Environment.NewLine}");

                }
            }
        }
        public static void getpointsCommand(OnChatCommandReceivedArgs e)
        {
            if (e.Command.ArgumentsAsList.Count != 0)
            {
                if (!e.Command.ArgumentsAsList[0].Contains(";"))
                {
                    int points = 0;
                    Handlers.DatabaseHandler.ExecuteNonQuery($"INSERT IGNORE INTO Users (username,points) values ('{e.Command.ArgumentsAsList[0].ToLower()}', 0)");
                    points = int.Parse(Handlers.DatabaseHandler.ScalarCommand($"SELECT points FROM Users WHERE username = '{MySqlHelper.EscapeString(e.Command.ArgumentsAsList[0].ToLower())}';"));
                    Program.client.SendMessage(Program.client.JoinedChannels[0], $"{e.Command.ArgumentsAsList[0].ToLower()} has {points} snacks.");
                }
            }
            else
            {
                int points = 0;
                Handlers.DatabaseHandler.ExecuteNonQuery($"INSERT IGNORE INTO Users (username,points) values ('{e.Command.ChatMessage.Username.ToLower()}', 0)");
                points = int.Parse(Handlers.DatabaseHandler.ScalarCommand($"SELECT points FROM Users WHERE username = '{MySqlHelper.EscapeString(e.Command.ChatMessage.Username.ToLower())}';"));
                Program.client.SendMessage(Program.client.JoinedChannels[0], $"{e.Command.ChatMessage.Username.ToLower()} has " + points + " snacks.");
            }
            Console.WriteLine("SYSTEM: getpoints command was called!");
            Program.CommandLogger($"{e.Command.ChatMessage.Username} has used !getpoints at {DateTime.UtcNow} {Environment.NewLine}");
            Console.WriteLine("Successfully wrote to the log.");
        }
        #endregion
        #region Text commands
        public static void discordCommand(OnChatCommandReceivedArgs e)
        {
            Console.WriteLine("SYSTEM: discord command was called!");
            Program.CommandLogger($"{e.Command.ChatMessage.Username} has used !discord at {DateTime.UtcNow} {Environment.NewLine}");
            Console.WriteLine("Successfully wrote to the log.");
            Program.client.SendMessage(Program.client.JoinedChannels[0], "For security reasons I blocked the Discord invite I was using :) Please hit me up at \"moody shiba#4276\" or on Twitch while I am streaming to get an invite! :P ");
        }
        public static void quoteCommand(OnChatCommandReceivedArgs e)
        {
            Console.WriteLine("SYSTEM: quote command was called!");
            Program.CommandLogger($"{e.Command.ChatMessage.Username} has used !quote at {DateTime.UtcNow} {Environment.NewLine}");
            Console.WriteLine("Successfully wrote to the log.");
            Program.client.SendMessage(Program.client.JoinedChannels[0], "\"Oh my god, this is so free\" - Emanuele before dying 1v4");
        }
        public static void helpCommand(OnChatCommandReceivedArgs e)
        {
            Console.WriteLine("SYSTEM: help command was called!");
            Program.CommandLogger($"{e.Command.ChatMessage.Username} has used !help at {DateTime.UtcNow} {Environment.NewLine}");
            Console.WriteLine("Successfully wrote to the log.");
            Program.client.SendMessage(Program.client.JoinedChannels[0], "You can find this bot's command list at https://uploads.eleuna.me/commands.txt");
        }
        public static void pingCommand(OnChatCommandReceivedArgs e)
        {
            if (e.Command.ChatMessage.IsModerator)
            {
                Console.WriteLine("SYSTEM: ping command was called!");
                Program.CommandLogger($"{e.Command.ChatMessage.Username} has used !ping at {DateTime.UtcNow} {Environment.NewLine}");
                Console.WriteLine("Successfully wrote to the log.");
                Program.client.SendMessage(Program.client.JoinedChannels[0], "Pong!");
            }
        }
        #endregion
        public static void songrequestCommand(OnChatCommandReceivedArgs e)
        {
            Console.WriteLine("SYSTEM: A song has been requested.");
            Program.CommandLogger($"{e.Command.ChatMessage.Username} has used !songrequest at {DateTime.UtcNow} {Environment.NewLine}");
            Console.WriteLine("Successfully wrote to the log.");
            if (e.Command.ArgumentsAsList.Count == 1)
            {
                if(!e.Command.IsSubscriber)
                {
                int points = 0;
                points = int.Parse(Handlers.DatabaseHandler.ScalarCommand($"SELECT points FROM Users WHERE username = '{e.Command.ChatMessage.Username.ToLower()}';"));
                if (points < 10000)
                {
                    Program.client.SendMessage(Program.client.JoinedChannels[0], $"{e.Command.ChatMessage.Username} - Sorry but you need 10000 snacks and you currently have {points} snacks. :(");
                }
                }
                else
                {
                    if (e.Command.ArgumentsAsList[0].StartsWith("https://www.youtube.com"))
                    {
                        Program.client.SendMessage(Program.client.JoinedChannels[0], $"{e.Command.ChatMessage.Username} has requested {YoutubeParse.GetTitle(e.Command.ArgumentsAsList[0])}");
                        File.AppendAllText("/var/www/apis.eleuna.me/bot/songrequests.txt", $"{DateTime.UtcNow} -   {e.Command.ArgumentsAsList[0]} has been requested by {e.Command.ChatMessage.Username}" + Environment.NewLine);
                    }
                    else
                    {
                        Program.client.SendMessage(Program.client.JoinedChannels[0], $"{e.Command.ChatMessage.Username} has requested {e.Command.ArgumentsAsList[0]}");
                        File.AppendAllText("/var/www/apis.eleuna.me/bot/songrequests.txt", $"{e.Command.ArgumentsAsList[0]} has been requested by {e.Command.ChatMessage.Username}" + Environment.NewLine);
                    }
                    var request = (HttpWebRequest)WebRequest.Create("https://www.streamlabs.com/api/v1.0/alerts");
                    var postData = "access_token=yfEYh5DJo9mlmVmh4tcDVn5eJY66pNKWlrQDC7ZZ";
                    postData += "&type=follow";
                    postData += $"&message=*{e.Command.ChatMessage.Username}* has just requested a song!";
                    postData += "&image_href=https://apis.eleuna.me/bot/musicreq.gif";
                    postData += "&sound_href=https://apis.eleuna.me/bot/boop.wav";
                    postData += "&duration=3000";
                    postData += "&special_text_color='#77dd77'";

                    var data = Encoding.ASCII.GetBytes(postData);

                    request.Method = "POST";
                    request.ContentType = "application/x-www-form-urlencoded";
                    request.ContentLength = data.Length;

                    using (var stream = request.GetRequestStream())
                    {
                        stream.Write(data, 0, data.Length);
                    }

                    var response = (HttpWebResponse)request.GetResponse();

                    var responseString = new StreamReader(response.GetResponseStream()).ReadToEnd();
                    Handlers.DatabaseHandler.ExecuteNonQuery($"UPDATE Users SET points = points - 10000 WHERE username='{e.Command.ChatMessage.Username.ToLower()}';");
                }
            }
        }
        public static void addfriendCommand(OnChatCommandReceivedArgs e)
        {
            Console.WriteLine("SYSTEM: Someone asked to be added on League.");
            Program.CommandLogger($"{e.Command.ChatMessage.Username} has used !addfriend at {DateTime.UtcNow} {Environment.NewLine}");
            Console.WriteLine("Successfully wrote to the log.");
            if (e.Command.ArgumentsAsList.Count != 0)
            {
                int points = 0;
                points = int.Parse(Handlers.DatabaseHandler.ScalarCommand($"SELECT points FROM Users WHERE username = '{e.Command.ChatMessage.Username.ToLower()}';"));
                if (points < 15000)
                {
                    Program.client.SendMessage(Program.client.JoinedChannels[0], $"{e.Command.ChatMessage.Username} - Sorry but you need 15000 snacks and you currently have {points} snacks. :(");
                }
                else
                {
                    Program.client.SendMessage(Program.client.JoinedChannels[0], $"{e.Command.ChatMessage.Username} has asked to be added on League! IGN: {e.Command.ArgumentsAsString}");
                    File.AppendAllText("/var/www/apis.eleuna.me/bot/leaguerequests.txt", $"{DateTime.UtcNow} -   {e.Command.ArgumentsAsString} has asked to be added on League. (Twitch username:{e.Command.ChatMessage.Username})" + Environment.NewLine);
                    Handlers.DatabaseHandler.ExecuteNonQuery($"UPDATE Users SET points = points - 15000 WHERE username='{e.Command.ChatMessage.Username.ToLower()}';");
                }
            }
        }
        public static void duoqCommand(OnChatCommandReceivedArgs e)
        {
            Console.WriteLine("SYSTEM: Someone asked to DuoQ on League.");
            Program.CommandLogger($"{e.Command.ChatMessage.Username} has used !duoq at {DateTime.UtcNow} {Environment.NewLine}");
            Console.WriteLine("Successfully wrote to the log.");
            if (e.Command.ArgumentsAsList.Count != 0)
            {
                int points = 0;
                points = int.Parse(Handlers.DatabaseHandler.ScalarCommand($"SELECT points FROM Users WHERE username = '{e.Command.ChatMessage.Username.ToLower()}';"));
                if (points < 50000)
                {
                    Program.client.SendMessage(Program.client.JoinedChannels[0], $"{e.Command.ChatMessage.Username} - Sorry but you need 50000 snacks and you currently have {points} snacks. :(");
                }
                else
                {

                    Program.client.SendMessage(Program.client.JoinedChannels[0], $"{e.Command.ChatMessage.Username} has asked to DuoQ on League! IGN: {e.Command.ArgumentsAsString}");
                    File.AppendAllText("/var/www/apis.eleuna.me/bot/leaguerequests.txt", $"{DateTime.UtcNow} -   {e.Command.ArgumentsAsString} has asked to Duo on League. (Twitch username:{e.Command.ChatMessage.Username})" + Environment.NewLine);
                    Handlers.DatabaseHandler.ExecuteNonQuery($"UPDATE Users SET points = points - 50000 WHERE username='{e.Command.ChatMessage.Username.ToLower()}';");
                }
            }
        }
        public static void champreqCommand(OnChatCommandReceivedArgs e)
        {
            Console.WriteLine("SYSTEM: Someone requested a Champion!");
            Program.CommandLogger($"{e.Command.ChatMessage.Username} has used !champion at {DateTime.UtcNow} {Environment.NewLine}");
            Console.WriteLine("Successfully wrote to the log.");
            if (e.Command.ArgumentsAsList.Count != 0)
            {
                int points = 0;
                points = int.Parse(Handlers.DatabaseHandler.ScalarCommand($"SELECT points FROM Users WHERE username = '{e.Command.ChatMessage.Username.ToLower()}';"));
                if (points < 35000)
                {
                    Program.client.SendMessage(Program.client.JoinedChannels[0], $"{e.Command.ChatMessage.Username} - Sorry but you need 35000 snacks and you currently have {points} snacks. :(");
                }
                else
                {

                    Program.client.SendMessage(Program.client.JoinedChannels[0], $"{e.Command.ChatMessage.Username} has requested a champion! They requested: {e.Command.ArgumentsAsString}");
                    File.AppendAllText("/var/www/apis.eleuna.me/bot/leaguerequests.txt", $"{DateTime.UtcNow} - {e.Command.ChatMessage.Username} has requested a champion! They requested: {e.Command.ArgumentsAsString})" + Environment.NewLine);
                    Handlers.DatabaseHandler.ExecuteNonQuery($"UPDATE Users SET points = points - 35000 WHERE username='{e.Command.ChatMessage.Username.ToLower()}';");
                }
            }
        }
        public static void addhighmodCommand(OnChatCommandReceivedArgs e)
        {
            if (e.Command.ChatMessage.IsBroadcaster)
            {
                Console.WriteLine($"SYSTEM: {e.Command.ArgumentsAsList[0]} is being added as new high mod!");
                File.AppendAllText("highmods.txt", $",{e.Command.ArgumentsAsList[0]}");
                Program.CommandLogger($"{e.Command.ChatMessage.Username} has used !addhighmod at {DateTime.UtcNow} {Environment.NewLine}");
                Console.WriteLine("Successfully wrote to the log.");
                Console.WriteLine("SYSTEM: Refreshing high mods list...");
                Program.highmods = Program.GetHighMods("highmods.txt");
                Console.WriteLine($"SYSTEM: High mods list has successfully been refreshed and now has {Program.highmods.Length} members.");
                Program.client.SendMessage(Program.client.JoinedChannels[0], $"{e.Command.ArgumentsAsList[0]}has successfully been added as a high mod!");
            }
        }
        public static void subtestCommand(OnChatCommandReceivedArgs e)
        {
            if (e.Command.ChatMessage.IsBroadcaster)
            {
                Console.WriteLine("SYSTEM: subtest command was called!");
                Program.CommandLogger($"{e.Command.ChatMessage.Username} has used !subtest at {DateTime.UtcNow} {Environment.NewLine}");
                Console.WriteLine("Successfully wrote to the log.");
                Program.client.InvokeNewSubscriber(new System.Collections.Generic.List<System.Collections.Generic.KeyValuePair<string, string>>(), "", System.Drawing.Color.AliceBlue, e.Command.ArgumentsAsList[0], "", "", "", "", "", "", TwitchLib.Client.Enums.SubscriptionPlan.NotSet, "", "", "", false, false, false, false, "", TwitchLib.Client.Enums.UserType.Admin, "", "eleuname");
            }

        }
        public static void debugTriviaCommand(OnChatCommandReceivedArgs e)
        {
            if (e.Command.ChatMessage.IsBroadcaster)
            {
                Program.client.SendMessage(Program.client.JoinedChannels[0], $"{Trivia.Questions.Count()} questions remaining on the main array.");
                Program.client.SendMessage(Program.client.JoinedChannels[0], $"{Trivia.UsedQuestions.Count()} questions remaining on the used array.");
            }
        }

        public static void triviastartCommand(OnChatCommandReceivedArgs e)
        {
            bool check = Array.Exists(Program.highmods, element => element == e.Command.ChatMessage.Username);
            if (e.Command.ChatMessage.IsBroadcaster || check == true)
            {
                Trivia.Load();
                Trivia.Start();
                Console.WriteLine("Loaded " + Trivia.Questions.Count() + " questions.");
                Program.client.SendMessage(Program.client.JoinedChannels[0], "A new trivia session has been started!");
            }
        }
        public static void triviastopCommand(OnChatCommandReceivedArgs e)
        {
            bool check = Array.Exists(Program.highmods, element => element == e.Command.ChatMessage.Username);
            if (e.Command.ChatMessage.IsBroadcaster || check == true)
            {
                Trivia.Running = false;
                Trivia.Timer.Stop();
                Trivia.UsedQuestions.Clear();
                Trivia.Questions.Clear();
                Program.client.SendMessage(Program.client.JoinedChannels[0], "Trivia has ended for today! Thanks for playing and see you next stream!");
                Console.WriteLine("Trivia session has been successfully closed");
            }
        }
        public static void duelCommand(OnChatCommandReceivedArgs e)
        {
            if (e.Command.ArgumentsAsList.Count != 0)
            {
                int rand = Program.rnd.Next(1, 100);
                if(rand < 51)
                {
                    Program.client.SendMessage(Program.client.JoinedChannels[0], $"{e.Command.ChatMessage.Username.ToLower()} stole Gangplank's pistol and shoot {e.Command.ArgumentsAsList[0]} in the head winning the duel! ARGHHH");
                }
                else
                {
                    Program.client.SendMessage(Program.client.JoinedChannels[0], $"During the duel, Fiora, a cute girl who was walking by the road, borrowed {e.Command.ArgumentsAsList[0]} her sword and he used it to win the fight! Poor {e.Command.ChatMessage.Username.ToLower()}");
                }
            }  
        }
        public static void gambleCommand(OnChatCommandReceivedArgs e)
        {
            int rand = Program.rnd.Next(1, 100);
            int betsnacks;
            if (int.TryParse(e.Command.ArgumentsAsList[0], out betsnacks) || e.Command.ArgumentsAsString == "all")
            {
                if (e.Command.ArgumentsAsString == "all")
                {
                    betsnacks = int.Parse(Handlers.DatabaseHandler.ScalarCommand($"SELECT points FROM Users WHERE username = '{MySqlHelper.EscapeString(e.Command.ChatMessage.Username.ToLower())}';"));
                    if (rand < 41)
                    {
                        Program.client.SendMessage(Program.client.JoinedChannels[0], $"Congratulations, {e.Command.ChatMessage.Username.ToLower()} you won {betsnacks * 2} snacks!");
                        Handlers.DatabaseHandler.ExecuteNonQuery($"UPDATE Users SET points = points + {betsnacks * 2} WHERE username='{e.Command.ChatMessage.Username.ToLower()}';");

                    }
                    else
                    {
                        Program.client.SendMessage(Program.client.JoinedChannels[0], $"Sorry, {e.Command.ChatMessage.Username.ToLower()} you lost all the snacks you bet! :c");
                        Handlers.DatabaseHandler.ExecuteNonQuery($"UPDATE Users SET points = points - {betsnacks} WHERE username='{e.Command.ChatMessage.Username.ToLower()}';");
                    }
                }
                else
                {
                    if (rand < 41)
                    {
                        if (int.Parse(Handlers.DatabaseHandler.ScalarCommand($"SELECT points FROM Users WHERE username = '{MySqlHelper.EscapeString(e.Command.ChatMessage.Username.ToLower())}';")) > betsnacks)
                        {
                            Program.client.SendMessage(Program.client.JoinedChannels[0], $"Congratulations, {e.Command.ChatMessage.Username.ToLower()} you won {betsnacks * 2} snacks!");
                            Handlers.DatabaseHandler.ExecuteNonQuery($"UPDATE Users SET points = points + {betsnacks * 2} WHERE username='{e.Command.ChatMessage.Username.ToLower()}';");
                        }
                    }
                    else
                    {
                        if (int.Parse(Handlers.DatabaseHandler.ScalarCommand($"SELECT points FROM Users WHERE username = '{MySqlHelper.EscapeString(e.Command.ChatMessage.Username.ToLower())}';")) > betsnacks)
                        {
                            Program.client.SendMessage(Program.client.JoinedChannels[0], $"Sorry, {e.Command.ChatMessage.Username.ToLower()} you lost all the snacks you bet! :c");
                            Handlers.DatabaseHandler.ExecuteNonQuery($"UPDATE Users SET points = points - {betsnacks} WHERE username='{e.Command.ChatMessage.Username.ToLower()}';");
                        }
                    }
                }
            }
        }
    }
}