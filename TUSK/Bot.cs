using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Telegram.Bot;
using System.Threading;
using Markov;
using Telegram.Bot.Types;

namespace TUSK
{
    class Bot
    {
        private Api Telegram;
        private List<long> ChatIds;
        private MarkovChain<string> Chain;
        public Bot(string apiKey)
        {
            Telegram = new Api(apiKey);
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
                Chain.Add(lin.Split(splitterChars, StringSplitOptions.RemoveEmptyEntries));
            }
        }

        /// <summary>
        /// WHAT FUCKING EEEEEEVER DUUUUUUDE
        /// </summary>
        public void Init()
        {
            ChatIds = DatabaseAccess.GetActiveChats();
            Chain = new MarkovChain<string>(2);
            List<string> messages = DatabaseAccess.GetAllMessages();
            foreach (string str in messages)
            {
                Feed(str);
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
        internal async void PostMessage(string message, long chatId)
        {
            await Telegram.SendTextMessage(chatId, message);
        }

        public void SendOutMessages()
        {
            string chain = Generate();
            foreach (long id in ChatIds)
            {
                PostMessage(chain, id);
            }
        }

        public void UnsubChat(long chatid)
        {
            if (ChatIds.Contains(chatid) == false)
            {
                PostMessage("WhatEVER dude you weren't in my life ANYWAY fucking cuck", chatid);
                return;
            }
            ChatIds.Remove(chatid);
            DatabaseAccess.DeactivateChat(chatid);
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
        private Update[] updateArray;
        private async void GetUpdates()
        {
            try
            {
                updateArray = await Telegram.GetUpdates(Properties.Settings.Default.LastReadMessage);
            }
            catch (Exception e)
            {
                updateArray = null;
            }
        }

        private void Update()
        {
            GetUpdates();
            while (updateArray == null)
            {
                /* WOO I HAVE TO FUCKIGHJBN WAIT BECAUSE FUCK THREADS AAAAAAAAAAAAA */
            }
            if (updateArray == null)
            {
                return;
            }
            foreach (Update update in updateArray)
            {
                if (update.Type == UpdateType.MessageUpdate && update.Message.Text != null)
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
                Properties.Settings.Default.LastReadMessage = update.Id + 1;
                Properties.Settings.Default.Save();
            }
            updateArray = null;
        }

        public void Run()
        {
            while (true)
            {
                Update();
                Thread.Sleep(5000);
                if (Timer.PostingTime())
                {
                    SendOutMessages();
                    Properties.Settings.Default.LastPost = DateTime.UtcNow;
                }
            }
        }
    }
}
