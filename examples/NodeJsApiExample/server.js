const express = require('express');
const axios = require('axios');
const cors = require('cors');

const app = express();
app.use(cors());
app.use(express.json());

const IDENTITY_SERVER = process.env.IDENTITY_SERVER || 'https://localhost:5000';
const INTROSPECT_URL = `${IDENTITY_SERVER}/connect/introspect`;

/**
 * Middleware to validate access tokens via introspection.
 * If the token has been revoked (admin logged user out remotely),
 * returns 401 Unauthorized.
 */
async function validateToken(req, res, next) {
    const authHeader = req.headers.authorization;
    if (!authHeader?.startsWith('Bearer ')) {
        return res.status(401).json({ error: 'No token provided' });
    }

    const token = authHeader.substring(7);

    try {
        const response = await axios.post(INTROSPECT_URL,
            new URLSearchParams({
                token: token,
                token_type_hint: 'access_token'
            }),
            {
                headers: { 'Content-Type': 'application/x-www-form-urlencoded' }
            }
        );

        if (!response.data.active) {
            console.log('Token revoked or expired');
            return res.status(401).json({ error: 'Token revoked' });
        }

        // Token is valid, attach user info to request
        req.user = {
            subject: response.data.sub,
            clientId: response.data.client_id
        };
        next();
    } catch (error) {
        console.error('Introspection failed:', error.message);
        return res.status(401).json({ error: 'Invalid token' });
    }
}

// Protected route example
app.get('/api/protected', validateToken, (req, res) => {
    res.json({
        message: 'This is a protected resource',
        user: req.user
    });
});

// Health check (no auth required)
app.get('/api/health', (req, res) => {
    res.json({ status: 'ok' });
});

const PORT = process.env.PORT || 3000;
app.listen(PORT, () => {
    console.log(`API running on port ${PORT}`);
    console.log(`Identity Server: ${IDENTITY_SERVER}`);
});
