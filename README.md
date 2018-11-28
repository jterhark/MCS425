# Motivations
On today's modern Internet, no one server can handle the traffic it can get from the world. Therefore the easiest solution would be to just spin up more instances of the application on another server. Easy enough, but this can cause a couple problems. The one I am focusing on is Authentication (AuthN) and Authorization (AuthZ). Using this tactic means each app will be responsible for authenticating users. This defeats the purpose of spinning up new instances of the app since each will likely make a database call to some common database shared between all active instances. Obviously, we would like to remove this limitation, but how? The answer is a complete separation of AuthN responsibilities from the app in question. That is, have one app dedicated solely to serving requests and one app dedication to authenticating users. Plus, it would be pretty nice if no passwords needed to be stored. This is where JWT and OAuth2 can help. Json Web Tokens is a token that allows users to be authenticated without the need for a database call, since all the information is stored in the token and is signed by whatever server (in this case, the authentication server) that issued the token. OAuth2 is a way of allowing users to sign into your applications by using a third party (or first party if you’re ambitious). For an example, think when you sign into a web app through Google. This entire operation is known as Single Sign On. Having one server be in charge of issuing credentials that allow users to sign into several apps at once. Using Google as an example again, when you sign into Gmail, you are instantly signed into YouTube, Photos, etc. Lets start with a very brief overview of OAuth2.

# OAuth2
First, some terminology. In OAuth2, scopes are defined as what services you are allowed to access by the third party authenticator. For example, if someone where to give permissions for an application to look on their Google Drive, that would be “Drive.Read”. For the actual authentication mechanism, there are several steps.
1. Authentication server redirects to Third Party.
2. User consents to allow us (the devs) to access their account using predefined scopes.
3. The Third Party redirects back to the application with an authorization code.
4. The application requests a refresh token from the authorization code.
5. The application uses that refresh token to get an access token.
6. The application uses the access token to query services by the Third Party, or in my case, just to check who the user is.
7. If the application is using external services, and the access token expires, the application can issue a request to the Third Party for another access token using the refresh token.
8. If the user wishes to logout from one browser, the application simply deletes the access token.
9. If the user wishes to logout from all browsers, the applications makes a request to the Third Party using the refresh token. The Third Party then invalidates all refresh tokens. Note that there will be a delay where some access tokens are still valid.

Please note that this is the Authorization code flow. There are many OAuth2.0 flows, but this is the most widely used one. The access tokens are stateful, meaning they are stored in a cache somewhere so when the application asks for a new access token, the user does not need to sign in again. The access tokens can be stateful, but for our purposes, let’s make them stateless. I.E. They are not stored anywhere and are validated by each individual service upon a request. Thus no actual database call is needed and all instances of this service can run independently of each other. I am not going to go into to much detail on the access tokens, as they can really be any string that is cryptographically secure. For the access tokens to be stateless, they must contain the info needed to make sure the requester is actually logged in. This is where JWT’s come into play.

# JWT 
Again, let’s start with the terminology.
-	Claim: Some information about a user (ie. I claim that my name is Jake TerHark)
-	iss: Issuer claim. Who issued the token?
-	sub: Subject claim. Who is the token about?
-	aud: Audience claim. What server is the token for?
-	exp: Epoch Expiration Time. When should I no longer accept this token?
-	nbf: Not Before Claim. When should I start accepting this token? (not really used)
-	iat: Epoch Issed Time. When was this token issued?
-	jti: JWT Id. A unique identifier for this specific token. (not really used)
-	Header: First part of a JWT. Contains the hash algorithm used to create the signature. Base64 Encoded JSON object.
-	Payload: The meat of the token. Contains the claims. Base 64 Encoded JSON object.
-	Signature: Hash of the header concatenated with a period and the payload along with a secret string (key/salt/whatever you prefer).
Here’s an example of a JWT

```eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJpc3MiOiJteWF1dGhzZXJ2ZXIuY29tIiwic3ViIjoiamFrZUBqYWtlLmNvbSIsIm5hbWUiOiJKYWtlIFRlckhhcmsiLCJleHAiOjE1NDMzODE4NjcsImlhdCI6MTU0MzM4MTI2NywiYXVkIjoibXlzaXRlLmNvbSJ9.IioPTPYSUexk3ve1RerN_BXel7n5Ssw7SG__3SolNBU```

Something of note here is that the header and the payload are not encrypted. Anyone can read them simply by Decoding the Base64 string. Here are the header and payload from the above token.
```
Header
{
	“alg”: “HS256”,
	“type”:”JWT”
}

Payload
{
"iss": "myauthserver.com",
"sub": "jake@jake.com",
"name": "Jake TerHark",
"exp": 1543381867,
"iat": 1543381267,
"aud": "mysite.com"
}
```

If you don’t believe me, head on over to jwt.io and past in the token. The secret is “secret”. This is the typical claims used for a JWT. If any of them were to be altered by an attacker who doesn’t know the secret key used in the hash, the token would be rejected. This is because one of the steps to accept a token as valid, the application takes the header, the period in between, and the payload and reruns the hash used to create it, along with the secret key. If the signature on the token is the different that the one computed, it rejects. It also validates that the audience claim is correct. I.E. this token was meant for this application. It also validates that the token is not expired. If any of these checks are failed. The user is redirected to the central auth server to sign in. If they are already authenticated with that server, the server simply redirects back with a token, and the user is oblivious to the fact that they signed in. Again, this is single sign on. Next time you sign into your Blackboard and you see that horribly designed blue screen where you enter your username/password, take a look at what is happening to the URL, you might find it interesting. For more info than you could ever want, check out the [ietf page](https://tools.ietf.org/html/rfc7519) for the official JWT standard. Additionally, check out the OAuth 2.0 specifications [here](https://tools.ietf.org/html/rfc6749)
