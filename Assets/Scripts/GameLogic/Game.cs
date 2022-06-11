using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace PlayerSlayer
{
    public class Game
    {
        public Slayer Guest { get; set; }
        public Slayer User { get; set; }
        Stack<Card> Deck { get; set; }
        public static Card Nothing = new Card() { Name = "Nothing", Priority = 10 };
        public static Card[] Cards = {
                new Card() { Name = "Roll", Priority = 2, StunAfter = false },
                new Card() { Name = "Guard", Priority = 0, StunAfter = false },
                new Card() { Name = "Quick Attack", Priority = 1, StunAfter = false },
                new Card() { Name = "Normal Attack", Priority = 3, StunAfter = false },
                new Card() { Name = "Heavy Attack", Priority = 4, StunAfter = true },
        };
        
        int breaths;

        public int? HostLock;
        public int? GuestLock;

        private static System.Random rng = new System.Random();

        public Game()
        {
            Initialize();
        }

        public void Initialize()
        {
            Guest = new Slayer();
            User = new Slayer();

            Debug.Log($"Very well. Prepare to lose.");

            FillDeck();
            ShuffleDeck();
            FillHands();
            breaths = 0;
        }

        public void TriggerTurn()
        {
            breaths++;
            ProgressTurn();

            if (breaths >= 4)
            {
                breaths = 0;
                ShuffleDeck();
                FillHands();
            }

            GuestLock = null;
            HostLock = null;
        }

        public void FillDeck()
        {
            Deck = new Stack<Card>();

            for (int i = 0; i < 3; i++)
            {
                Deck.Push(new Card() { Name = "Roll", Priority = 2, StunAfter = false });
                Deck.Push(new Card() { Name = "Guard", Priority = 0, StunAfter = false });
                Deck.Push(new Card() { Name = "Quick Attack", Priority = 1, StunAfter = false });
                Deck.Push(new Card() { Name = "Normal Attack", Priority = 3, StunAfter = false });
                Deck.Push(new Card() { Name = "Heavy Attack", Priority = 4, StunAfter = true });
            }
        }

        public void FillHands()
        {
            while (User.Hand.Count < 5)
            {
                User.Hand.Add(Deck.Pop());
            }

            while (Guest.Hand.Count < 5)
            {
                Guest.Hand.Add(Deck.Pop());
            }
        }

        public static List<T> Shuffle<T>(List<T> list)
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

        public void ShuffleDeck()
        {
            List<Card> decklist = Deck.ToList();
            Deck = new Stack<Card>(Shuffle(decklist));
        }

        public void DisplayState()
        {
            if (User.Health > 0 && Guest.Health > 0)
            {
                Debug.Log($"You have {User.Health} health. I have {Guest.Health} health.");
                Debug.Log($"These are the cards in your hand:");
                for (int i = 0; i < User.Hand.Count; i++)
                {
                    Debug.Log($"[{i}]: {User.Hand[i].Name}");
                }
            }
            else if (User.Health <= 0)
            {
                Debug.Log("Ha! I win.");
            }
            else
            {
                Debug.Log("How could you win? Did you cheat?");
            }
        }

        public void ProgressTurn()
        {
            bool clashed = false;

            if (HostLock >= User.Hand.Count)
            {
                Debug.Log("Invalid index.");
                DisplayState();
            }

            if (GuestLock >= Guest.Hand.Count)
            {
                Debug.Log("Invalid index.");
                DisplayState();
            }

            Card cpuPlay = Guest.Stunned ? Nothing : Guest.Hand[GuestLock ?? 0];
            Card userPlay = User.Stunned ? Nothing : User.Hand[HostLock ?? 0];

            Guest.Stunned = false;
            User.Stunned = false;

            if (!(cpuPlay == Nothing))
            {
                Guest.Hand.Remove(cpuPlay);
                Deck.Push(cpuPlay);
            }

            if (!(userPlay == Nothing))
            {
                User.Hand.Remove(userPlay);
                Deck.Push(userPlay);
            }

            Debug.Log($"You played {userPlay.Name}. I played {cpuPlay.Name}.");

            List<Card> plays = new List<Card>() { cpuPlay, userPlay };
            clashed = cpuPlay.Priority == userPlay.Priority;
            plays = plays.OrderBy(c => c.Priority).ToList();

            foreach(Card card in plays)
            {
                if (card == Nothing)
                {
                    continue;
                }

                Slayer target = card == cpuPlay ? User : Guest;
                Slayer actor = card == cpuPlay ? Guest : User;

                switch(card.Priority)
                {
                    case 0:
                        actor.Guarded = true;
                        break;
                        
                    case 1:
                        if (clashed || target.Damage(1) <= 0)
                        {
                            DisplayState();
                            return;
                        }
                        break;

                    case 2:
                        actor.Rolled = true;
                        break;

                    case 3:
                        if (clashed || target.Damage(2) <= 0)
                        {
                            DisplayState();
                            return;
                        }
                        break;

                    case 4:
                        if (clashed || target.Damage(3) <= 0)
                        {
                            DisplayState();
                            return;
                        }
                        break;
                }

                if (card.StunAfter)
                    actor.Stunned = true;
            }

            Guest.Cleanse();
            User.Cleanse();
        }
    }
}
