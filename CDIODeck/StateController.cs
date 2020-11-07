/*
 If there is more names on class written by, then it means we wrote it in collaboration.
===============================
 CLASS WRITTEN BY: Rasmus Søborg (S185119)
 CREATE DATE: 23/06/2020
 PURPOSE: This class handles the state of the game
 SPECIAL NOTES: Primarily used by logic component through events.
===============================
*/

using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Remoting.Messaging;
using System.Text;
using System.Threading.Tasks;

namespace Deck
{
    public class StateController
    {
        public List<CardModel>[] Top = new List<CardModel>[4];
        public List<CardModel>[] Bottom = new List<CardModel>[7];

        public bool Initialized { get; set; } = false;

        public CardModel DeckCard;

        public delegate void DeckChanged(CardModel DeckCard, List<CardModel>[] ColorDeck, List<CardModel>[] Deck, BoardModel LastBoard);
        public event DeckChanged OnDeckChanged;

        public delegate void ControllerInitialized(BoardModel Board);
        public event ControllerInitialized OnInitialized;

        public StateController()
        {
            for (int i = 0; i < Top.Length; i++) Top[i] = new List<CardModel>();
            for (int i = 0; i < Bottom.Length; i++) Bottom[i] = new List<CardModel>();

            var CoveredCard = new CardModel();
            CoveredCard.Type = CardType.Covered;
            CoveredCard.Uncovered = false;

            Bottom[1].Add(CoveredCard);
            
            for (int i = 0; i < 2; i++)
                Bottom[2].Add(CoveredCard);

            for (int i = 0; i < 3; i++)
                Bottom[3].Add(CoveredCard);

            for (int i = 0; i < 4; i++)
                Bottom[4].Add(CoveredCard);

            for (int i = 0; i < 5; i++)
                Bottom[5].Add(CoveredCard);

            for (int i = 0; i < 6; i++)
                Bottom[6].Add(CoveredCard);
        }

        public void InitializeBoard(BoardModel StartBoard)
        {
            for (int i = 0; i < StartBoard.Bottom.Length; i++)
                if (StartBoard.Bottom[i] != default)
                    Bottom[i].Add(StartBoard.Bottom[i]);

            for (int i = 0; i < StartBoard.Top.Length; i++)
                if (StartBoard.Top[i] != default)
                    Top[i].Add(StartBoard.Bottom[i]);

            DeckCard = StartBoard.DeckCard;
            OnInitialized?.Invoke( StartBoard );
        }

        public void UpdateBoardState(BoardModel CurrentState)
        {
            OnDeckChanged?.Invoke(GetDeckCard(), GetTopDeck(), GetBottomDeck(), CurrentState);
        }

        public List<CardModel>[] GetTopDeck() => Top;
        public List<CardModel>[] GetBottomDeck() => Bottom;

        public CardModel GetDeckCard() => DeckCard;


    }
}
