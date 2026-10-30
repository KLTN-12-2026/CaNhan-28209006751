# Atomos frontend

## Local HTTPS development

The frontend and API use HTTPS on `localhost` during development so the
authentication cookie keeps its production security attributes in the browser.

1. Trust the ASP.NET Core development certificate once:

   ```powershell
   dotnet dev-certs https --trust
   ```

2. Start the frontend:

   ```powershell
   npm run dev
   ```

The dev server is available at `https://localhost:5173` and calls the API at
`https://localhost:7001`. Vite exports the trusted development certificate to a
random temporary PFX, reads it into memory, and immediately deletes the file.
No certificate or private key is stored in the repository.
