## Phase 2: Project Planning

**Team: ** Ryan Morov & Felipe Mesa Paredes

**Project:** Flashcard Study App

#### 1. Understanding the Problem

The application will represent a study tool based on the concept of flashcards. It functions as a self-assessment system where users organize study material into specific subjects. The core mechanism involves presenting a question side of a card, allowing the user to recall the answer, and then revealing the answer side for verification. The system tracks the user's self-reported performance (correct or incorrect) during a session to provide a final score percentage, mimicking a real-world quiz environment.

#### 2. Formulating the Problem

The study data will be represented as a collection of Decks, where each Deck contains a list of Flashcard objects holding two string values (Front and Back). User input will be captured to create these lists and modify them. During a study session, the order of cards within a Deck will be randomized using a shuffling algorithm. The application will maintain temporary counters for correct and incorrect answers during the session. Upon completion, the session statistics (score, date) will be calculated and stored persistently alongside the deck data to allow for historical tracking of performance.

#### 3. Development

##### 1. Main Dashboard (MainWindow)

- **Purpose:** The central hub where users see all their decks.
- **Elements:** List of decks, buttons to "Create Deck", "Delete Deck", "Start Study", and "Exit".

##### 2. Deck Editor (DeckManagerWindow)

- **Purpose:** To add or edit cards within a selected deck.
- **Elements:** Input fields for Front/Back text, a list of current cards, and "Save" button.

##### 3. Study/Quiz Mode (StudyWindow)

- **Purpose:** The active learning interface.
- **Elements:** Large display for the card question, a "Reveal Answer" button, and "Correct/Incorrect" buttons to track progress.