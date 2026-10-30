import { execFileSync } from 'node:child_process';
import { mkdtempSync, readFileSync, rmSync } from 'node:fs';
import { tmpdir } from 'node:os';
import { join } from 'node:path';
import { randomBytes } from 'node:crypto';
import { defineConfig } from 'vite';
import react from '@vitejs/plugin-react';

function loadAspNetDevelopmentCertificate() {
  const certificateDirectory = mkdtempSync(join(tmpdir(), 'atomos-vite-https-'));
  const certificatePath = join(certificateDirectory, 'localhost.pfx');
  const password = randomBytes(32).toString('hex');

  try {
    execFileSync('dotnet', [
      'dev-certs',
      'https',
      '--export-path',
      certificatePath,
      '--password',
      password,
    ], { stdio: 'pipe' });

    return { pfx: readFileSync(certificatePath), passphrase: password };
  } catch {
    throw new Error(
      'Unable to export the trusted ASP.NET HTTPS development certificate. Run "dotnet dev-certs https --trust" and retry.',
    );
  } finally {
    rmSync(certificateDirectory, { recursive: true, force: true });
  }
}

export default defineConfig(({ command }) => {
  return {
    plugins: [react()],
    server: {
      host: 'localhost',
      port: 5173,
      strictPort: true,
      ...(command === 'serve' ? { https: loadAspNetDevelopmentCertificate() } : {}),
    },
  };
});
