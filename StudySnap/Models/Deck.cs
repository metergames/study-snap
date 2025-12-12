/* Ryan Morov & Felipe Mesa Paredes
 * 2492176 & 2466265
 * Project - Flashcard Study App
 */
namespace StudySnap.Models
{
    /// <summary>
    /// Collection of flashcards, grouped.
    /// </summary>
    public class Deck
    {
        private string _name;
        private List<Flashcard> _cards;

        /// <summary>
        /// Name of the deck. Cannot be empty.
        /// </summary>
        public string Name
        {
            get { return _name; }
            set
            {
                if (string.IsNullOrWhiteSpace(value))
                    throw new ArgumentException("Deck name cannot be empty.");
                _name = value;
            }
        }

        /// <summary>
        /// List of flashcards contained in deck.
        /// Setting property to null will replace it with an empty list, so deck never stores null values.
        /// </summary>
        public List<Flashcard> Cards
        {
            get { return _cards; }
            set
            {
                if (value == null)
                    _cards = new List<Flashcard>();
                else _cards = value;
            }
        }

        /// <summary>
        /// Initializes a new deck with the specified name and an empty flashcard list.
        /// The name is required and the deck is created ready for editing and adding flashcards.
        /// </summary>
        /// <param name="name">Deck name</param>
        public Deck(string name)
        {
            Name = name;
            Cards = new List<Flashcard>();
        }

        /// <summary>
        /// Adds a new flashcard to the deck.
        /// </summary>
        /// <param name="card">Flashcard object to add</param>
        /// <exception cref="ArgumentException">If card to add is null</exception>
        public void AddCard(Flashcard card)
        {
            if (card == null)
                throw new ArgumentException("Cannot add a null card to the deck.");
            Cards.Add(card);
        }

        /// <summary>
        /// Removes a flashcard from the deck.
        /// </summary>
        /// <param name="card">Flashcard object to remove</param>
        public void RemoveCard(Flashcard card)
        {
            if (card != null)
                Cards.Remove(card);
        }

        /// <summary>
        /// Returns total number of cards in the deck.
        /// </summary>
        /// <returns>Integer for count of cards</returns>
        public int GetCardCount()
        {
            return Cards.Count;
        }
    }
}
