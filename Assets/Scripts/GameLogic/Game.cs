using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace PlayerSlayer
{
    public class Game
    {
        // Public Fields
        public Slayer Guest { get; set; }
        public Slayer Host { get; set; }

        public int? HostLock;
        public int? GuestLock;

        public bool GameOver;
        public int Victor;

        public bool? HostRematch = null;
        public bool? GuestRematch = null;

        public int MaxHealth = 3;

        public static Card Nothing = new Card() { Name = "Nothing", Priority = 10 };
        public static Card[] Cards = {
                new Card() { Name = "Roll", Priority = 2, StunAfter = false },
                new Card() { Name = "Guard", Priority = 0, StunAfter = false },
                new Card() { Name = "Quick Attack", Priority = 1, StunAfter = false },
                new Card() { Name = "Normal Attack", Priority = 3, StunAfter = false },
                new Card() { Name = "Heavy Attack", Priority = 4, StunAfter = true },
        };
        
        int breaths;
        Stack<Card> Deck { get; set; }

        private static System.Random rng = new System.Random();

        public Game()
        {
            Initialize();
        }

        public void Initialize()
        {
            Guest = new Slayer();
            Host = new Slayer();

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

            if (breaths >= 4 && !GameOver)
            {
                breaths = 0;
                ShuffleDeck();
                FillHands();
                Guest.Health++;
                Host.Health++;

                if (Guest.Health > MaxHealth)
                    Guest.Health = MaxHealth;
                
                if (Host.Health > MaxHealth)
                    Host.Health = MaxHealth;
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
            while (Host.Hand.Count < 5)
            {
                Host.Hand.Add(Deck.Pop());
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
            GameOver = !(Host.Health > 0 && Guest.Health > 0);
        }

        public void ProgressTurn()
        {
            bool clashed = false;

            if (HostLock >= Host.Hand.Count)
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
            Card HostPlay = Host.Stunned ? Nothing : Host.Hand[HostLock ?? 0];

            Guest.Stunned = false;
            Host.Stunned = false;

            if (!(cpuPlay == Nothing))
            {
                Guest.Hand.Remove(cpuPlay);
                Deck.Push(cpuPlay);
            }

            if (!(HostPlay == Nothing))
            {
                Host.Hand.Remove(HostPlay);
                Deck.Push(HostPlay);
            }

            Debug.Log($"You played {HostPlay.Name}. I played {cpuPlay.Name}.");

            List<Card> plays = new List<Card>() { cpuPlay, HostPlay };
            clashed = cpuPlay.Priority == HostPlay.Priority;
            plays = plays.OrderBy(c => c.Priority).ToList();

            foreach(Card card in plays)
            {
                if (card == Nothing)
                {
                    continue;
                }

                Slayer target = card == cpuPlay ? Host : Guest;
                Slayer actor = card == cpuPlay ? Guest : Host;

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
            Host.Cleanse();
        }
    }
}
