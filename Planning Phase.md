## Phase 2: Project Planning

**Team: ** Ryan Morov & Felipe Mesa Paredes

**Project:** Flashcard Study App

#### 1. Understanding the Problem

Students often struggle to organize and study large amounts of information effectively using physical notes or index cards, which are easily lost or damaged. The problem is the lack of a simple, digital tool that allows students to create custom decks of questions and answers, test themselves interactively and track which cards they know and which they miss. The solution requires an application that saves this data so students can pick up their study sessions exactly where they left off without losing progress. This ensures steady learning, refined for each user.

#### 2. Formulating the Problem

To solve this, we will build a C# WPF Desktop Application. The problem consists of three key parts:

- **Input:** The user creates Decks and adds Flashcards (Front/Back text) to them.
- **Processing:** The app manages these lists, saves them to a local file and randomizes the cards for a Quiz Mode. It also calculates a simple score based on user self-assessment.
- **Output:** The app visually displays the cards one by one, reveals the answer on command and provides a summary of the study session results.

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