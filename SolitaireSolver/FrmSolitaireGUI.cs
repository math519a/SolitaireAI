/*
 If there is more names on class written by, then it means we wrote it in collaboration.
===============================
 CLASS WRITTEN BY: Rasmus Søborg (S185119)
 CREATE DATE: 06/06/2020
 PURPOSE: Window for showing graphical state of our StateController and handling logic component
 SPECIAL NOTES: 
===============================
*/

using ComputerVision;
using Deck;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Net;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace SolitaireSolver
{
    public partial class FrmSolitaireGUI : Form
    {
        Graphics graphics;
        Graphics backBufferGraphics;
        Bitmap backBuffer;
        Size originalSize;
        Size cardSize = new Size((int)(65.0 / 1.15d), (int)(100.0 / 1.15d)); //new Size(65, 100);

        //private Deck.SolitaireSolver Solver = new Deck.SolitaireSolver();
        private StateController StateController = new StateController();
        private SolitaireLogicComponent Logic;
        
        readonly object mutex = new object();
        public readonly BoardController BoardController;

        private Dictionary<CardType, Bitmap> CardImages = new  Dictionary<CardType, Bitmap>();
        private Bitmap BacksideCard;

        public FrmSolitaireGUI(FrmSolitaire Invoker)
        {
            InitializeComponent();
            LoadCards();

            BoardController = new BoardController();
            Logic = new SolitaireLogicComponent(StateController, BoardController);            
            originalSize = this.Size;

            ResizeBuffer(this.Width, this.Height);
            Resize += (s, e) => ResizeBuffer(((Form)s).Width, ((Form)s).Height);
            
            new Thread(() => {
                while (true)
                {
                    Thread.Sleep(15);
                    Draw();
                }
            }).Start();

            Invoker.OnScanComplete += Invoker_OnScanComplete;
            
        }
        
        private void LoadCards()
        {
            foreach(var CardType in Enum.GetValues(typeof(CardType)))
            {
                CardType currentType = (CardType)CardType;

                if (currentType == Deck.CardType.Covered)
                    continue;

                var Name = currentType
                    .ToString()
                    .Split('_')
                    .Last();

                CardImages[currentType] = (Bitmap)Image.FromFile($@"Data\{Name}.png");
            }

            BacksideCard = (Bitmap)Image.FromFile(@"Data\Backside.png");
        }
        private void Invoker_OnScanComplete(CvModel[] Observations)
        {
            BoardController.UpdateBoardWithObservations(Observations);
        }

        public void Draw()
        {
            lock (mutex)
            {
                backBufferGraphics.Clear(Color.Green);

                var boardState = BoardController.GetBoard();

                if (StateController.Initialized)
                {
                    if (StateController.DeckCard != null && StateController.DeckCard.Type != default)
                        DrawDeck(StateController.DeckCard.Type);
                    else
                        DrawDeck();
                }
                else
                {
                    if (boardState.DeckCard != default)
                        DrawDeck(boardState.DeckCard.Type);
                    else
                        DrawDeck();

                }


                if (StateController.Initialized)
                {
                    DrawColorStacks(StateController.Top);
                    DrawStack(StateController.Bottom);
                } else
                {
                    DrawColorStacks(boardState.Top);
                    DrawStack(boardState.Bottom);
                }

                DrawNextMove();

                graphics.DrawImage(backBuffer, new Point(0, 0));                
            }
        }

        private Size calculateRelativeSize(Size original, Size wantedSize, Size newSize)
        {
            //newSize = new Size(816, 489);

            var aspectRatio = (double)wantedSize.Height / wantedSize.Width;
            var calculatedWidth = (wantedSize.Width / (double)original.Width) * newSize.Width;

            return new Size(
                (int)calculatedWidth,
                (int)(calculatedWidth * aspectRatio)

                //(int)((wantedSize.Height / (double)original.Height) * newSize.Height)
            ); 
        }
        private Point calculateRelativePoint(Size original, Point wantedPoint, Size newSize)
        {
            //newSize = new Size(816, 489);

            var aspectRatio = (double)wantedPoint.Y / wantedPoint.X;
            var calculatedX = (wantedPoint.X / (double)original.Width) * newSize.Width;

            /*
            return new Point(
               (int)calculatedX,
               (int)(calculatedX * aspectRatio)
               (int)((wantedPoint.Y / (double)original.Height) * newSize.Height)
           );
            */
            return new Point(
                (int)calculatedX,
                (int)(calculatedX * aspectRatio)
            );
        }

        private void ResizeBuffer(int width, int height)
        {
            lock (mutex)
            {
                backBufferGraphics?.Dispose();
                graphics?.Dispose();
                
                backBuffer = new Bitmap(width, height);
                backBufferGraphics = Graphics.FromImage(backBuffer);

                graphics = this.CreateGraphics();
            }
        }
    
        private void DrawDeck(CardType Type)
        {
            var relativeCardSize = calculateRelativeSize(originalSize, cardSize, this.Size);
            var relativeCardPoint = calculateRelativePoint(originalSize, new Point(5, 5), this.Size);

            backBufferGraphics.DrawImage(CardImages[Type], new Rectangle(relativeCardPoint, relativeCardSize));
        }

        private void DrawDeck()
        {
            var relativeCardSize = calculateRelativeSize(originalSize, cardSize, this.Size);
            var relativeCardPoint = calculateRelativePoint(originalSize, new Point(5, 5), this.Size);

            backBufferGraphics.DrawRectangle(new Pen(new SolidBrush(Color.Black)), new Rectangle(
                new Point(relativeCardPoint.X - 1, relativeCardPoint.Y - 1),
                new Size(relativeCardSize.Width + 1, relativeCardSize.Height + 1)));
            backBufferGraphics.FillRectangle(new SolidBrush(Color.White), new Rectangle(relativeCardPoint, relativeCardSize));
        }


        private void DrawColorStacks(CardModel[] Cards)
        {
            for (int i = 0; i < 4; i++)
            {
                var cardPoint = new Point(450 + i * (cardSize.Width + 15), 5);

                var relativeCardSize = calculateRelativeSize(originalSize, cardSize, this.Size);
                var relativeCardPoint = calculateRelativePoint(originalSize, cardPoint, this.Size);

                if (Cards != default && Cards[i] != default)
                {
                    backBufferGraphics.DrawImage(
                       CardImages[Cards[i].Type],
                       new Rectangle(relativeCardPoint, relativeCardSize));
                }
                else
                {
                    backBufferGraphics.DrawRectangle(new Pen(new SolidBrush(Color.Black)), new Rectangle(
                        new Point(relativeCardPoint.X - 1, relativeCardPoint.Y - 1),
                        new Size(relativeCardSize.Width + 1, relativeCardSize.Height + 1)));

                    backBufferGraphics.FillRectangle(
                        new SolidBrush(Color.White),
                        new Rectangle(relativeCardPoint, relativeCardSize));
                }
            }
        }

        private void DrawStack(CardModel[] Cards)
        {
            for (int i = 0; i < 7; i++)
            {
                var cardPoint = new Point(237 + i * (cardSize.Width + 15), 150);

                var relativeCardSize = calculateRelativeSize(originalSize, cardSize, this.Size);
                var relativeCardPoint = calculateRelativePoint(originalSize, cardPoint, this.Size);

                if (Cards != default && Cards[i] != default)
                {
                    backBufferGraphics.DrawImage(
                       CardImages[Cards[i].Type],
                       new Rectangle(relativeCardPoint, relativeCardSize));
                }
                else
                {
                    if (Logic.AwaitingCard() && Logic.AwaitingCardIndex() == i)
                        continue;

                    backBufferGraphics.DrawRectangle(new Pen(new SolidBrush(Color.Black)), new Rectangle(
                        new Point(relativeCardPoint.X - 1, relativeCardPoint.Y - 1),
                        new Size(relativeCardSize.Width + 1, relativeCardSize.Height + 1)));

                    backBufferGraphics.FillRectangle(
                        new SolidBrush(Color.White),
                        new Rectangle(relativeCardPoint, relativeCardSize));
                }
            }
        }

        private void DrawColorStacks(List<CardModel>[] Cards)
        {
            for (int i = 0; i < 4; i++)
            {
                var cardPoint = new Point(450 + i * (cardSize.Width + 15), 5);

                var relativeCardSize = calculateRelativeSize(originalSize, cardSize, this.Size);
                var relativeCardPoint = calculateRelativePoint(originalSize, cardPoint, this.Size);

                if (Cards.Count() > 0 && Cards[i].Count > 0)
                {
                    backBufferGraphics.DrawImage(
                       CardImages[Cards[i].Last().Type],
                       new Rectangle(relativeCardPoint, relativeCardSize));
                }
                else
                {
                    backBufferGraphics.DrawRectangle(new Pen(new SolidBrush(Color.Black)), new Rectangle(
                        new Point(relativeCardPoint.X - 1, relativeCardPoint.Y - 1),
                        new Size(relativeCardSize.Width + 1, relativeCardSize.Height + 1)));

                    backBufferGraphics.FillRectangle(
                        new SolidBrush(Color.White),
                        new Rectangle(relativeCardPoint, relativeCardSize));
                }
            }
        }
    
        private void DrawStack(List<CardModel>[] Cards) {
            
            for (int i = 0; i < 7; i++)
            {
                var cardPoint = new Point(210 + i * (cardSize.Width + 15), 150);

                var relativeCardSize = calculateRelativeSize(originalSize, cardSize, this.Size);
                var relativeCardPoint = calculateRelativePoint(originalSize, cardPoint, this.Size);

                // Draw each card
                for (int CardIndex = 0; CardIndex < Cards[i].Count; CardIndex++)
                {
                    var Card = Cards[i][CardIndex];

                    if (Card != null)
                    {
                        var drawPoint = new Point(relativeCardPoint.X, relativeCardPoint.Y + 25 * CardIndex);

                        if (Card.Uncovered)
                        {
                            backBufferGraphics.DrawImage(
                               CardImages[Card.Type],
                               new Rectangle(drawPoint, relativeCardSize));
                        } else
                        {
                            backBufferGraphics.DrawImage(
                               BacksideCard,
                               new Rectangle(drawPoint, relativeCardSize));

                            //backBufferGraphics.DrawRectangle(new Pen(new SolidBrush(Color.Black)),
                            //     new Rectangle(drawPoint, relativeCardSize));
                        }
                    }
                }

               

                // No uncovered card..
                // TODO: Rewrite to a more sensible statement
                if (!(Cards.Count() > 0 && Cards[i].Count > 0 && Cards[i].Last() != null && Cards[i].Last().Uncovered))
                {
                    if (Logic.AwaitingCard() && Logic.AwaitingCardIndex() == i)
                    {
                        backBufferGraphics.DrawString("Waiting for new card ... ",
                            this.Font, new SolidBrush(Color.Black), calculateRelativePoint(originalSize, new Point(75, 5), this.Size));
                        continue;
                    }

                    backBufferGraphics.DrawRectangle(new Pen(new SolidBrush(Color.Black)), new Rectangle(
                        new Point(relativeCardPoint.X - 1, relativeCardPoint.Y - 1),
                        new Size(relativeCardSize.Width + 1, relativeCardSize.Height + 1)));

                    backBufferGraphics.FillRectangle(
                        new SolidBrush(Color.White),
                        new Rectangle(relativeCardPoint, relativeCardSize));
                }
            }
        }
    
        private void DrawNextMove()
        {
            backBufferGraphics.DrawString($"Next Move: {Logic.GetNextMove()}", this.Font, new SolidBrush(Color.Black), new Point(5, 300));
        }

        private void btnNextMove_Click(object sender, EventArgs e)
        {
            if (!StateController.Initialized)
            {
                btnNextMove.Text = "Get Next Move";
                StateController.InitializeBoard(BoardController.GetBoard());
                StateController.Initialized = true;
            } else
            {
                StateController.UpdateBoardState(BoardController.GetBoard());
            }

            
        }
    }
}
