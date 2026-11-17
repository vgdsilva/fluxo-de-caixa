# Cria (se ainda não existirem) o usuário e o banco de desenvolvimento no Postgres local,
# usando DB_NAME, DB_USER e DB_PASSWORD de apps/api/.env.
#
# Rodar a partir da raiz do repo:  pnpm --filter @fluxo/api db:setup
# O psql pede a senha do superusuário (postgres). Ela não é gravada em lugar nenhum.
#
# O usuário da aplicação NÃO é superusuário: é dono só do banco do app (spec da fase 0).

$ErrorActionPreference = 'Stop'
$apiDir = Split-Path -Parent $PSScriptRoot
$envFile = Join-Path $apiDir '.env'
if (-not (Test-Path $envFile)) { throw "Não achei $envFile. Copie .env.example para .env primeiro." }

$cfg = @{}
foreach ($line in Get-Content $envFile -Encoding utf8) {
  if ($line -match '^\s*([A-Z_]+)\s*=\s*(.*)\s*$') { $cfg[$Matches[1]] = $Matches[2] }
}
foreach ($k in 'DB_NAME', 'DB_USER', 'DB_PASSWORD') {
  if (-not $cfg[$k]) { throw "$k não definido em $envFile" }
}
$dbHost = if ($cfg['DB_HOST']) { $cfg['DB_HOST'] } else { 'localhost' }
$dbPort = if ($cfg['DB_PORT']) { $cfg['DB_PORT'] } else { '5432' }

$psql = (Get-Command psql -ErrorAction SilentlyContinue).Source
if (-not $psql) {
  $psql = Get-ChildItem 'C:\Program Files\PostgreSQL\*\bin\psql.exe' -ErrorAction SilentlyContinue |
    Sort-Object FullName -Descending | Select-Object -First 1 -ExpandProperty FullName
}
if (-not $psql) { throw 'psql não encontrado. Adicione a pasta bin do PostgreSQL ao PATH.' }

$superUser = if ($env:PGSUPERUSER) { $env:PGSUPERUSER } else { 'postgres' }

# A senha é pedida aqui (e não pelo psql, que trava quando roda dentro do pnpm) e passada ao psql
# só pela variável PGPASSWORD deste processo. Ela não é gravada em arquivo nem mostrada na tela.
$secure = Read-Host "Senha do usuario '$superUser' do Postgres em ${dbHost}:${dbPort}" -AsSecureString
$bstr = [Runtime.InteropServices.Marshal]::SecureStringToBSTR($secure)
$env:PGPASSWORD = [Runtime.InteropServices.Marshal]::PtrToStringBSTR($bstr)
[Runtime.InteropServices.Marshal]::ZeroFreeBSTR($bstr)

$sql = @'
SELECT format('CREATE ROLE %I LOGIN PASSWORD %L NOSUPERUSER NOCREATEDB NOCREATEROLE', :'app_user', :'app_password')
WHERE NOT EXISTS (SELECT 1 FROM pg_roles WHERE rolname = :'app_user') \gexec
SELECT format('ALTER ROLE %I PASSWORD %L', :'app_user', :'app_password') \gexec
SELECT format('CREATE DATABASE %I OWNER %I ENCODING ''UTF8''', :'app_db', :'app_user')
WHERE NOT EXISTS (SELECT 1 FROM pg_database WHERE datname = :'app_db') \gexec
'@

$sqlFile = Join-Path ([IO.Path]::GetTempPath()) "fluxo-setup-$([guid]::NewGuid()).sql"
try {
  [IO.File]::WriteAllText($sqlFile, $sql, [Text.UTF8Encoding]::new($false))
  # -w: nunca pedir senha interativamente (usa PGPASSWORD); falha na hora se estiver errada.
  & $psql -h $dbHost -p $dbPort -U $superUser -d postgres -w -v ON_ERROR_STOP=1 `
    -v "app_user=$($cfg['DB_USER'])" -v "app_password=$($cfg['DB_PASSWORD'])" -v "app_db=$($cfg['DB_NAME'])" `
    -q -f $sqlFile
  $code = $LASTEXITCODE
} finally {
  Remove-Item $sqlFile -ErrorAction SilentlyContinue
  Remove-Item Env:PGPASSWORD -ErrorAction SilentlyContinue
}
if ($code -ne 0) { throw "psql terminou com erro ($code). Confira a senha do '$superUser'." }

Write-Host "Pronto: banco '$($cfg['DB_NAME'])' com dono '$($cfg['DB_USER'])'."
Write-Host 'Próximo passo: pnpm dev (as migrations rodam no start) e pnpm --filter @fluxo/api db:seed'
