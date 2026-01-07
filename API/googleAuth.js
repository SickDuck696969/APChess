const mysql = require('mysql2/promise');

// REMOVE node-fetch (Node 20 already has fetch built in)

const CLIENT_ID = "115960215654-o22j5k56g2mvf26653pjdtb3m00jr0ut.apps.googleusercontent.com";
const CLIENT_SECRET = "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";
const REDIRECT_URI = "http://localhost";

exports.runGoogleAuth = async (req, res) => {
    const code = req.query.code;

    if (!code) {
        res.status(400).send("Missing ?code=");
        return;
    }

    try {
        // 1. Exchange code for tokens
        const tokenResp = await fetch("https://oauth2.googleapis.com/token", {
            method: "POST",
            headers: { "Content-Type": "application/x-www-form-urlencoded" },
            body: new URLSearchParams({
                code,
                client_id: CLIENT_ID,
                client_secret: CLIENT_SECRET,
                redirect_uri: REDIRECT_URI,
                grant_type: "authorization_code"
            })
        });

        const tokenData = await tokenResp.json();
        if (tokenData.error) throw new Error(tokenData.error_description);

        // 2. Get user info
        const userResp = await fetch("https://www.googleapis.com/oauth2/v2/userinfo", {
            headers: { Authorization: `Bearer ${tokenData.access_token}` }
        });

        const userInfo = await userResp.json();

        // 3. Insert or update DB
        await pool.query(
            `INSERT INTO Users (user_id, username, email, password, bday, createwhen, History)
             VALUES (?, ?, ?, ?, NULL, NOW(), 'Google Login')
             ON DUPLICATE KEY UPDATE username = VALUES(username)`,
            [
                userInfo.id,
                userInfo.name,
                userInfo.email,
                "__google__",
            ]
        );

        // 4. Return user info
        res.status(200).json({
            user_id: userInfo.id,
            email: userInfo.email,
            username: userInfo.name
        });

    } catch (err) {
        console.error(err);
        res.status(500).send("Google Login Failed: " + err.message);
    }
};
