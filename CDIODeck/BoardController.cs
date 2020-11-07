/*
 If there is more names on class written by, then it means we wrote it in collaboration.
===============================
 CLASS WRITTEN BY: Rasmus Søborg (S185119)
 CREATE DATE: 10/06/2020
 PURPOSE: The board controller is responsible for handling the current board
 SPECIAL NOTES: 
===============================
*/

using ComputerVision;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Deck
{
    public class BoardController
    {
        private readonly object mutex = new object();

        private BoardModel Board = new BoardModel();
        private CvCardTransformer Transformer;

        private const int ConsistencyBuffer = 15;
        private const int ConsistencyUpdatesPerCheck = 15;
        private const int MinConsistencyRequired = 1;
        private bool BufferFull = false;

        private int CurrentDeterminedBoardModelIndex = 0;
        private BoardModel[] LastDeterminedBoardModels = new BoardModel[ConsistencyBuffer]; // We save the last 10 determined board models to ensure that the data is consistent.

        public delegate void BoardUpdate(BoardModel NewBoard);
        public event BoardUpdate OnBoardUpdate;

        public BoardModel GetBoard() 
        {
            lock (mutex)
            {
                return Board;
            }
        }

        public BoardController()
        {
            Transformer = new CvCardTransformer();
        }

        public CardModel[] Transformed => Transformer.GetTransformedObservations();

        public void UpdateBoardWithObservations(CvModel[] Observations)
        {
            lock (mutex)
            {                
                Transformer.GetBoardState(Observations);
                LastDeterminedBoardModels[CurrentDeterminedBoardModelIndex++] = Transformer.GetBoardState(Observations); 
                if (CurrentDeterminedBoardModelIndex > ConsistencyBuffer - 1) 
                {
                    BufferFull = true;
                    CurrentDeterminedBoardModelIndex = 0;
                }

                if (CurrentDeterminedBoardModelIndex % ConsistencyUpdatesPerCheck == 0 && BufferFull)
                {
                    // Check for most agreed consistent data
                    var dict = new Dictionary<BoardModel, int>();

                    foreach (var value in LastDeterminedBoardModels)
                    {
                        if (dict.ContainsKey(value))
                            dict[value]++;
                        else
                            dict[value] = 1;
                    }

                    KeyValuePair<BoardModel, int> MostOccuredModel = new KeyValuePair<BoardModel, int>(new BoardModel(), 0);
                    foreach (var pair in dict)
                        if (pair.Key.CardsOnBoard() >= MostOccuredModel.Key.CardsOnBoard())
                            if (pair.Value > MostOccuredModel.Value && pair.Value > MinConsistencyRequired)
                                MostOccuredModel = pair;

                    Board = MostOccuredModel.Key;
                    OnBoardUpdate?.Invoke(Board);
                }

            }
        }



    }
}
