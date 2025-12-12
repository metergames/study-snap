/* Ryan Morov & Felipe Mesa Paredes
 * 2492176 & 2466265
 * Project - Flashcard Study App
 */
namespace StudySnap.Models
{
    /// <summary>
    /// Represents a flashcard containing a question and answer
    /// </summary>
    public class Flashcard
    {
        private string _front;
        private string _back;

        /// <summary>
        /// Front of card (question/prompt). Cannot be empty.
        /// </summary>
        public string Front
        {
            get { return _front; }
            set
            {
                if (string.IsNullOrWhiteSpace(value))
                    throw new ArgumentException("The front of the card cannot be empty.");
                _front = value;
            }
        }

        /// <summary>
        /// Back of card (answer/definition). Cannot be empty.
        /// </summary>
        public string Back
        {
            get { return _back; }
            set
            {
                if (string.IsNullOrWhiteSpace(value))
                    throw new ArgumentException("The front of the card cannot be empty.");
                _back = value;
            }
        }

        /// <summary>
        /// Constructor for flashcard, with front and back content
        /// </summary>
        /// <param name="front">Front contents of flashcard (question)</param>
        /// <param name="back">Rear contents of flashcard (answer)</param>
        public Flashcard(string front, string back)
        {
            Front = front;
            Back = back;
        }
    }
}
