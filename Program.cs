﻿using System;
using TwitchLib;
using TwitchLib.Client;
using TwitchLib.Client.Models;
using TwitchLib.Client.Events;
using TwitchLib.Client.Extensions;
using TwitchLib.Api;
using TwitchLib.Api.Models.Helix.Users.GetUsersFollows;
using TwitchLib.Api.Models.v5.Subscriptions;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Configuration;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net.Sockets;
using System.Net;
using System.IO;
using System.Threading;
using System.Text.RegularExpressions;
using MySql.Data.MySqlClient;


namespace EleunameBotConsole
{
    public class Program
    {
        #region Variables
        public static TwitchClient client;
        public static int[] dailybonuses;
        public static string[] highmods;
        public static ConnectionCredentials credentials = new ConnectionCredentials("username", "token");
        public static int subscriptionbonus = 2500;
        static Handlers.CommandHandler handler = new Handlers.CommandHandler();
        public static Random rnd = new Random();
        #endregion
        public static void Main(string[] args)
        {
            client = new TwitchClient();
            client.Initialize(credentials, "channelname");
            client.OnJoinedChannel += onJoinedChannel;
            client.OnMessageReceived += onMessageReceived;
            client.OnUserJoined += onUserJoined;
            client.OnGiftedSubscription += new EventHandler<OnGiftedSubscriptionArgs>(onGiftedSubscription);
            client.OnWhisperReceived += onWhisperReceived;
            client.OnChatCommandReceived += onCommandReceived;
            client.OnNewSubscriber += onNewSubscriber;
            client.OnReSubscriber += new EventHandler<OnReSubscriberArgs>(onReSubscription);
            client.Connect();
            #region Commands registration (?)
            handler.RegisterCommand("gamble", Features.Commands.gambleCommand);
            handler.RegisterCommand("punch", Features.Commands.punchCommand);
            handler.RegisterCommand("hug", Features.Commands.hugCommand);
            handler.RegisterCommand("getpoints", Features.Commands.getpointsCommand);
            handler.RegisterCommand("discord", Features.Commands.discordCommand);
            handler.RegisterCommand("quote", Features.Commands.quoteCommand);
            handler.RegisterCommand("help", Features.Commands.helpCommand);
            handler.RegisterCommand("ping", Features.Commands.pingCommand);
            handler.RegisterCommand("songrequest", Features.Commands.songrequestCommand);
            handler.RegisterCommand("givepoints", Features.Commands.givepointsCommand);
            handler.RegisterCommand("dailybonus", Features.Commands.dailybonusCommand);
            handler.RegisterCommand("addfriend", Features.Commands.addfriendCommand);
            handler.RegisterCommand("duoq", Features.Commands.duoqCommand);
            handler.RegisterCommand("championreq", Features.Commands.champreqCommand);
            handler.RegisterCommand("debug", Features.Commands.debugTriviaCommand);
            handler.RegisterCommand("addhighmod", Features.Commands.addhighmodCommand);
            handler.RegisterCommand("subtest", Features.Commands.subtestCommand);
            handler.RegisterCommand("duel", Features.Commands.duelCommand);
            handler.RegisterCommand("triviastop", Features.Commands.triviastopCommand);
            handler.RegisterCommand("triviastart", Features.Commands.triviastartCommand);
            #endregion   
            dailybonuses = GetDailyBonusValues("Resources/dailybonuses.txt");
            Console.WriteLine("Successfully loaded dailybonus values.");
            highmods = GetHighMods("Resources/highmods.txt");
            Console.WriteLine($"Successfully loaded {highmods.Length} high mods.");
            Console.WriteLine("Successfully connected to chat.");
            Console.ReadLine();

        }
        public static int[] GetDailyBonusValues(string filename)
        {
            string fileContents = File.ReadAllText(filename); //Read all file to string
            string[] splitFileContents = fileContents.Split(','); //Split string by comma values
            return splitFileContents.Select(s => int.Parse(s)).ToArray(); //convert each split string to an integer, and return the whole thing as an array
        }

        public static string[] GetHighMods(string filename)
        {
            string fileContents = File.ReadAllText(filename); //Read all file to string
            string[] splitFileContents = fileContents.Split(','); //Split string by comma values
            return splitFileContents; //returns file content as an array.
        }
        
        public static void CommandLogger(string text)
        {
            File.AppendAllTextAsync("Resources/commandlog.txt", text);
        }
        
        public static void ChatLogger(string text)
        {
            File.AppendAllTextAsync("Resources/chatlog.txt", text);
        }
        
        #region TwitchLib Functions

        private static void onJoinedChannel(object sender, OnJoinedChannelArgs e)
        {
            client.SendMessage(e.Channel, "Successfully connected, hello!");
        }
        private static void onMessageReceived(object sender, OnMessageReceivedArgs e)
        {
            Console.WriteLine($"{e.ChatMessage.Username}: {e.ChatMessage.Message}");
            ChatLogger($"{DateTime.UtcNow} - {e.ChatMessage.Username}: {e.ChatMessage.Message} {Environment.NewLine}");
            if (e.ChatMessage.Message.Equals(Trivia.CurrentAnswer, StringComparison.CurrentCultureIgnoreCase))
            {
                if (e.ChatMessage.IsSubscriber)
                {
                    Trivia.answerPoints = 500;
                }
                else
                {
                    Trivia.answerPoints = 250;
                }
                Trivia.AnswerQuestion(e.ChatMessage.Username.ToLower());
                }
        }


        private static void onWhisperReceived(object sender, OnWhisperReceivedArgs e)
        {
        }

        private static void onNewSubscriber(object sender, OnNewSubscriberArgs e)
        {
            if (e.Subscriber.SubscriptionPlanName.Equals("prime", StringComparison.CurrentCultureIgnoreCase))
            {
                client.SendMessage(client.JoinedChannels[0], $"{e.Subscriber.DisplayName} has just subscribed using Twitch Prime! Thank you and welcome to the Shibas family! You have received 2500 points for subscribing.");
                Handlers.DatabaseHandler.ExecuteNonQuery($"INSERT INTO Users (username,points) values ('{e.Subscriber.DisplayName.ToLower()}', {subscriptionbonus}) ON DUPLICATE KEY UPDATE points = points + {subscriptionbonus}");
            }
            else
            {
                client.SendMessage(client.JoinedChannels[0], $"{e.Subscriber.DisplayName} has just subscribed! Thank you and welcome to the Shibas family! You have received 2500 points for subscribing.");
                Handlers.DatabaseHandler.ExecuteNonQuery($"INSERT INTO Users (username,points) values ('{e.Subscriber.DisplayName.ToLower()}', {subscriptionbonus}) ON DUPLICATE KEY UPDATE points = points + {subscriptionbonus}");
            }
        }

        private static void onReSubscription(object sender, OnReSubscriberArgs e)
        {
            int resubpoints = e.ReSubscriber.Months * subscriptionbonus;
            client.SendMessage(client.JoinedChannels[0], $"{e.ReSubscriber.Login} has resubscribed for {e.ReSubscriber.Months} months. You have received {resubpoints} points for re-subscribing. Included message: {e.ReSubscriber.ResubMessage}");
            Handlers.DatabaseHandler.ExecuteNonQuery($"INSERT INTO Users (username,points) values ('{e.ReSubscriber.Login}', {resubpoints}) ON DUPLICATE KEY UPDATE points = points + {resubpoints}");
        }
        private static void onGiftedSubscription(object sender, OnGiftedSubscriptionArgs e)
        {
            client.SendMessage(client.JoinedChannels[0], $"Wow! {e.GiftedSubscription.Login} has gifted a subscription to {e.GiftedSubscription.MsgParamRecipientUserName}! Thank you so much for your generosity! You have been awarded 5000 points for being so kind :)");
            Handlers.DatabaseHandler.ExecuteNonQuery($"INSERT INTO Users (username,points) values ('{e.GiftedSubscription.Login}', {5000}) ON DUPLICATE KEY UPDATE points = points + {5000}");
        }
        private static void onUserJoined(object sender, OnUserJoinedArgs e)
        {

        }

        #endregion

        private static void onCommandReceived(object sender, OnChatCommandReceivedArgs e)
        {
            handler.OnCommandReceived(e);
        }
    }
}
