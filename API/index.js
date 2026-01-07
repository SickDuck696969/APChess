"use strict";

const { OAuth2Client } = require("google-auth-library");
const mysql = require("mysql2/promise");

/* ------------------------------- */
/*        GOOGLE CONFIG            */
/* ------------------------------- */

const GOOGLE_CLIENT_ID =
    "115960215654-o22j5k56g2mvf26653pjdtb3m00jr0ut.apps.googleusercontent.com";

const googleClient = new OAuth2Client(GOOGLE_CLIENT_ID);

/* ------------------------------- */
/*        MYSQL CONNECTION         */
/* ------------------------------- */

let pool;

function getPool() {
    if (!pool) {
        pool = mysql.createPool({
            host: "34.150.111.71",
            port: 3306,
            user: "free-trial-first-projecta",
            password: "ChessDB_2024_Strong!",
            database: "APChess",
            waitForConnections: true,
            connectionLimit: 5,
        });
    }
    return pool;
}

/* ------------------------------- */
/*        GOOGLE LOGIN API         */
/* ------------------------------- */
/*
POST /googleLogin
Body:
{
  "id_token": "eyJhbGciOiJSUzI1NiIs..."
}
*/

exports.googleLogin = async (req, res) => {
    try {
        const { id_token } = req.body || {};

        if (!id_token) {
            res.status(400).json({ error: "Missing id_token" });
            return;
        }

        // 1️⃣ Verify Google ID token
        const ticket = await googleClient.verifyIdToken({
            idToken: id_token,
            audience: GOOGLE_CLIENT_ID,
        });

        const payload = ticket.getPayload();

        const googleId = payload.sub;           // stable Google user ID
        const email = payload.email;
        const name =
            payload.name ||
            email?.split("@")[0] ||
            "GoogleUser";

        // 2️⃣ Insert or update user
        await getPool().query(
            `
            INSERT INTO Users (user_id, username, email, password, createwhen, History)
            VALUES (?, ?, ?, '__google__', NOW(), 'Google Login')
            ON DUPLICATE KEY UPDATE
                username = VALUES(username),
                email = VALUES(email)
            `,
            [googleId, name, email]
        );

        // 3️⃣ Return user info
        res.status(200).json({
            user_id: googleId,
            username: name,
            email: email,
        });

    } catch (err) {
        console.error("Google login failed:", err);
        res.status(401).json({ error: "Invalid Google ID token" });
    }
};
