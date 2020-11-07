/*
 If there is more names on class written by, then it means we wrote it in collaboration.
===============================
 CLASS WRITTEN BY: Rasmus Søborg (S185119)
 CREATE DATE: 17/06/2020
 PURPOSE: This class transforms card observations from world space into a board state
 SPECIAL NOTES: 
===============================
*/

using ComputerVision;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Deck
{
    public class CvCardTransformer
    {
        private class TwinCardType
        {
            public CvModel[] Observations = new CvModel[2];
            public float Distance =>
                DeterminePointDistance(
                    new Point(Observations[0].BoundingBox.X + Observations[0].Look_Bounds.X, Observations[0].BoundingBox.Y + Observations[0].Look_Bounds.Y),
                    new Point(Observations[1].BoundingBox.X + Observations[1].Look_Bounds.X, Observations[1].BoundingBox.Y + Observations[1].Look_Bounds.Y)
                );

            private float DeterminePointDistance(Point p1, Point p2)
            {
                return (float)Math.Sqrt(
                    Math.Pow(p2.X - p1.X, 2) + 
                    Math.Pow(p2.Y - p1.Y, 2)
                );
            }
        }

        private class PointComparer : IComparer
        {
            private Point comparePoint;
            public PointComparer(Point comparePoint)
            {
                this.comparePoint = comparePoint;
            }

            public int Compare(object x, object y)
            {
                if (!(x is CardModel) || !(y is CardModel)) throw new ArgumentException("Point comparer can only be used to compare points");

                var model1 = x as CardModel;
                var model2 = y as CardModel;

                var d1 = DeterminePointDistance(comparePoint, model1.MinWorldPoint);
                var d2 = DeterminePointDistance(comparePoint, model2.MinWorldPoint);

                return d1.CompareTo(d2);
            }

            private float DeterminePointDistance(Point p1, Point p2)
            {
                return (float)Math.Sqrt(
                    Math.Pow(p2.X - p1.X, 2) +
                    Math.Pow(p2.Y - p1.Y, 2)
                );
            }
        }

        public CvCardTransformer() { }

        private List<CardModel> _TransformedObservations;

        public CardModel[] GetTransformedObservations()
        {
            if (_TransformedObservations != default)
                return _TransformedObservations.ToArray();
            return default;
        }

        public BoardModel GetBoardState(CvModel[] allObservations)
        {
            BoardModel @out = new BoardModel();

            List<CardModel> cardObservations = new List<CardModel>();
            List<string> determinedTypes = new List<string>();

            // Determine where each card is located from the observations
            foreach (var observation in allObservations)
            {
                // Skip cards that are already determined
                if (determinedTypes.Contains(observation.Type))
                    continue;

                // Add type as determined
                determinedTypes.Add(observation.Type);

                // Find all observations of this type
                var allObservationsOfType = allObservations.Where(c => c.Type == observation.Type).ToArray();

                // Determine twin pair (ignore potential mismatched types)
                var twinPairs = new List<TwinCardType>();    
                for (int i = 0; i < allObservationsOfType.Count() - 1; i++) 
                {  
                    for (int twinIndex = i + 1; i < allObservationsOfType.Count(); i++)
                    {
                        var TwinCards = new TwinCardType();
                        TwinCards.Observations[0] = allObservationsOfType[i];
                        TwinCards.Observations[1] = allObservationsOfType[twinIndex];

                        if (TwinCards.Distance > 0) twinPairs.Add(TwinCards);
                    }
                }

                if (twinPairs.Count <= 0) continue;
                var lowestPair = twinPairs.OrderBy(c => c.Distance).First();

                if (lowestPair.Observations[0] == default || lowestPair.Observations[1] == default) continue;

                // Determine middle position of two closest points
                var yPoints = new int[] { lowestPair.Observations[0].BoundingBox.Y + lowestPair.Observations[0].Look_Bounds.Y, lowestPair.Observations[1].BoundingBox.Y + lowestPair.Observations[1].Look_Bounds.Y }.OrderBy(y => y);
                var xPoints = new int[] { lowestPair.Observations[0].BoundingBox.X + lowestPair.Observations[0].Look_Bounds.X, lowestPair.Observations[1].BoundingBox.X + lowestPair.Observations[1].Look_Bounds.X }.OrderBy(x => x);

                

                int minY = yPoints.First(),
                    maxY = yPoints.Last(),
                    minX = xPoints.First(),
                    maxX = xPoints.Last();

                var minWorld = new Point(minX, minY);
                var maxWorld = new Point(maxX + lowestPair.Observations[0].BoundingBox.Width, maxY + lowestPair.Observations[0].BoundingBox.Height);

                // Add card to observation
                cardObservations.Add(new CardModel( $"_{observation.Type}" ) {
                    MinWorldPoint = minWorld,
                    MaxWorldPoint = maxWorld,
                    Confidence = observation.Confidence
                });
            }

            // Determine the top left card
            cardObservations.Sort((p1, p2) => new PointComparer(new Point(5, 5)).Compare(p1, p2));

            if (cardObservations.Count > 0)
            {
                var topLeft = cardObservations.First();
                @out.DeckCard = topLeft;


                // Make green zone for top + a margin
                var topGreenZoneMaxY = @out.DeckCard.MaxWorldPoint.Y + 25;
                var cardsInTopGreenZone = cardObservations.Where(c => c.MinWorldPoint.Y < topGreenZoneMaxY && c.Type != @out.DeckCard.Type);
                cardsInTopGreenZone.ToList().Sort((p1, p2) => new PointComparer(@out.DeckCard.MinWorldPoint).Compare(p1, p2));

                var cardInTopGreenZoneIndex = 0;
                foreach (var cardInGreenZone in cardsInTopGreenZone) 
                {
                    @out.Top[cardInTopGreenZoneIndex++] = cardInGreenZone;
                    if (cardInTopGreenZoneIndex > 3) break;
                }

                // Make green zone for bottom
                var topGreenZoneMinY = topGreenZoneMaxY;
                var cardsInBottomGreenZone = cardObservations.Where(c => c.MinWorldPoint.Y > topGreenZoneMinY);
                cardsInBottomGreenZone.ToList().Sort((p1, p2) => new PointComparer(new Point(5, topGreenZoneMinY)).Compare(p1, p2));
                cardInTopGreenZoneIndex = 0;

                foreach (var cardInGreenZone in cardsInBottomGreenZone)
                {
                    @out.Bottom[cardInTopGreenZoneIndex++] = cardInGreenZone;
                    if (cardInTopGreenZoneIndex > 6) break;
                }


            }

            _TransformedObservations = cardObservations;

            return @out;
        }



    }
}
