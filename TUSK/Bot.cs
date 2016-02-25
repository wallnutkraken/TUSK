using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Threading;
using Markov;
using TelegramBotNet;
using TelegramBotNet.DTOs;

namespace TUSK
{
    class Bot
    {
        private TelegramBotApi Telegram;
        private List<long> ChatIds;
        private MarkovChain<string> Chain;
        public Bot(string apiKey)
        {
            Telegram = new TelegramBotApi(apiKey);
        }

        private void Format(ref string str)
        {
            str = str.Replace("\"", "");
            str = str.Replace("?", " ?");
        }

        private char[] splitterChars = { ' ' };
        private char[] newlineArray = { '\n' };
        private void Feed(string txt)
        {
            string[] lines = txt.Split(newlineArray, StringSplitOptions.RemoveEmptyEntries);
            foreach (string line in lines)
            {
                string lin = line;
                Format(ref lin);
                string[] words = lin.Split(splitterChars, StringSplitOptions.RemoveEmptyEntries);
                List<string> goodWords = new List<string>();
                foreach (string word in words)
                {
                    if (CheckForLink(word) == false)
                    {
                        goodWords.Add(word);
                    }
                }
                Chain.Add(goodWords);
            }
        }

        private bool CheckForLink(string word)
        {
            Uri uriResult;
            bool result = Uri.TryCreate(word, UriKind.Absolute, out uriResult)
                && (uriResult.Scheme == Uri.UriSchemeHttp || uriResult.Scheme == Uri.UriSchemeHttps);
            return result;
        }

        /// <summary>
        /// WHAT FUCKING EEEEEEVER DUUUUUUDE
        /// </summary>
        public void Init()
        {
            ChatIds = DatabaseAccess.GetActiveChats();
            Chain = new MarkovChain<string>(1);
            List<string> messages = DatabaseAccess.GetAllMessages();
            if (RunArgs.Dump == true)
            {
                System.IO.File.WriteAllLines("dbDump.log", messages);
            }
            foreach (string str in messages)
            {
                Feed(str);
            }
            if (RunArgs.Post == true)
            {
                SendOutMessages();
                Console.WriteLine("Done. Exiting.");
                Environment.Exit(0);
            }
        }

        private string Generate()
        {
            List<string> chainWords = Chain.Chain().ToList();
            string end = "";
            foreach (string word in chainWords)
            {
                end += word + " ";
            }

            return end;
        }

        /// <summary>
        /// Posts a message to a chat
        /// </summary>
        internal void PostMessage(string message, long chatId)
        {
            try
            {
                Telegram.SendMessage(chatId.ToString(), message);
            }
            catch (Exception)
            {
                RemoveChat(chatId);
            }
        }

        public void SendOutMessages()
        {
            string chain = Generate();
            foreach (long id in ChatIds)
            {
                PostMessage(chain, id);
            }
        }

        private void RemoveChat(long chatid)
        {
            ChatIds.Remove(chatid);
            DatabaseAccess.DeactivateChat(chatid);
        }
        public void UnsubChat(long chatid)
        {
            if (ChatIds.Contains(chatid) == false)
            {
                PostMessage("WhatEVER dude you weren't in my life ANYWAY fucking cuck", chatid);
                return;
            }
            RemoveChat(chatid);
            PostMessage(Responses.UnSub(), chatid);
        }

        public void SubChat(long chatid)
        {
            if (ChatIds.Contains(chatid))
            {
                PostMessage("Yeah naw m8 you're already subscribed.", chatid);
                return;
            }
            ChatIds.Add(chatid);
            DatabaseAccess.AddChat(chatid);
            PostMessage(Responses.Sub(), chatid);
        }
        private Update[] GetUpdates()
        {
            return Telegram.GetUpdates(Properties.Settings.Default.LastReadMessage.ToString()).ToArray();
        }

        private void Update()
        {
            Update[] updates = GetUpdates();
            foreach (Update update in updates)
            {
                if (update.Message.Text != null)
                {
                    if (update.Message.Text.StartsWith("/tellmestuff"))
                    {
                        SubChat(update.Message.Chat.Id);
                    }
                    else if (update.Message.Text.StartsWith("/endyourlife"))
                    {
                        UnsubChat(update.Message.Chat.Id);
                    }
                    else
                    {
                        Feed(update.Message.Text);
                        string[] lines = update.Message.Text.Split(newlineArray, StringSplitOptions.RemoveEmptyEntries);
                        foreach (string line in lines)
                        {
                            DatabaseAccess.AddMessage(update.Message.Chat.Id, line);
                        }
                    }
                }
                Properties.Settings.Default.LastReadMessage = update.UpdateId + 1;
                Properties.Settings.Default.Save();
            }
        }

        public void Run()
        {
            ConsoleHelper.WriteLineIf(RunArgs.Verbose, "done.");
            while (true)
            {
                Update();
                Thread.Sleep(5000);
                if (Timer.PostingTime())
                {
                    ConsoleHelper.WriteIf(RunArgs.Verbose, "Begin post... ");
                    SendOutMessages();
                    Properties.Settings.Default.LastPost = DateTime.UtcNow;
                    Properties.Settings.Default.Save();
                    ConsoleHelper.WriteLineIf(RunArgs.Verbose, "done.");
                }
            }
        }
    }
}
