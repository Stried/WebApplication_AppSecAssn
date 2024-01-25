Assignment Project:
Registration Form (4%)
- Sucessfully saving member info into Database (Done)
- Check for duplicate email and rectify issue (Done)

Securing Credentials
Set Strong Password (10%)
- Perform password complexity checks (Min 12 chars, use combination of lower/upper case, numbers and special characters) (Done)
- Offer feedback to user on strong password (Done)
- Implement both Client-based and Server-based checks.

Securing User Data and Passwords (6%)
- Implement Password Protection
- Encryption of customer data (Encrypt in database)
- Decryption of customer data (Display in homepage)

Session Management (10%)
- Create a Secured Session upon successful login
- Perform session timeout
- Route to homepage/login page after session timeout.
- Detect multiple logins from different devices

Credential Verfication (10%)
- Able to login to system after registration
- Rate limiting (Account lockout after 3 login failures)
- Perform proper and safe logout (Clear session and redirect to login page)
- Perform audit log (save user activities in database)
- Redirect to homepage after credential verification. Home page displays the user info including encrypted data

Anti-bot (5%)
- IMplement Google reCaptcha v3 service

Proper Input Validation  (15%)
- Prevent injection, CSRF and XSS attack
- Perform proper input sanitation, validation and verification
- Client and server input validation
- Display error or warning message on improper input requirements
- Perform proper encoding before saving into database.