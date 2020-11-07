/*
 If there is more names on class written by, then it means we wrote it in collaboration.
===============================
 CLASS WRITTEN BY: Rasmus Søborg (S185119) and Hans Krogh Thomsen (S185110)
 CREATE DATE: 22/06/2020
 PURPOSE: This class is the solver of a solitaire game.
 SPECIAL NOTES:
 https://www.chessandpoker.com/solitaire_strategy.html
===============================
*/

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Permissions;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Deck
{
    public class SolitaireLogicComponent
    {
        private readonly object mutex = new object();

        private string NextMoveString;

        public string GetNextMove() { lock (mutex) { return NextMoveString; } }

        private StateController StateController;

        private bool LookUpNewDeckCard = false;
        private int LookUpBottomCardIndex = -1;
        private bool HasUncoveredCard = false;
        private int UncoveredCardIndex = default;
        private bool FoundBottomCard = false;

        public int AwaitingCardIndex() => LookUpBottomCardIndex;
        public bool AwaitingCard() => LookUpBottomCardIndex >= 0;
        public bool HasFoundDeckCard() => FoundBottomCard;

        private List<string> Moves = new List<string>();
        private List<CardType> KnownCards = new List<CardType>();

        public SolitaireLogicComponent(StateController StateController, BoardController BoardController)
        {
            this.StateController = StateController;
            StateController.OnDeckChanged += StateController_OnDeckChanged;
            StateController.OnInitialized += StateController_OnInitialized;
            BoardController.OnBoardUpdate += BoardController_OnBoardUpdate;//for visually updating dynamically

            
        }

        private void StateController_OnInitialized(BoardModel Board)
        {
            // Add all aldready observed cards..
            KnownCards.Add(Board.DeckCard.Type);

            foreach (var Card in Board.Bottom)
                if (Card != default && Card.Uncovered)
                        KnownCards.Add(Card.Type);
            foreach (var Card in Board.Top)
                if (Card != default && Card.Uncovered)
                        KnownCards.Add(Card.Type);
        }

        private void BoardController_OnBoardUpdate(BoardModel Board)
        {
            // Lookup Changes
            //if (LookUpNewDeckCard)
            //{
            var NewDeckCard = Board.DeckCard;
            StateController.DeckCard = NewDeckCard;
            //}

            if (LookUpBottomCardIndex >= 0)
            {
                CardModel NewUncoveredCard = null;

                foreach (var Card in Board.Bottom)
                {
                    if (Card != null)
                    {
                        if (!KnownCards.Contains(Card.Type))
                        {
                            NewUncoveredCard = Card;
                        }
                    }
                }

                if (NewUncoveredCard == null) return;

                if (!HasUncoveredCard)
                {
                    for (int i = StateController.Bottom[LookUpBottomCardIndex].Count - 1; i >= 0; i--)
                        if (StateController.Bottom[LookUpBottomCardIndex][i].Uncovered == false)
                        {
                            StateController.Bottom[LookUpBottomCardIndex][i] = NewUncoveredCard;
                            UncoveredCardIndex = i;
                            HasUncoveredCard = true;
                            break;
                        }
                }

                StateController.Bottom[LookUpBottomCardIndex][UncoveredCardIndex] = NewUncoveredCard;
                FoundBottomCard = true;
            }
        }

        private void StateController_OnDeckChanged(CardModel DeckCard, List<CardModel>[] ColorDeck, List<CardModel>[] Deck, BoardModel Board)
        {
            // Add all aldready observed cards..
            /*
            foreach (var Card in Board.Bottom)
                if (Card != default && Card.Uncovered)
                    if (!KnownCards.Contains(Card.Type))
                        KnownCards.Add(Card.Type);
            foreach (var Card in Board.Top)
                if (Card != default && Card.Uncovered)
                    if (!KnownCards.Contains(Card.Type))
                        KnownCards.Add(Card.Type);

            if (Board.DeckCard != default && Board.DeckCard.Type != default)
                if (!KnownCards.Contains(Board.DeckCard.Type))
                    KnownCards.Add(Board.DeckCard.Type);
            */

            // If we have a new card discovered add it to the known cards
            if (LookUpBottomCardIndex >= 0)
                for (int i = StateController.Bottom[LookUpBottomCardIndex].Count - 1; i >= 0; i--)
                    if (StateController.Bottom[LookUpBottomCardIndex][i].Uncovered)
                        if (!KnownCards.Contains(StateController.Bottom[LookUpBottomCardIndex][i].Type))
                            KnownCards.Add(StateController.Bottom[LookUpBottomCardIndex][i].Type);

            if (LookUpNewDeckCard)
                if (!KnownCards.Contains(Board.DeckCard.Type))
                    KnownCards.Add(Board.DeckCard.Type);

            // Reset Lookup Variables
            HasUncoveredCard = false;
            LookUpNewDeckCard = false;
            LookUpBottomCardIndex = -1;
            UncoveredCardIndex = -1;
            FoundBottomCard = false;

            // Check if game has finished
            bool HasNotFinished = false;
            for (int i = 0; i < 4; i++)
            {
                if (StateController.Top.Count() != 13) 
                {
                    HasNotFinished = true;
                    break;
                }
            }

            if (!HasNotFinished) // Finished
            {
                MessageBox.Show("The game has finished!", "Finished");
                return;
            }

            // If there is an Ace then move it to color stack
            if (MoveAceToColorStack(DeckCard, ColorDeck, Deck, Board))
                goto End;

            // Get all moves we can do!
            var AllMoves = GetAllMoves();

            // No more moves
            if (AllMoves.Count == 0)
            {
                Moves.Add("Turn a deck card");
                LookUpNewDeckCard = true;
                goto End;
            }

            // Find the move with most covered cards in it
            CardMove MoveWithMostCoveredCards = default;
            int CoveredCardCount = -1;
            foreach (var Move in AllMoves)
            {
                // Find how much covered shit we have
                if (CoveredCardCount == -1 && Move.FromPosition == -1) {
                    MoveWithMostCoveredCards = Move;
                };

                if (Move.FromPosition == -1)
                    continue;

                var CurrentCoveredCards = StateController.Bottom[Move.FromPosition].Count(c => !c.Uncovered);
                if (CurrentCoveredCards > CoveredCardCount)
                {
                    MoveWithMostCoveredCards = Move;
                    CoveredCardCount = CurrentCoveredCards;
                }
            }

            // Perform shit move
            if (MoveWithMostCoveredCards.MoveToTop)
                MoveCardToTop(MoveWithMostCoveredCards.FromPosition, MoveWithMostCoveredCards.ToPosition, MoveWithMostCoveredCards.CardIndex);
             else
                MoveCardBottom(MoveWithMostCoveredCards.FromPosition, MoveWithMostCoveredCards.ToPosition, MoveWithMostCoveredCards.CardIndex);
            
        End:
            SaveMoves();
        }

        private void SaveMoves()
        {
            lock (mutex)
            {
                StringBuilder @out = new StringBuilder();
                @out.AppendLine("");

                if (Moves.Count < 5)
                {
                    int moveIndex = 1;
                    foreach (var Move in Moves)
                    {
                        @out.AppendLine($"{moveIndex++}. {Move}");
                    }
                }
                else
                {
                    for (int i = Moves.Count - 5; i < Moves.Count; i++)
                    {
                        @out.AppendLine($"{i + 1}. {Moves[i]}");
                    }  
                }

                NextMoveString = @out.ToString();
            }
        }

        private bool MoveAceToColorStack(CardModel DeckCard, List<CardModel>[] ColorDeck, List<CardModel>[] Deck, BoardModel Board)
        {
            for (int i = 6; i >= 0; i--)
            {
                int CardIndex = 0;
                foreach (var Card in Deck[i])
                {
                    if (IsAce(Card))
                    {
                        if (Deck[i].Count(c => c.Uncovered) > 0) // Dont try to lookup a new card in a deck that has no uncovered cards
                        { 
                            LookUpBottomCardIndex = i;
                            Moves.Add($"Uncover New Card");
                        }

                        MoveCardToTop(i, DetermineTopPosition(Card), CardIndex);
                        return true;
                    }

                    CardIndex++;
                }
            }

            if (IsAce(DeckCard))
            { 
                LookUpNewDeckCard = true;
                Moves.Add($"Move {GetCardName(DeckCard)} to Color Stack");
                Moves.Add($"Draw a new card from deck.");
                StateController.Top[DetermineTopPosition(DeckCard)].Add(DeckCard);
                StateController.DeckCard = null;
                return true;
            }

            return false;
        }

        public List<CardMove> GetAllMoves()
        {
            var moves = new List<CardMove>();
            var bottomMoves = new int[] { 0, 1, 2, 3, 4, 5, 6 };
            var topMoves = new int[] { 0, 1, 2, 3 };

            for (int FromPosition = 0; FromPosition < 7; FromPosition++)
            {
                if (StateController.Bottom[FromPosition] != default)
                {
                    bool FirstBottomReached = false; //only look for first card in bottom of the stack if we do a bottom move

                    for (int CardIndex = 0; CardIndex < StateController.Bottom[FromPosition].Count; CardIndex++)
                    {
                        // If we have reached an covered card then stop looking in this index
                        if (!StateController.Bottom[FromPosition][CardIndex].Uncovered)
                            continue;

                        if (!FirstBottomReached)
                        {
                            // Try all moves in bottom
                            foreach (var ToPosition in bottomMoves)
                            {
                                if (ToPosition == FromPosition) continue;//dont try to move to same location
                                if (ValidMoveBottom(FromPosition, ToPosition, CardIndex))
                                    moves.Add(new CardMove()
                                    {
                                        MoveToTop = false,
                                        FromPosition = FromPosition,
                                        ToPosition = ToPosition,
                                        CardIndex = CardIndex
                                    });
                            }

                            FirstBottomReached = true;
                        }

                        // Try all moves to top
                        // Only try to move to top if u are touching the last card in the shit
                        if (CardIndex == StateController.Bottom[FromPosition].Count - 1)
                            foreach (var ToPosition in topMoves)
                            {
                                if (ValidMoveTop(FromPosition, ToPosition, CardIndex))
                                    moves.Add(new CardMove()
                                    {
                                        MoveToTop = true,
                                        FromPosition = FromPosition,
                                        ToPosition = ToPosition,
                                        CardIndex = CardIndex
                                    });
                            }
                    }
                }
            }

            // Try all moves deck to bottom
            foreach (var ToPosition in bottomMoves)
            {
                if (ValidMoveDeckBottom(ToPosition))
                    moves.Add(new CardMove()
                    {
                        MoveToTop = false,
                        FromPosition = -1,
                        ToPosition = ToPosition
                    });
            }

            // Try all moves to top
            foreach (var ToPosition in topMoves)
            {
                if (ValidMoveDeckTop(ToPosition))
                    moves.Add(new CardMove()
                    {
                        MoveToTop = true,
                        FromPosition = -1,
                        ToPosition = ToPosition
                    });
            }

            return moves;
        }

        private bool ValidMoveTop(int BottomPosition, int TopPosition, int CardIndex)
        {
            var CardToMove = StateController.Bottom[BottomPosition][CardIndex];

            if (StateController.Top[TopPosition].Count == 0)
                return IsAce(CardToMove) && DetermineTopPosition(CardToMove) == TopPosition;
            
            var CardToLandOn = StateController.Top[TopPosition].Last();

            if (!CardToMove.Uncovered) return false;
            if (!CardToLandOn.Uncovered) return false;

            if (DetermineTopPosition(CardToMove) != TopPosition) return false;//only same type on eachother
            var ValueOfCardToMove = GetValueOfCard(CardToMove);
            var ValueOfCardToLand = GetValueOfCard(CardToLandOn);
            if (ValueOfCardToLand+1 == ValueOfCardToMove) return true;
            return false;
        }

        private int GetValueOfCard(CardModel Card)
        {
            var TypeStr = Card.Type.ToString();
            TypeStr = TypeStr.Substring(1, TypeStr.Length - 1);

            var ChArr = TypeStr.ToCharArray();
            if (ChArr.Length >= 3)
            {
                // 10
                return  int.Parse(ChArr[0].ToString() + ChArr[1].ToString());
            } else
            {
                switch (ChArr[0].ToString())
                {
                    case "A":
                        return 1;
                    case "K":
                        return 13;
                    case "Q":
                        return 12;
                    case "J":
                        return 11;
                    default: return int.Parse(ChArr[0].ToString());
                }
            }

        }

        private bool ValidMoveBottom(int Bottom1, int Bottom2, int CardIndex)
        {
            var CardToMove = StateController.Bottom[Bottom1][CardIndex];
            if (CardToMove.Uncovered && IsAce(CardToMove)) return false;
            var CardToLandOn = StateController.Bottom[Bottom2].LastOrDefault();


            if (IsKing(CardToMove))
            {
                // Dont move a king if it 0 bottom stack
                if (CardIndex == 0)
                {
                    return false;
                }
            }

            if (CardToLandOn == default)
            {
                // Move kings on empty spaces
                if (IsKing(CardToMove)) return true;
                return false;
            }


            if (!CardToMove.Uncovered) return false;
            if (!CardToLandOn.Uncovered) return false;
            
            var ValueOfCardToMove = GetValueOfCard(CardToMove);
            var ValueOfCardToLand = GetValueOfCard(CardToLandOn);

            if (ValueOfCardToLand - 1 == ValueOfCardToMove && ((!IsBlack(CardToMove) && IsBlack(CardToLandOn)) || 
                (IsBlack(CardToMove) && !IsBlack(CardToLandOn)))) return true;

            return false;
        }

        private bool ValidMoveDeckBottom(int Bottom2)
        {
            var CardToMove = StateController.DeckCard;
            if (CardToMove.Uncovered && IsAce(CardToMove)) return false;
            var CardToLandOn = StateController.Bottom[Bottom2].LastOrDefault();

            if (CardToLandOn == default)
            {
                // Move kings on empty spaces
                if (IsKing(CardToMove)) return true;
                return false;
            }

            if (!CardToMove.Uncovered) return false;
            if (!CardToLandOn.Uncovered) return false;

            var ValueOfCardToMove = GetValueOfCard(CardToMove);
            var ValueOfCardToLand = GetValueOfCard(CardToLandOn);
            if (ValueOfCardToLand - 1 == ValueOfCardToMove && ((!IsBlack(CardToMove) && IsBlack(CardToLandOn)) ||
                (IsBlack(CardToMove) && !IsBlack(CardToLandOn)))) return true;

            return false;
        }

        private bool ValidMoveDeckTop(int TopPosition)
        {
            var CardToMove = StateController.DeckCard;

            if (StateController.Top[TopPosition].Count == 0)
                return IsAce(CardToMove) && DetermineTopPosition(CardToMove) == TopPosition;

            var CardToLandOn = StateController.Top[TopPosition].Last();

            if (!CardToMove.Uncovered) return false;
            if (!CardToLandOn.Uncovered) return false;

            if (DetermineTopPosition(CardToMove) != TopPosition) return false;//only same type on eachother
            var ValueOfCardToMove = GetValueOfCard(CardToMove);
            var ValueOfCardToLand = GetValueOfCard(CardToLandOn);
            if (ValueOfCardToLand + 1 == ValueOfCardToMove) return true;
            return false;
        }

        private bool IsBlack(CardModel Card) => IsClubs(Card) || IsSpar(Card);

        private void MoveCardBottom(int BottomPosition, int BottomPositionTwo, int CardMoveIndex)
        {
            var CardsToMove = new List<CardModel>();

            // Find the cards that we want to move
            if (BottomPosition == -1)//-1 represents the deck card. 
            {
                CardsToMove.Add(StateController.DeckCard);
            } else
            { // move all cards over the card we want to move
                for (int CardIndex = CardMoveIndex; CardIndex < StateController.Bottom[BottomPosition].Count; CardIndex++)
                {
                    // Dont move covered cards
                    if (!StateController.Bottom[BottomPosition][CardIndex].Uncovered)
                        continue;

                    // Add uncovered card to be part of movement!
                    CardsToMove.Add(StateController.Bottom[BottomPosition][CardIndex]);
                }
            }


            // When we move a card from a bottom position to another bottom position we want to 
            // lookup if there is a card beneath that is covered.
            if (BottomPosition != -1 && StateController.Bottom[BottomPosition].Count(c => !c.Uncovered) > 0) // Dont try to lookup a new card in a deck that has no uncovered cards
                LookUpBottomCardIndex = BottomPosition;
            else if (BottomPosition == -1)
                LookUpNewDeckCard = true;

            // Find name of the card we want to move this (This can be empty)
            var NameToMoveTo = "";
            var LandCard = StateController.Bottom[BottomPositionTwo].LastOrDefault();
            if (LandCard != default) NameToMoveTo = GetCardName(LandCard);
            else NameToMoveTo = $"stack {BottomPositionTwo + 1}";

            // Promt the movement
            Moves.Add($"Move {GetCardName(CardsToMove.First())} to {NameToMoveTo}");

            // Move the given cards to the top of the other stack
            foreach (var CardToMove in CardsToMove)
                StateController.Bottom[BottomPositionTwo].Add(CardToMove);

            if (BottomPosition != -1)
                foreach (var CardToMove in CardsToMove)
                    StateController.Bottom[BottomPosition].Remove(CardToMove);
            else
                StateController.DeckCard = null;
        }

        private void MoveCardToTop(int BottomPosition, int TopPosition, int CardMoveIndex)
        {
            var CardsToMove = new List<CardModel>();

            // Find the cards that we want to move
            if (BottomPosition == -1)//-1 represents the deck card. 
            {
                CardsToMove.Add(StateController.DeckCard);
            }
            else
            { // move all cards over the card we want to move
                for (int CardIndex = CardMoveIndex; CardIndex < StateController.Bottom[BottomPosition].Count; CardIndex++)
                {
                    CardsToMove.Add(StateController.Bottom[BottomPosition][CardIndex]);
                }
            }

            // When we move a card from a bottom position to another bottom position we want to 
            // lookup if there is a card beneath that is covered.
            if (BottomPosition != -1 && StateController.Bottom[BottomPosition].Count(c => c.Uncovered) > 0) // Dont try to lookup a new card in a deck that has no uncovered cards
                LookUpBottomCardIndex = BottomPosition;
            else if (BottomPosition == -1)
                LookUpNewDeckCard = true;

            // Promt the movement
            Moves.Add($"Move {GetCardName(CardsToMove.First())} to Color Stack.");

            // Move the given card to the top of the other stack
            foreach (var CardToMove in CardsToMove)
                StateController.Top[TopPosition].Add(CardToMove);

            if (BottomPosition != -1)
                foreach (var CardToMove in CardsToMove)
                    StateController.Bottom[BottomPosition].Remove(CardToMove);
            else
                StateController.DeckCard = null;
        }

        private bool IsAce(CardModel Card)
        {
            return Card.Type == CardType._Ac || Card.Type == CardType._Ad ||
                Card.Type == CardType._Ah || Card.Type == CardType._As;
        }

        private bool IsKing(CardModel Card)
        {
            return Card.Type == CardType._Kc || Card.Type == CardType._Kd ||
                Card.Type == CardType._Kh || Card.Type == CardType._Ks;
        }

        private int DetermineTopPosition(CardModel Card)
        {
            if (IsHeart(Card)) return 0;
            if (IsClubs(Card)) return 1;
            if (IsDiamonds(Card)) return 2;
            if (IsSpar(Card)) return 3;
            throw new ArgumentException("Unknown Type.");
        }

        private bool IsHeart(CardModel Card) { return Card.Type.ToString().Contains("h"); }
        private bool IsClubs(CardModel Card) { return Card.Type.ToString().Contains("c"); }
        private bool IsDiamonds(CardModel Card) { return Card.Type.ToString().Contains("d"); }
        private bool IsSpar(CardModel Card) { return Card.Type.ToString().Contains("s"); }

        private string GetCardName(CardModel Card)
        {
            var Name = Card.Type.ToString();

            if (Name.StartsWith("_"))
                Name = Name.Replace("_", string.Empty);
            
            return Name;
        }
    }
}
