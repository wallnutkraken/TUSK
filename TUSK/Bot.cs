using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using Markov;
using Telegram.Bot;
using Telegram.Bot.Types;

namespace TUSK
{
    class Bot
    {
        [SuppressMessage("ReSharper", "FieldCanBeMadeReadOnly.Local")]
        private TelegramBotClient _telegram;
        private List<long> _chatIds;
        private MarkovChain<string> _chain;
        public Bot(string apiKey)
        {
           _telegram = new TelegramBotClient(apiKey);
        }

        private void Format(ref string str)
        {
            str = str.Replace("\"", "");
            str = str.Replace("?", " ?");
        }

        private readonly char[] _splitterChars = { ' ' };
        private readonly char[] _newlineArray = { '\n' };


        private void Feed(string txt)
        {
            string[] lines = txt.Split(_newlineArray, StringSplitOptions.RemoveEmptyEntries);
            foreach (string line in lines)
            {
                string lin = line;
                Format(ref lin);
                string[] words = lin.Split(_splitterChars, StringSplitOptions.RemoveEmptyEntries);
                List<string> goodWords = new List<string>();
                foreach (string word in words)
                {
                    if (CheckForLink(word) == false)
                    {
                        goodWords.Add(word);
                    }
                }
                _chain.Add(goodWords);
            }
        }

        private static bool CheckForLink(string word)
        {
            Uri uriResult;
            bool result = Uri.TryCreate(word, UriKind.Absolute, out uriResult)
                && (uriResult.Scheme == Uri.UriSchemeHttp || uriResult.Scheme == Uri.UriSchemeHttps);
            bool alternate = word.StartsWith("?v=");
            return result || alternate;
        }

        /// <summary>
        /// WHAT FUCKING EEEEEEVER DUUUUUUDE
        /// </summary>
        public void Init()
        {
            _chatIds = DatabaseAccess.GetActiveChats();
            _chain = new MarkovChain<string>(2);
            List<ITelegramDbEntry> messages = DatabaseAccess.GetAllMessages();
            List<string> messagesStr = FormatHelpers.CollectionToString(messages).ToList();
            if (RunArgs.Dump)
            {
                System.IO.File.WriteAllLines($"dbDump-{DateTime.Now.ToShortDateString()}-{DateTime.Now.Hour}.{DateTime.Now.Minute}.log", messagesStr);
            }
            foreach (ITelegramDbEntry entry in messages)
            {
                Feed(entry.Text);
            }
            if (RunArgs.Post)
            {
                SendOutMessages();
                Console.WriteLine("Done. Exiting.");
                Environment.Exit(0);
            }
        }

        internal string Generate()
        {
            List<string> chainWords = _chain.Chain().ToList();
            /* BUG: Sometimes the chain would be "". Unsure if it should be fixed. */
            var chain = chainWords.Aggregate("", (current, word) => current + (word + " "));
            if (chain == "")
            {
                return Generate(); // C# feels so yucky after writing Go for so long
            }
            return chain;
        }

        /// <summary>
        /// Posts a message to a chat
        /// </summary>
        internal void PostMessage(string message, long chatId)
        {
            try
            {
                var chat = new ChatId(chatId);
                var messageTask = _telegram.SendTextMessageAsync(chat, message);
            }
            catch (Exception)
            {
                RemoveChat(chatId);
            }
        }

        /// <summary>
        /// Vanilla sendout, generated.
        /// </summary>
        internal void SendOutMessages()
        {
            string chain = Generate();
            SendOutMessages(chain);
        }

        /// <summary>
        /// Override sendout, used for thinking
        /// </summary>
        /// <param name="overrideMsg">Message to send</param>
        internal void SendOutMessages(string overrideMsg)
        {
            ConsoleHelper.WriteLineIf(RunArgs.Verbose, "Generating chain...");
            string chain = overrideMsg;
            ConsoleHelper.WriteLineIf(RunArgs.Verbose, $"Generated chain: {chain}", ConsoleColor.DarkGreen);
            foreach (long id in _chatIds)
            {
                PostMessage(chain, id);
            }
        }

        private void RemoveChat(long chatid)
        {
            _chatIds.Remove(chatid);
            DatabaseAccess.DeactivateChat(chatid);
        }
        public void UnsubChat(long chatid)
        {
            if (_chatIds.Contains(chatid) == false)
            {
                PostMessage("WhatEVER dude you weren't in my life ANYWAY fucking cuck", chatid);
                return;
            }
            RemoveChat(chatid);
            PostMessage(Responses.UnSub(), chatid);
        }

        public void SubChat(long chatid)
        {
            if (_chatIds.Contains(chatid))
            {
                PostMessage("Yeah naw m8 you're already subscribed.", chatid);
                return;
            }
            _chatIds.Add(chatid);
            DatabaseAccess.AddChat(chatid);
            PostMessage(Responses.Sub(), chatid);
        }
        private Update[] GetUpdates()
        {
            var updateTask = _telegram.GetUpdatesAsync(Properties.Settings.Default.LastReadMessage);
            updateTask.Wait();
            return updateTask.Result;
        }

        private void Update()
        {
            Update[] updates = GetUpdates();
            foreach (Update update in updates)
            {
                if (update.Message.Text != null)
                {
                    ConsoleHelper.WriteLineIf(RunArgs.Verbose, $"[{update.Message.From.Id}]{update.Message.From.FirstName}: {update.Message.Text}");
                    if (update.Message.Text.StartsWith("/subscribe"))
                    {
                        SubChat(update.Message.Chat.Id);
                    }
                    else if (update.Message.Text.StartsWith("/unsubscribe"))
                    {
                        UnsubChat(update.Message.Chat.Id);
                    }
                    else if (update.Message.Text.Contains("crab"))
                    {
                        SendOutMessages();
                    }
                    else
                    {
                        Feed(update.Message.Text);
                        string[] lines = update.Message.Text.Split(_newlineArray, StringSplitOptions.RemoveEmptyEntries);
                        foreach (string line in lines)
                        {
                            DatabaseAccess.AddMessage(update.Message.Chat.Id, line);
                        }
                    }
                }
                Properties.Settings.Default.LastReadMessage = update.Id + 1;
                Properties.Settings.Default.Save();
            }
            if (updates.Length > 0 && Precentage.CheckChance(5))
            {
                SendOutMessages();
            }
        }

        [SuppressMessage("ReSharper", "FunctionNeverReturns")]
        public void Run()
        {
            ConsoleHelper.WriteLineIf(RunArgs.Verbose, "done.");
            ControlThread control = new ControlThread();
            control.Start();
            while (true)
            {
                Update();
                //Thread.Sleep(500);
                Timer.DumpTime();
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
