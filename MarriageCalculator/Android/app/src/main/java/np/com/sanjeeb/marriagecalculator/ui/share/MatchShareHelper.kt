package np.com.sanjeeb.marriagecalculator.ui.share

import android.content.Context
import android.content.Intent
import android.graphics.*
import android.net.Uri
import android.widget.Toast
import androidx.core.content.FileProvider
import np.com.sanjeeb.marriagecalculator.data.model.Currency
import java.io.File
import java.io.FileOutputStream

data class PlayerShareEntry(
    val name: String,
    val totalMaal: Int,
    val totalScore: Int,
    val totalMoney: Double,
    val rank: Int
)

data class SettlementTransfer(
    val fromPlayer: String,
    val toPlayer: String,
    val amount: Double
)

data class MatchShareData(
    val matchName: String,
    val dateFormatted: String,
    val roundsCount: Int,
    val gamesCount: Int,
    val currency: Currency,
    val standings: List<PlayerShareEntry>,
    val settlements: List<SettlementTransfer>
)

object MatchShareHelper {

    /**
     * Minimizes peer-to-peer cash transactions using a greedy debtor-creditor matching algorithm.
     */
    fun computeSettlements(balances: List<Pair<String, Double>>): List<SettlementTransfer> {
        val debtors = balances.filter { it.second < -0.009 }
            .map { it.first to -it.second }
            .toMutableList()
        val creditors = balances.filter { it.second > 0.009 }
            .map { it.first to it.second }
            .toMutableList()

        debtors.sortByDescending { it.second }
        creditors.sortByDescending { it.second }

        val transfers = mutableListOf<SettlementTransfer>()
        var d = 0
        var c = 0
        while (d < debtors.size && c < creditors.size) {
            val (debtorName, debtAmount) = debtors[d]
            val (creditorName, creditAmount) = creditors[c]
            val settled = minOf(debtAmount, creditAmount)
            if (settled > 0.009) {
                transfers.add(SettlementTransfer(debtorName, creditorName, settled))
            }
            val remainingDebt = debtAmount - settled
            val remainingCredit = creditAmount - settled
            debtors[d] = debtorName to remainingDebt
            creditors[c] = creditorName to remainingCredit
            if (debtors[d].second < 0.009) d++
            if (creditors[c].second < 0.009) c++
        }
        return transfers
    }

    /**
     * Formats match summary as clean plain text with emojis for WhatsApp, Viber, or SMS.
     */
    fun formatMatchSummaryText(data: MatchShareData): String {
        val sb = StringBuilder()
        sb.appendLine("🎴 *Marriage Calculator - Match Results*")
        val roundsLabel = if (data.roundsCount == 1) "1 Round" else "${data.roundsCount} Rounds"
        sb.appendLine("📅 ${data.matchName} · $roundsLabel (${data.gamesCount} games played)")
        sb.appendLine()
        sb.appendLine("🏆 *Final Standings:*")
        data.standings.sortedBy { it.rank }.forEach { entry ->
            val medal = when (entry.rank) {
                1 -> "🥇"
                2 -> "🥈"
                3 -> "🥉"
                else -> "• "
            }
            val sign = if (entry.totalMoney > 0) "+" else ""
            sb.appendLine("$medal ${entry.name}: $sign${data.currency.formatMoney(entry.totalMoney)} (${entry.totalMaal} maal)")
        }

        if (data.settlements.isNotEmpty()) {
            sb.appendLine()
            sb.appendLine("💳 *Settlement (Who Pays Whom):*")
            data.settlements.forEach { transfer ->
                sb.appendLine("• ${transfer.fromPlayer} ➔ ${transfer.toPlayer}: ${data.currency.formatMoney(transfer.amount)}")
            }
        }

        sb.appendLine()
        sb.appendLine("✨ _Calculated with Marriage Calculator_")
        return sb.toString()
    }

    /**
     * Generates a high-resolution 1080px wide card bitmap suitable for sharing.
     */
    fun createMatchShareCardBitmap(data: MatchShareData): Bitmap {
        val width = 1080
        val baseHeight = 360 + (data.standings.size * 90) + (if (data.settlements.isNotEmpty()) 160 + (data.settlements.size * 60) else 60) + 120
        val height = maxOf(1200, baseHeight)

        val bitmap = Bitmap.createBitmap(width, height, Bitmap.Config.ARGB_8888)
        val canvas = Canvas(bitmap)

        // 1. Background gradient (Midnight luxury lounge)
        val bgPaint = Paint().apply {
            shader = LinearGradient(
                0f, 0f, 0f, height.toFloat(),
                Color.parseColor("#0E1626"),
                Color.parseColor("#060A12"),
                Shader.TileMode.CLAMP
            )
        }
        canvas.drawRect(0f, 0f, width.toFloat(), height.toFloat(), bgPaint)

        // 2. Gold border & frame
        val borderPaint = Paint().apply {
            color = Color.parseColor("#D97706")
            style = Paint.Style.STROKE
            strokeWidth = 6f
            isAntiAlias = true
        }
        val innerBorderPaint = Paint().apply {
            color = Color.parseColor("#451A03").apply { Color.argb(80, 245, 158, 11) }
            style = Paint.Style.STROKE
            strokeWidth = 2f
            isAntiAlias = true
        }
        val cardRect = RectF(28f, 28f, width - 28f, height - 28f)
        canvas.drawRoundRect(cardRect, 32f, 32f, borderPaint)
        val innerRect = RectF(40f, 40f, width - 40f, height - 40f)
        canvas.drawRoundRect(innerRect, 24f, 24f, innerBorderPaint)

        // 3. Card suit watermarks in corners
        val watermarkPaint = Paint().apply {
            color = Color.parseColor("#F59E0B")
            alpha = 20
            textSize = 90f
            typeface = Typeface.DEFAULT_BOLD
            isAntiAlias = true
        }
        canvas.drawText("♠ ♥", 65f, 130f, watermarkPaint)
        canvas.drawText("♦ ♣", width - 200f, height - 70f, watermarkPaint)

        // 4. Header title
        val titlePaint = Paint().apply {
            color = Color.parseColor("#F59E0B")
            textSize = 42f
            typeface = Typeface.create(Typeface.SERIF, Typeface.BOLD)
            textAlign = Paint.Align.CENTER
            isAntiAlias = true
        }
        canvas.drawText("MARRIAGE CALCULATOR", width / 2f, 120f, titlePaint)

        // Match Name & Date
        val subtitlePaint = Paint().apply {
            color = Color.WHITE
            textSize = 34f
            typeface = Typeface.DEFAULT_BOLD
            textAlign = Paint.Align.CENTER
            isAntiAlias = true
        }
        canvas.drawText(data.matchName, width / 2f, 175f, subtitlePaint)

        val metaPaint = Paint().apply {
            color = Color.parseColor("#94A3B8")
            textSize = 24f
            textAlign = Paint.Align.CENTER
            isAntiAlias = true
        }
        val roundsText = "${data.roundsCount} ${if (data.roundsCount == 1) "Round" else "Rounds"} · ${data.gamesCount} Games Played"
        canvas.drawText(roundsText, width / 2f, 215f, metaPaint)

        // Header divider
        val dividerPaint = Paint().apply {
            color = Color.parseColor("#334155")
            strokeWidth = 2f
        }
        canvas.drawLine(80f, 250f, width - 80f, 250f, dividerPaint)

        // 5. Standings section
        var currentY = 300f
        val sectionHeaderPaint = Paint().apply {
            color = Color.parseColor("#FBBF24")
            textSize = 26f
            typeface = Typeface.DEFAULT_BOLD
            letterSpacing = 0.08f
            isAntiAlias = true
        }
        canvas.drawText("STANDINGS & EARNINGS", 80f, currentY, sectionHeaderPaint)
        currentY += 40f

        val rowBgPaint = Paint().apply {
            color = Color.parseColor("#152136")
            style = Paint.Style.FILL
            isAntiAlias = true
        }
        val rowBorderPaint = Paint().apply {
            color = Color.parseColor("#334155")
            style = Paint.Style.STROKE
            strokeWidth = 1.5f
            isAntiAlias = true
        }
        val textPrimaryPaint = Paint().apply {
            color = Color.WHITE
            textSize = 30f
            typeface = Typeface.DEFAULT_BOLD
            isAntiAlias = true
        }
        val textSecondaryPaint = Paint().apply {
            color = Color.parseColor("#94A3B8")
            textSize = 24f
            isAntiAlias = true
        }
        val positiveMoneyPaint = Paint().apply {
            color = Color.parseColor("#22C55E")
            textSize = 32f
            typeface = Typeface.DEFAULT_BOLD
            textAlign = Paint.Align.RIGHT
            isAntiAlias = true
        }
        val negativeMoneyPaint = Paint().apply {
            color = Color.parseColor("#F87171")
            textSize = 32f
            typeface = Typeface.DEFAULT_BOLD
            textAlign = Paint.Align.RIGHT
            isAntiAlias = true
        }
        val zeroMoneyPaint = Paint().apply {
            color = Color.parseColor("#94A3B8")
            textSize = 32f
            typeface = Typeface.DEFAULT_BOLD
            textAlign = Paint.Align.RIGHT
            isAntiAlias = true
        }

        data.standings.sortedBy { it.rank }.forEach { entry ->
            val rowRect = RectF(80f, currentY, width - 80f, currentY + 74f)
            canvas.drawRoundRect(rowRect, 14f, 14f, rowBgPaint)
            canvas.drawRoundRect(rowRect, 14f, 14f, rowBorderPaint)

            val medal = when (entry.rank) {
                1 -> "🥇 1st"
                2 -> "🥈 2nd"
                3 -> "🥉 3rd"
                else -> "${entry.rank}th"
            }
            canvas.drawText(medal, 105f, currentY + 46f, textPrimaryPaint)
            canvas.drawText(entry.name, 250f, currentY + 46f, textPrimaryPaint)
            canvas.drawText("${entry.totalMaal} maal", 550f, currentY + 46f, textSecondaryPaint)

            val moneyPaint = when {
                entry.totalMoney > 0.009 -> positiveMoneyPaint
                entry.totalMoney < -0.009 -> negativeMoneyPaint
                else -> zeroMoneyPaint
            }
            val sign = if (entry.totalMoney > 0.009) "+" else ""
            val moneyFormatted = "$sign${data.currency.formatMoney(entry.totalMoney)}"
            canvas.drawText(moneyFormatted, width - 110f, currentY + 47f, moneyPaint)

            currentY += 88f
        }

        // 6. Settlement section
        if (data.settlements.isNotEmpty()) {
            currentY += 20f
            canvas.drawLine(80f, currentY, width - 80f, currentY, dividerPaint)
            currentY += 45f

            canvas.drawText("QUICK SETTLEMENT (WHO PAYS WHOM)", 80f, currentY, sectionHeaderPaint)
            currentY += 35f

            val settleBoxRect = RectF(80f, currentY, width - 80f, currentY + (data.settlements.size * 55f) + 24f)
            val settleBgPaint = Paint().apply {
                color = Color.parseColor("#111A2C")
                style = Paint.Style.FILL
                isAntiAlias = true
            }
            canvas.drawRoundRect(settleBoxRect, 14f, 14f, settleBgPaint)
            canvas.drawRoundRect(settleBoxRect, 14f, 14f, rowBorderPaint)

            val transferFromPaint = Paint().apply {
                color = Color.parseColor("#FCA5A5")
                textSize = 26f
                typeface = Typeface.DEFAULT_BOLD
                isAntiAlias = true
            }
            val transferArrowPaint = Paint().apply {
                color = Color.parseColor("#94A3B8")
                textSize = 24f
                isAntiAlias = true
            }
            val transferToPaint = Paint().apply {
                color = Color.parseColor("#86EFAC")
                textSize = 26f
                typeface = Typeface.DEFAULT_BOLD
                isAntiAlias = true
            }
            val transferAmountPaint = Paint().apply {
                color = Color.parseColor("#FBBF24")
                textSize = 26f
                typeface = Typeface.DEFAULT_BOLD
                textAlign = Paint.Align.RIGHT
                isAntiAlias = true
            }

            var transferY = currentY + 44f
            data.settlements.forEach { transfer ->
                canvas.drawText("• ${transfer.fromPlayer}", 115f, transferY, transferFromPaint)
                canvas.drawText("➔ pays", 340f, transferY, transferArrowPaint)
                canvas.drawText(transfer.toPlayer, 450f, transferY, transferToPaint)
                canvas.drawText(data.currency.formatMoney(transfer.amount), width - 115f, transferY, transferAmountPaint)
                transferY += 54f
            }
        }

        // 7. Footer Branding
        val footerPaint = Paint().apply {
            color = Color.parseColor("#64748B")
            textSize = 22f
            textAlign = Paint.Align.CENTER
            isAntiAlias = true
        }
        canvas.drawText("🎴 Calculated with Marriage Calculator", width / 2f, height - 55f, footerPaint)

        return bitmap
    }

    /**
     * Saves bitmap to app cache and opens the native Android share sheet with image and text caption.
     */
    fun shareMatchSummary(context: Context, data: MatchShareData) {
        val summaryText = formatMatchSummaryText(data)
        try {
            val bitmap = createMatchShareCardBitmap(data)
            val shareDir = File(context.cacheDir, "shared")
            if (!shareDir.exists()) shareDir.mkdirs()

            val imageFile = File(shareDir, "match_results_${System.currentTimeMillis()}.png")
            FileOutputStream(imageFile).use { out ->
                bitmap.compress(Bitmap.CompressFormat.PNG, 100, out)
            }

            val imageUri: Uri = FileProvider.getUriForFile(
                context,
                "${context.packageName}.fileprovider",
                imageFile
            )

            val shareIntent = Intent(Intent.ACTION_SEND).apply {
                type = "image/png"
                putExtra(Intent.EXTRA_STREAM, imageUri)
                putExtra(Intent.EXTRA_TEXT, summaryText)
                clipData = android.content.ClipData.newRawUri("Match Results", imageUri)
                addFlags(Intent.FLAG_GRANT_READ_URI_PERMISSION)
            }

            val chooser = Intent.createChooser(shareIntent, "Share Match Results").apply {
                addFlags(Intent.FLAG_GRANT_READ_URI_PERMISSION)
                addFlags(Intent.FLAG_ACTIVITY_NEW_TASK)
            }
            context.startActivity(chooser)
        } catch (e: Exception) {
            e.printStackTrace()
            // Fallback to text-only share if image generation fails
            val textIntent = Intent(Intent.ACTION_SEND).apply {
                type = "text/plain"
                putExtra(Intent.EXTRA_TEXT, summaryText)
            }
            val chooser = Intent.createChooser(textIntent, "Share Match Results")
            chooser.addFlags(Intent.FLAG_ACTIVITY_NEW_TASK)
            context.startActivity(chooser)
        }
    }
}
