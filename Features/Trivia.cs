using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Threading;
using System.Timers;
using System.Data;
using MySql.Data.MySqlClient;

namespace EleunameBotConsole
{
    public abstract class Trivia //this class is messy because it's taken and re-adapted from one of my old projects, but it works fine so I'm okay with it.
    {
        #region Variables
        public static bool WaitingOnAnswer = false;
        public static string CurrentAnswer = "";
        public static int answerPoints = 250;
        public static System.Timers.Timer Timer;
        public static double HintInterval = 3;
        public static bool Running = false;
        public static string Winner = "";
        public static string TotalQuestions = "";
        public static List<TrivQuestion> Questions = new List<TrivQuestion>();
        public static List<TrivQuestion> UsedQuestions = new List<TrivQuestion>();
        public static string CategoryName = "League of Legends";
        public static int QuestionInterval = 15000;
        public static Random rnd = new Random();
        public static MySqlConnection dbConnection;
        #endregion

        public static void AnswerQuestion(string winner)
        {
            if (!WaitingOnAnswer)
                return;
            else
            {
                WaitingOnAnswer = false;
                Winner = winner;
            }
        }
        public static void Load()
        {

            Timer = new System.Timers.Timer();
            Timer.Interval = QuestionInterval;
            Timer.Elapsed += new ElapsedEventHandler(Timer_Elapsed);
            dbConnection = new MySqlConnection(Handlers.DatabaseHandler.myConnectionString); //I had no other way than to do this in this class instead of handling it in the Handlers.DatabaseHandler one.
            dbConnection.Open();
            string query = "SELECT * FROM Questions ORDER BY UID";
            MySqlCommand cmd = new MySqlCommand(query, dbConnection);

            MySqlDataReader dr = cmd.ExecuteReader();
            while (dr.Read())
            {
                TrivQuestion item = new TrivQuestion();
                item.ID = dr.GetInt32(0);
                item.Question = dr.GetString(1);
                item.Answer = dr.GetString(2);
                Questions.Add(item);
            }
            dbConnection.Close();
        }

        private static void Timer_Elapsed(object sender, ElapsedEventArgs e)
        {
            Timer.Interval = QuestionInterval;
            Timer.Stop();
            int rand = rnd.Next(0, Math.Max(0, Questions.Count - 1));
            TrivQuestion currentQuestion = Questions[rand];
            CurrentAnswer = currentQuestion.Answer;
            Program.client.SendMessage(Program.client.JoinedChannels[0], currentQuestion.Question);
            WaitingOnAnswer = true;
            for (int i = 0; i < 3; i++)
            {
                for (int x = 0; x < HintInterval; x++)
                {
                    if (!WaitingOnAnswer)
                        break;
                    Thread.Sleep(1000);
                }
                if (!WaitingOnAnswer)
                    break;
                switch (i)
                {
                    case 0: break;
                    case 1: break;
                    case 2: break;
                    default: break;
                }
            }
            if (!WaitingOnAnswer)
            {
                Handlers.DatabaseHandler.ExecuteNonQuery($"INSERT INTO Users (username,points) VALUES ('{Winner}', {answerPoints}) ON DUPLICATE KEY UPDATE points = points + {answerPoints}");
                Program.client.SendMessage(Program.client.JoinedChannels[0], $"{Winner} answered first with the correct answer and has earned {answerPoints} points!");
            }
            else
            {
                Program.client.SendMessage(Program.client.JoinedChannels[0], $"Time has ended. The correct answer was {CurrentAnswer}");
            }
            TrivQuestion questionToMove = currentQuestion;
            Questions.Remove(currentQuestion);
            UsedQuestions.Add(questionToMove);
            if(Questions.Count < 2)
            {
                Questions = UsedQuestions;
                UsedQuestions.Clear();
            }
            Timer.Start();
        }

        public static void Start()
        {
            Running = true;
            Timer.Start();
        }

        public class TrivQuestion
        {
            public int ID;
            public string Question;
            public string Answer;
        }
    }
}
