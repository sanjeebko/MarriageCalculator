package com.sanjeeb.marriagecalculator

import com.sanjeeb.marriagecalculator.data.local.*
import com.sanjeeb.marriagecalculator.data.model.Currency
import com.sanjeeb.marriagecalculator.data.model.GameSettings
import com.sanjeeb.marriagecalculator.data.repository.toEntity
import com.sanjeeb.marriagecalculator.data.repository.toDomainModel
import org.junit.Assert.*
import org.junit.Test

class OfflineStorageTest {

    @Test
    fun `PlayerEntity maps to Player correctly`() {
        val entity = PlayerEntity(id = 1, name = "Test Player", email = "test@test.com", isGuest = true)
        val player = entity.toDomainModel()
        assertEquals("1", player.id)
        assertEquals("Test Player", player.name)
        assertEquals("test@test.com", player.email)
    }

    @Test
    fun `GameSettings converts to entity and back`() {
        val settings = GameSettings(
            murder = true,
            kidnap = false,
            seenPoint = 3,
            unseenPoint = 10,
            pointRate = 10.0,
            currency = Currency.NPR_Rupee,
            dublee = true,
            dubleePointLess = true
        )
        val entity = settings.toEntity()
        val restored = entity.toDomainModel()

        assertEquals(settings.murder, restored.murder)
        assertEquals(settings.kidnap, restored.kidnap)
        assertEquals(settings.seenPoint, restored.seenPoint)
        assertEquals(settings.unseenPoint, restored.unseenPoint)
        assertEquals(settings.pointRate, restored.pointRate, 0.01)
        assertEquals(settings.currency, restored.currency)
        assertEquals(settings.dublee, restored.dublee)
    }

    @Test
    fun `GameSettings entity preserves currency ordinal`() {
        val settings = GameSettings(currency = Currency.GBP_Pence)
        val entity = settings.toEntity()
        assertEquals(2, entity.currency)
    }

    @Test
    fun `RoundScoreEntity captures all fields`() {
        val score = RoundScoreEntity(
            roundId = 1,
            playerId = 2,
            score = -10,
            maal = 5,
            isSeen = true,
            isWinner = false,
            isDublee = false
        )
        assertEquals(1, score.roundId)
        assertEquals(2, score.playerId)
        assertEquals(-10, score.score)
        assertEquals(5, score.maal)
        assertTrue(score.isSeen)
        assertFalse(score.isWinner)
    }

    @Test
    fun `GameSetEntity tracks sync status`() {
        val gameSet = GameSetEntity(id = 1, settingsId = 1, synced = false, remoteId = null)
        assertFalse(gameSet.synced)
        assertNull(gameSet.remoteId)

        val synced = gameSet.copy(synced = true, remoteId = "42")
        assertTrue(synced.synced)
        assertEquals("42", synced.remoteId)
    }

    @Test
    fun `RoundEntity tracks sync status`() {
        val round = RoundEntity(gameSetId = 1, roundNumber = 1, winnerId = 2, synced = false)
        assertFalse(round.synced)

        val synced = round.copy(synced = true, remoteId = "99")
        assertTrue(synced.synced)
        assertEquals("99", synced.remoteId)
    }
}
