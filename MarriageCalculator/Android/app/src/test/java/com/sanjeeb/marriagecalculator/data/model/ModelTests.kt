package com.sanjeeb.marriagecalculator.data.model

import org.junit.Assert.*
import org.junit.Test

class PlayerTest {
    @Test
    fun `default player has empty fields`() {
        val player = Player()
        assertEquals("", player.id)
        assertEquals("", player.name)
        assertEquals("", player.email)
        assertFalse(player.deleted)
        assertFalse(player.selected)
    }

    @Test
    fun `create player with values`() {
        val player = Player(id = "1", name = "Ram", email = "ram@test.com", selected = true)
        assertEquals("1", player.id)
        assertEquals("Ram", player.name)
        assertEquals("ram@test.com", player.email)
        assertTrue(player.selected)
    }

    @Test
    fun `create player request has correct fields`() {
        val request = CreatePlayerRequest(name = "Shyam", email = "shyam@test.com")
        assertEquals("Shyam", request.name)
        assertEquals("shyam@test.com", request.email)
    }
}

class GameSettingsTest {
    @Test
    fun `default settings match C# defaults`() {
        val settings = GameSettings.default()
        assertTrue(settings.murder)
        assertFalse(settings.kidnap)
        assertEquals(3, settings.seenPoint)
        assertEquals(10, settings.unseenPoint)
        assertEquals(10.0, settings.pointRate, 0.01)
        assertEquals(Currency.NPR_Rupee, settings.currency)
        assertTrue(settings.dublee)
        assertTrue(settings.dubleePointLess)
        assertEquals(0, settings.dubleePointBonus)
        assertEquals(15, settings.foulPoint)
        assertEquals(FoulPointBonusType.NEXT_GAME, settings.foulPointBonus)
        assertTrue(settings.audio)
    }

    @Test
    fun `currency display names are correct`() {
        assertEquals("NPR (₨)", Currency.NPR_Rupee.displayName())
        assertEquals("INR (₹)", Currency.INR_Rupee.displayName())
        assertEquals("GBP (p)", Currency.GBP_Pence.displayName())
        assertEquals("USD (¢)", Currency.USD_Cent.displayName())
        assertEquals("AUD (¢)", Currency.AUD_Cent.displayName())
    }

    @Test
    fun `foul point bonus type display names`() {
        assertEquals("Next Game", FoulPointBonusType.NEXT_GAME.displayName())
        assertEquals("Current Game", FoulPointBonusType.CURRENT_GAME.displayName())
    }
}

class MarriageGameSetTest {
    @Test
    fun `default game set is active`() {
        val gameSet = MarriageGameSet()
        assertTrue(gameSet.isActive)
        assertEquals("", gameSet.id)
    }
}

class MarriageGameScoreTest {
    @Test
    fun `default score is zeroed`() {
        val score = MarriageGameScore()
        assertEquals(0, score.score)
        assertEquals(0.0, score.moneyWon, 0.01)
        assertFalse(score.seen)
        assertFalse(score.winner)
        assertFalse(score.duply)
        assertEquals(0, score.maal)
    }

    @Test
    fun `score with values`() {
        val score = MarriageGameScore(
            playerId = "1",
            seen = true,
            maal = 15,
            winner = true,
            score = 45,
            moneyWon = 450.0
        )
        assertTrue(score.seen)
        assertEquals(15, score.maal)
        assertTrue(score.winner)
        assertEquals(45, score.score)
        assertEquals(450.0, score.moneyWon, 0.01)
    }
}

class MarriageGameTest {
    @Test
    fun `default game has no scores`() {
        val game = MarriageGame()
        assertNull(game.marriageGameScores)
        assertFalse(game.closedRound)
    }
}

class MarriageGameRoundTest {
    @Test
    fun `default round is not completed`() {
        val round = MarriageGameRound()
        assertFalse(round.completed)
        assertNull(round.marriageGames)
    }
}
