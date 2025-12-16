/* Ryan Morov & Felipe Mesa Paredes
 * 2492176 & 2466265
 * Project - Flashcard Study App
 */
namespace StudySnap.Models
{
    /// <summary>
    /// Represents an interactive study session for reviewing flashcards from a specific deck.
    /// </summary>
    public class StudySession
    {
        private Deck _deck;
        private int _currentCardIndex;
        private int _correctCount;

        /// <summary>
        /// Gets the current deck in use.
        /// </summary>
        public Deck CurrentDeck
        {
            get { return _deck; }
            private set { _deck = value; }
        }

        /// <summary>
        /// Gets the index of the currently selected card.
        /// </summary>
        public int CurrentCardIndex
        {
            get { return _currentCardIndex; }
            private set { _currentCardIndex = value; }
        }

        /// <summary>
        /// Gets the number of correct answers recorded.
        /// </summary>
        public int CorrectCount
        {
            get { return _correctCount; }
            private set { _correctCount = value; }
        }

        /// <summary>
        /// Gets the total number of cards currently in the deck.
        /// </summary>
        public int TotalCards
        {
            get { return _deck.Cards.Count; }
        }

        public StudySession (Deck deck)
        {
            if (deck == null)
            {
                throw new ArgumentException("Study session requires a valid deck.");
            }
            CurrentDeck = deck;
            CurrentCardIndex = 0;
            CorrectCount = 0;
            ShuffleDeck();
        }

        /// <summary>
        /// Retrieves the next available flashcard in the current deck or null if there are no more cards to review.
        /// </summary>
        /// <returns>The next Flashcard in the current deck to be reviewed or null if all cards have been reviewed.</returns>
        public Flashcard GetNextCard()
        {
            if (HasMoreCards())
            {
                return CurrentDeck.Cards[CurrentCardIndex];
            }
            return null;
        }

        /// <summary>
        /// Determines whether there are additional cards remaining in the collection.
        /// </summary>
        /// <returns>Returns true if there are more cards available to access or false if not.</returns>
        public bool HasMoreCards()
        {
            return CurrentCardIndex < TotalCards;
        }

        /// <summary>
        /// Records the user's answer for the current card and advances to the next card.
        /// </summary>
        /// <param name="isCorrect">true if the user's answer to the current card is correct or false if not.</param>
        public void RecordAnswer(bool isCorrect)
        {
            if (isCorrect)
            {
                CorrectCount++;
            }
            CurrentCardIndex++;
        }

        /// <summary>
        /// Calculates the percentage score based on the number of correct answers and the total number of cards.
        /// </summary>
        /// <returns>Returns a double representing the percentage of correct answers or returns 0 if the TotalCards is 0.</returns>
        public double CalculateScore()
        {
            if (TotalCards == 0) return 0;
            return (double)CorrectCount / TotalCards * 100;
        }

        /// <summary>
        /// Randomizes the order of the cards in the current deck.
        /// </summary>
        public void ShuffleDeck()
        {
            if(CurrentDeck.Cards == null || CurrentDeck.Cards.Count <= 1)
            {
                return;
            }
            Random r = new Random();
            int cardCount = CurrentDeck.Cards.Count;
            while (cardCount > 1)
            {
                cardCount--;
                int newIndex = r.Next(cardCount + 1);
                Flashcard temp = CurrentDeck.Cards[newIndex];
                CurrentDeck.Cards[newIndex] = CurrentDeck.Cards[cardCount];
                CurrentDeck.Cards[cardCount] = temp;
            }
        }

        /// <summary>
        /// Creates a new StudySessionResult instance representing the outcome of the current study session.
        /// </summary>
        /// <returns>A StudySessionResult containing the deck name, calculated score, number of correct answers,
        /// total number of cards, and the current date and time.</returns>
        public StudySessionResult CreateSessionResult()
        {
            return new StudySessionResult (CurrentDeck.Name, CalculateScore(), CorrectCount, TotalCards, DateTime.Now);
        }
    }
}
