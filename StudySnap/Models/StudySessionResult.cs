/* Ryan Morov & Felipe Mesa Paredes
 * 2492176 & 2466265
 * Project - Flashcard Study App
 */
namespace StudySnap.Models
{
    /// <summary>
    /// Statistics for a completed study session.
    /// </summary>
    public class StudySessionResult
    {
        private string _deckName;
        private DateTime _date;
        private double _score;
        private int _correctCount;
        private int _totalCards;

        /// <summary>
        /// Name of deck that was studied.
        /// </summary>
        public string DeckName
        {
            get { return _deckName; }
            set
            {
                if (string.IsNullOrWhiteSpace(value))
                    throw new ArgumentException("Deck name cannot be empty.");
                _deckName = value;
            }
        }

        /// <summary>
        /// Date & time when the study session was completed.
        /// </summary>
        public DateTime Date
        {
            get { return _date; }
            set { _date = value; }
        }

        /// <summary>
        /// Final calculated score as a percentage (0-100).
        /// </summary>
        public double Score
        {
            get { return _score; }
            set
            {
                if (value < 0 || value > 100)
                    throw new ArgumentException("Score must be between 0 and 100.");
                _score = value;
            }
        }

        /// <summary>
        /// Number of cards answered correctly.
        /// </summary>
        public int CorrectCount
        {
            get { return _correctCount; }
            set
            {
                if (value < 0)
                    throw new ArgumentException("Correct count cannot be negative.");
                _correctCount = value;
            }
        }

        /// <summary>
        /// Total number of cards attempted in the session (as session can be terminated early).
        /// </summary>
        public int TotalCards
        {
            get { return _totalCards; }
            set
            {
                if (value < 0)
                    throw new ArgumentException("Total cards cannot be negative.");
                _totalCards = value;
            }
        }

        public StudySessionResult(string deckName, double score, int correctCount, int totalCards, DateTime date)
        {
            DeckName = deckName;
            Score = score;
            CorrectCount = correctCount;
            TotalCards = totalCards;
            Date = date;
        }
    }
}
