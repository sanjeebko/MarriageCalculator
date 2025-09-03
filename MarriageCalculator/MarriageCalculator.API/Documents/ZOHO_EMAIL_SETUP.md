# Zoho Email Configuration Guide for Marriage Calculator API

## Current Configuration ?

Based on your successful configuration check, you have:
- **SMTP Server**: `smtp.zoho.eu` ?
- **From Email**: `noreply@sanjeebojha.com.np` ?  
- **Password**: Configured ?

## Zoho-Specific Requirements

### **1. Enable IMAP/POP Access**
1. Log into your Zoho Mail account
2. Go to **Settings** ? **Mail** ? **POP/IMAP Access**
3. **Enable IMAP Access** (required for SMTP authentication)
4. Save the settings

### **2. Two-Factor Authentication (2FA)**
If you have 2FA enabled on your Zoho account:
1. Go to **Zoho Accounts** ? **Security** ? **App Passwords**
2. Generate a new **App Password** for "Mail"
3. Use this App Password instead of your regular password
4. Set `MCMAILPASSWORD=your-app-password`

### **3. SMTP Settings for Zoho EU**
```sh
MCSMTP=smtp.zoho.eu
MCMAILUSERNAME=noreply@sanjeebojha.com.np
MCMAILPASSWORD=your-password-or-app-password
```

## Enhanced EmailService Features ?

I've upgraded your EmailService with the following improvements:

### **?? Performance Enhancements**
- **Connection timing** - Logs setup and send times separately
- **Optimal SMTP settings** - Automatically detects best port/SSL for Zoho
- **30-second timeout** - Prevents hanging connections
- **Better error handling** - Specific guidance for different error types

### **?? Enhanced Logging**
- **Detailed timing information** - See exactly where delays occur
- **Network error codes** - Specific guidance for socket errors
- **SMTP status codes** - Targeted advice for SMTP failures
- **Server-specific guidance** - Zoho-optimized error messages

### **?? Better Email Headers**
- **X-Mailer header** - Identifies your application
- **Priority settings** - Normal priority for better deliverability
- **Proper encoding** - Better compatibility across email clients

## Testing Your Configuration

### **Step 1: Test SMTP Providers**
```
GET https://localhost:7294/api/EmailTest/test-smtp-providers
```
This will check if `smtp.zoho.eu` is accessible from your network.

### **Step 2: Test Your Specific Connection**
```
GET https://localhost:7294/api/EmailTest/test-smtp-connection
```

### **Step 3: Send Test Email**
```
POST https://localhost:7294/api/EmailTest/send-verification
{
  "email": "your-test-email@example.com",
  "displayName": "Test User"
}
```

## Common Zoho Issues & Solutions

### **Issue 1: "Resource temporarily unavailable"**
**Solution:**
- Check if your firewall/ISP blocks port 587
- Try using a VPN or different network
- Verify Zoho service status

### **Issue 2: Authentication Failed**
**Solutions:**
1. **Enable IMAP Access** in Zoho Mail settings
2. **Generate App Password** if 2FA is enabled
3. **Check username format** - use full email address
4. **Verify password** - no typos or special characters issues

### **Issue 3: Rate Limiting**
**Solutions:**
- Zoho has sending limits (check your plan)
- Implement retry logic with delays
- Use dedicated SMTP service for high volume

### **Issue 4: IP Blocking**
**Solutions:**
- Check if your server IP is blacklisted
- Contact Zoho support for IP whitelisting
- Use authenticated SMTP (which you are)

## Advanced Troubleshooting

### **Check Zoho Server Status**
1. Visit Zoho Status page: `https://status.zoho.com/`
2. Check for any EU region issues
3. Verify SMTP service availability

### **Network Diagnostics**
```bash
# Test port connectivity
telnet smtp.zoho.eu 587

# Check DNS resolution
nslookup smtp.zoho.eu

# Test with openssl
openssl s_client -connect smtp.zoho.eu:587 -starttls smtp
```

### **Log Analysis**
Look for these patterns in your log files:

**? Success Pattern:**
```
[INFO] Sending email - To: test@example.com, Subject: Test, SMTP: smtp.zoho.eu
[DEBUG] Using SMTP settings - Port: 587, SSL: True
[INFO] Email sent successfully to test@example.com in 2500ms (Setup: 150ms, Send: 2350ms)
```

**? Error Patterns:**
```
[ERROR] Network connectivity error sending email after 4000ms. Error Code: 11
[WARN] Network resource temporarily unavailable. Try different network or VPN.
```

## Performance Expectations

### **Typical Zoho Response Times:**
- **Setup Time**: 100-300ms
- **Send Time**: 1500-4000ms
- **Total Time**: 1600-4300ms

### **When to Be Concerned:**
- Setup > 1000ms (DNS/network issues)
- Send > 10000ms (SMTP server issues)
- Frequent timeouts (connectivity problems)

## Production Recommendations

### **For High Volume:**
1. **Consider Zoho TransMail** - Dedicated transactional email service
2. **Implement retry logic** - Handle temporary failures
3. **Queue emails** - Don't block API responses
4. **Monitor limits** - Track daily/hourly sending quotas

### **For Better Deliverability:**
1. **Set up SPF record** for your domain
2. **Configure DKIM** in Zoho Mail settings
3. **Add DMARC policy** for your domain
4. **Monitor reputation** using Zoho's tools

## Testing Commands

### **Quick Test Sequence:**
```bash
# 1. Check configuration
curl -X GET "https://localhost:7294/api/EmailTest/config-status"

# 2. Test connectivity
curl -X GET "https://localhost:7294/api/EmailTest/test-smtp-connection"

# 3. Send test email
curl -X POST "https://localhost:7294/api/EmailTest/send-verification" \
     -H "Content-Type: application/json" \
     -d '{"email":"sanjeeb@live.com","displayName":"Test User"}'
```

---

**Your Zoho configuration looks excellent! The enhanced EmailService should provide much better error handling and debugging information for any connectivity issues.** ???