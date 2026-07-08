package np.com.sanjeeb.marriagecalculator.data.model

import kotlin.random.Random

/**
 * Card-draw based seating arrangement per requirement §2.2:
 * every player draws a card; the highest card takes the 1st seat,
 * the rest follow in descending order, and the lowest card holder
 * sits last and deals the first game.
 */
object SeatingDraw {

    /** Suit precedence for tie-breaking equal ranks (higher ordinal wins). */
    enum class Suit(val symbol: String, val precedence: Int) {
        CLUBS("♣", 0),
        DIAMONDS("♦", 1),
        HEARTS("♥", 2),
        SPADES("♠", 3)
    }

    /** rank: 2..14 where 11=J, 12=Q, 13=K, 14=A (Ace high). */
    data class PlayingCard(val rank: Int, val suit: Suit) : Comparable<PlayingCard> {
        init {
            require(rank in 2..14) { "rank must be 2..14" }
        }

        val rankLabel: String
            get() = when (rank) {
                11 -> "J"
                12 -> "Q"
                13 -> "K"
                14 -> "A"
                else -> rank.toString()
            }

        val label: String get() = "$rankLabel${suit.symbol}"

        override fun compareTo(other: PlayingCard): Int =
            compareValuesBy(this, other, { it.rank }, { it.suit.precedence })
    }

    data class DrawResult(
        /** Players in final seat order: index 0 = 1st seat, last = first dealer. */
        val seating: List<Player>,
        /** Card each player drew, keyed by player id. */
        val cards: Map<String, PlayingCard>
    ) {
        val firstDealer: Player? get() = seating.lastOrNull()
    }

    /** Full 52-card deck (single deck so every draw is unique). */
    fun fullDeck(): List<PlayingCard> =
        Suit.entries.flatMap { suit -> (2..14).map { rank -> PlayingCard(rank, suit) } }

    /**
     * Draws one distinct card per player and returns the resulting seating order
     * (highest card first, lowest card last = first dealer).
     * Supports up to 52 players; game rules cap at 6.
     */
    fun draw(players: List<Player>, random: Random = Random.Default): DrawResult {
        require(players.isNotEmpty()) { "players must not be empty" }
        require(players.size <= 52) { "cannot draw for more than 52 players" }

        val drawnCards = fullDeck().shuffled(random).take(players.size)
        val cardsByPlayer = players.zip(drawnCards).associate { (player, card) -> player.id to card }

        val seating = players.sortedByDescending { cardsByPlayer.getValue(it.id) }
        return DrawResult(seating = seating, cards = cardsByPlayer)
    }
}
