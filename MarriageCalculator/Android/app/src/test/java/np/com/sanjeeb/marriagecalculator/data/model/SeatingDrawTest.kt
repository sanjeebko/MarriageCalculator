package np.com.sanjeeb.marriagecalculator.data.model

import org.junit.Assert.*
import org.junit.Test
import kotlin.random.Random

class SeatingDrawTest {

    private fun players(count: Int): List<Player> =
        (1..count).map { Player(id = it.toString(), name = "Player $it") }

    @Test
    fun `full deck has 52 unique cards`() {
        val deck = SeatingDraw.fullDeck()
        assertEquals(52, deck.size)
        assertEquals(52, deck.toSet().size)
    }

    @Test
    fun `draw assigns a distinct card to every player`() {
        val result = SeatingDraw.draw(players(6), Random(42))
        assertEquals(6, result.cards.size)
        assertEquals(6, result.cards.values.toSet().size) // all distinct
    }

    @Test
    fun `seating is ordered highest card first`() {
        val result = SeatingDraw.draw(players(6), Random(7))
        val orderedCards = result.seating.map { result.cards.getValue(it.id) }
        for (i in 0 until orderedCards.size - 1) {
            assertTrue(
                "seat $i (${orderedCards[i].label}) should outrank seat ${i + 1} (${orderedCards[i + 1].label})",
                orderedCards[i] > orderedCards[i + 1]
            )
        }
    }

    @Test
    fun `lowest card holder is first dealer`() {
        val result = SeatingDraw.draw(players(4), Random(99))
        val dealer = result.firstDealer
        assertNotNull(dealer)
        val dealerCard = result.cards.getValue(dealer!!.id)
        val minCard = result.cards.values.min()
        assertEquals(minCard, dealerCard)
    }

    @Test
    fun `seating preserves all original players`() {
        val original = players(5)
        val result = SeatingDraw.draw(original, Random(1))
        assertEquals(original.toSet(), result.seating.toSet())
    }

    @Test
    fun `works for minimum two players`() {
        val result = SeatingDraw.draw(players(2), Random(3))
        assertEquals(2, result.seating.size)
        assertTrue(
            result.cards.getValue(result.seating[0].id) >
                result.cards.getValue(result.seating[1].id)
        )
    }

    @Test
    fun `single player is both first seat and dealer`() {
        val result = SeatingDraw.draw(players(1), Random(5))
        assertEquals("1", result.seating.single().id)
        assertEquals("1", result.firstDealer?.id)
    }

    @Test(expected = IllegalArgumentException::class)
    fun `empty player list throws`() {
        SeatingDraw.draw(emptyList())
    }

    @Test
    fun `card comparison is by rank then suit precedence`() {
        val aceClubs = SeatingDraw.PlayingCard(14, SeatingDraw.Suit.CLUBS)
        val kingSpades = SeatingDraw.PlayingCard(13, SeatingDraw.Suit.SPADES)
        val kingHearts = SeatingDraw.PlayingCard(13, SeatingDraw.Suit.HEARTS)

        assertTrue(aceClubs > kingSpades) // rank wins
        assertTrue(kingSpades > kingHearts) // suit breaks tie
    }

    @Test
    fun `card labels are human readable`() {
        assertEquals("A♠", SeatingDraw.PlayingCard(14, SeatingDraw.Suit.SPADES).label)
        assertEquals("10♥", SeatingDraw.PlayingCard(10, SeatingDraw.Suit.HEARTS).label)
        assertEquals("J♦", SeatingDraw.PlayingCard(11, SeatingDraw.Suit.DIAMONDS).label)
        assertEquals("Q♣", SeatingDraw.PlayingCard(12, SeatingDraw.Suit.CLUBS).label)
        assertEquals("K♠", SeatingDraw.PlayingCard(13, SeatingDraw.Suit.SPADES).label)
    }

    @Test(expected = IllegalArgumentException::class)
    fun `invalid rank throws`() {
        SeatingDraw.PlayingCard(15, SeatingDraw.Suit.SPADES)
    }

    @Test
    fun `draw is deterministic for a given seed`() {
        val a = SeatingDraw.draw(players(6), Random(123))
        val b = SeatingDraw.draw(players(6), Random(123))
        assertEquals(a.seating.map { it.id }, b.seating.map { it.id })
        assertEquals(a.cards, b.cards)
    }
}
